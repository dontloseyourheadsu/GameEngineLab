use engine_core::{
    character::character_2d::Character2D,
    maps::map_2d_model::Map2DModel,
    parsers::maps_parsers::map_2d_loader::load_map_from_json,
    physics::{collisions_2d::simple_collision_body::SimpleCollisionBody, velocity::Velocity},
    rendering::{coloring::texture_source::TextureSource, sprites_2d::sprite_2d::Sprite2D},
};
use rand::Rng;
use rand::seq::SliceRandom;
use raylib::prelude::*;
use std::collections::HashMap;
use std::time::{Duration, Instant};

use crate::clay_filter::apply_clay_filter;
use crate::ghosts::ghost::{GhostBehavior, GhostState};
use crate::ghosts::ghost_spawner::GhostSpawner;
use crate::pacman::{
    controller::handle_input,
    movement::{PacmanEvent, move_pacman},
    pacman::Pacman,
};

const MAP_PATH: &str = "data/map.json";
const UI_HEIGHT: i32 = 60;

pub fn run_game(rl: &mut RaylibHandle, thread: &RaylibThread) {
    let mut map = load_map_from_json(MAP_PATH);

    // Limit map size to 50x50
    if map.data.len() > 50 {
        panic!("Map height exceeds 50 cells!");
    }
    if !map.data.is_empty() && map.data[0].len() > 50 {
        panic!("Map width exceeds 50 cells!");
    }

    let screen_width = map.data[0].len() as i32 * map.tile_size as i32;
    let screen_height = (map.data.len() as i32 * map.tile_size as i32) + UI_HEIGHT;

    rl.set_window_size(screen_width, screen_height);
    rl.set_window_title(&thread, "Pacman-rs - Play");

    let mut textures = HashMap::new();
    for (name, source) in &map.textures {
        match source {
            TextureSource::File(filename) => {
                if !filename.is_empty() {
                    if let Some(symbol) = map.symbols.get(name) {
                        let full_path = format!("{}{}", map.textures_path, filename);

                        // Load with image crate
                        let img = image::open(&full_path).expect("Failed to load image");

                        let img_rgba = img.clone().to_rgba8();

                        // Load normal texture (RGBA)
                        let mut buffer = std::io::Cursor::new(Vec::new());
                        img_rgba
                            .write_to(&mut buffer, image::ImageOutputFormat::Png)
                            .unwrap();
                        let data = buffer.into_inner();
                        let file_extension = ".png";
                        let image = Image::load_image_from_mem(file_extension, &data)
                            .expect("Failed to load image from memory");
                        let texture = rl
                            .load_texture_from_image(&thread, &image)
                            .expect("Failed to create texture");
                        textures.insert(symbol.clone(), texture);

                        // If it's a ghost, also create a filtered version
                        if name == "ghost" {
                            // Filter works on RGB
                            let img_rgb = img.to_rgb8();
                            let processed_img = apply_clay_filter(&img_rgb);
                            let mut buffer = std::io::Cursor::new(Vec::new());
                            processed_img
                                .write_to(&mut buffer, image::ImageOutputFormat::Png)
                                .unwrap();
                            let data = buffer.into_inner();
                            let image = Image::load_image_from_mem(file_extension, &data)
                                .expect("Failed to load image from memory");
                            let texture = rl
                                .load_texture_from_image(&thread, &image)
                                .expect("Failed to create texture");
                            textures.insert(format!("{}_frightened", symbol), texture);
                        }
                    }
                }
            }
            TextureSource::Color(color_tile) => {
                if let Some(symbol) = map.symbols.get(name) {
                    let image = Image::gen_image_color(
                        map.tile_size as i32,
                        map.tile_size as i32,
                        color_tile.color,
                    );
                    let texture = rl
                        .load_texture_from_image(&thread, &image)
                        .expect("Failed to create texture");
                    textures.insert(symbol.clone(), texture);
                }
            }
        }
    }

    let mut pacman_start_pos = Vector2::zero();
    let mut pacman_map_pos = (-1, -1);
    for (y, row) in map.data.iter().enumerate() {
        if let Some(x) = row.find('P') {
            pacman_start_pos.x = (x as i32 * map.tile_size as i32) as f32;
            pacman_start_pos.y = (y as i32 * map.tile_size as i32) as f32;
            pacman_map_pos = (x as i32, y as i32);
            break;
        }
    }

    if pacman_map_pos != (-1, -1) {
        let row = &mut map.data[pacman_map_pos.1 as usize];
        let x = pacman_map_pos.0 as usize;
        row.replace_range(x..x + 1, ".");
    }

    let mut pacman_animation_mapper = HashMap::new();
    pacman_animation_mapper.insert("walk".to_string(), vec![0, 1, 2, 3]);

    let mut pacman = Pacman {
        character: Character2D {
            position: pacman_start_pos,
            velocity: Velocity(Vector2::zero()),
            collision_body: SimpleCollisionBody::Box {
                width: map.tile_size as f32,
                height: map.tile_size as f32,
            },
            sprite: Sprite2D::new(
                256.0,
                256.0,
                Duration::from_millis(100),
                pacman_animation_mapper,
            ),
        },
        current_direction: Vector2::zero(),
        desired_direction: Vector2::zero(),
        grid_position: (pacman_map_pos.0 as usize, pacman_map_pos.1 as usize),
    };
    pacman.character.sprite.animation_state = "walk".to_string();

    // Initialize ghost spawners
    let mut ghost_spawners: Vec<GhostSpawner> = Vec::new();
    let mut s_positions = Vec::new();

    for (y, row) in map.data.iter().enumerate() {
        for (x, ch) in row.chars().enumerate() {
            if ch == 'S' {
                s_positions.push((x, y));
            }
        }
    }

    // Limit to 8 ghosts and shuffle positions
    let mut rng = rand::thread_rng();
    s_positions.shuffle(&mut rng);
    if s_positions.len() > 8 {
        s_positions.truncate(8);
    }

    // Track behavior counts for weighted randomness
    let mut behavior_counts = HashMap::new();
    behavior_counts.insert(GhostBehavior::Blinky, 0);
    behavior_counts.insert(GhostBehavior::Pinky, 0);
    behavior_counts.insert(GhostBehavior::Inky, 0);
    behavior_counts.insert(GhostBehavior::Clyde, 0);

    let behaviors = [
        GhostBehavior::Blinky,
        GhostBehavior::Pinky,
        GhostBehavior::Inky,
        GhostBehavior::Clyde,
    ];

    for (_i, (x, y)) in s_positions.into_iter().enumerate() {
        let spawn_pos = Vector2::new(
            (x as i32 * map.tile_size as i32) as f32,
            (y as i32 * map.tile_size as i32) as f32,
        );
        let mut spawner = GhostSpawner::new(spawn_pos);

        // Pick behavior with weighted probability
        let mut best_behavior = GhostBehavior::Blinky; // Default
        let mut _min_score = f32::MAX;

        let mut candidates = Vec::new();
        for b in &behaviors {
            let count = behavior_counts.get(b).unwrap_or(&0);
            let r: f32 = rng.r#gen();
            let score = *count as f32 + r;
            candidates.push((*b, score));
        }

        candidates.sort_by(|a, b| a.1.partial_cmp(&b.1).unwrap());
        if let Some((b, _)) = candidates.first() {
            best_behavior = *b;
            *behavior_counts.get_mut(&best_behavior).unwrap() += 1;
        }

        spawner.spawn_ghost(map.tile_size as f32, best_behavior);

        if let Some(row) = map.data.get_mut(y) {
            row.replace_range(x..x + 1, "S");
        }

        ghost_spawners.push(spawner);
    }

    // Initialize Game State
    let mut score = 0;
    let mut lives = 3;
    let mut game_over = false;
    let mut win = false;

    // Count total items
    let mut total_items = 0;
    for row in &map.data {
        for ch in row.chars() {
            if ch == '.' || ch == 'o' {
                total_items += 1;
            }
        }
    }
    let mut items_eaten = 0;
    let map_offset = Vector2::new(0.0, UI_HEIGHT as f32);

    let mut last_move_time = Instant::now();
    let move_interval = Duration::from_millis(200);
    let mut previous_time = Instant::now();

    // Global Ghost Wave Timer
    let mut global_mode_timer = 0.0;
    let mut is_scatter_mode = true; // Start with Scatter
    let scatter_duration = 7.0;
    let chase_duration = 20.0;

    // Game loop specifically for the Play scene
    while !rl.window_should_close() {
        // Exit to menu on ESC
        if rl.is_key_pressed(KeyboardKey::KEY_ESCAPE) {
            break;
        }

        let current_time = Instant::now();
        let delta_time = (current_time - previous_time).as_secs_f32();

        if !game_over && !win {
            // Update Global Ghost Mode
            global_mode_timer += delta_time;
            let target_duration = if is_scatter_mode {
                scatter_duration
            } else {
                chase_duration
            };

            if global_mode_timer >= target_duration {
                global_mode_timer = 0.0;
                is_scatter_mode = !is_scatter_mode;
            }

            // Sync ghosts to global mode (unless frightened or waiting)
            for spawner in ghost_spawners.iter_mut() {
                if let Some(ghost) = spawner.ghost.as_mut() {
                    if ghost.state != GhostState::Frightened {
                        ghost.state = if is_scatter_mode {
                            GhostState::Scatter
                        } else {
                            GhostState::Chase
                        };
                    }
                }
            }

            handle_input(rl, &mut pacman);

            // Handle movement on tick
            if last_move_time.elapsed() >= move_interval {
                let mut reset_needed = false;
                let event = move_pacman(&mut pacman, &mut map);

                match event {
                    PacmanEvent::EatenFood => {
                        score += 10;
                        items_eaten += 1;
                    }
                    PacmanEvent::EatenPill => {
                        score += 50;
                        items_eaten += 1;
                        for spawner in ghost_spawners.iter_mut() {
                            if let Some(ref mut ghost) = spawner.ghost {
                                ghost.state = GhostState::Frightened;
                                ghost.frightened_timer = 10.0;
                            }
                        }
                    }
                    _ => {}
                }

                if items_eaten >= total_items {
                    win = true;
                }

                // Check collision after Pacman moves
                for spawner in ghost_spawners.iter_mut() {
                    if let Some(ref mut ghost) = spawner.ghost {
                        if ghost.is_active && ghost.grid_position == pacman.grid_position {
                            if ghost.state == GhostState::Frightened {
                                ghost.respawn();
                                score += 200;
                            } else {
                                lives -= 1;
                                reset_needed = true;
                            }
                        }
                    }
                }

                // Only move ghosts if we didn't just die
                if !reset_needed && !game_over && !win {
                    // Update ghosts
                    for spawner in ghost_spawners.iter_mut() {
                        if let Some(ref mut ghost) = spawner.ghost {
                            ghost.update(
                                delta_time,
                                pacman.character.position,
                                pacman.current_direction,
                                map.tile_size as f32,
                                &mut map,
                            );
                        }
                    }

                    // Check collision after Ghosts move
                    for spawner in ghost_spawners.iter_mut() {
                        if let Some(ref mut ghost) = spawner.ghost {
                            if ghost.is_active && ghost.grid_position == pacman.grid_position {
                                if ghost.state == GhostState::Frightened {
                                    ghost.respawn();
                                    score += 200;
                                } else {
                                    lives -= 1;
                                    reset_needed = true;
                                }
                            }
                        }
                    }
                }

                if reset_needed {
                    if lives <= 0 {
                        game_over = true;
                    } else {
                        // Reset Pacman
                        pacman.grid_position =
                            (pacman_map_pos.0 as usize, pacman_map_pos.1 as usize);
                        pacman.character.position = pacman_start_pos;
                        pacman.current_direction = Vector2::zero();
                        pacman.desired_direction = Vector2::zero();

                        // Reset All Ghosts
                        for spawner in ghost_spawners.iter_mut() {
                            if let Some(ref mut ghost) = spawner.ghost {
                                ghost.respawn();
                            }
                        }
                    }
                }

                last_move_time = Instant::now();
            }
        }

        pacman.character.sprite.update();

        if pacman.current_direction.x > 0.0 {
            // Right
            pacman.character.sprite.flip_horizontal = false;
            pacman.character.sprite.rotation = 0.0;
        } else if pacman.current_direction.x < 0.0 {
            // Left
            pacman.character.sprite.flip_horizontal = true;
            pacman.character.sprite.rotation = 0.0;
        } else if pacman.current_direction.y < 0.0 {
            // Up
            pacman.character.sprite.flip_horizontal = false;
            pacman.character.sprite.rotation = -90.0;
        } else if pacman.current_direction.y > 0.0 {
            // Down
            pacman.character.sprite.flip_horizontal = false;
            pacman.character.sprite.rotation = 90.0;
        }

        let mut d = rl.begin_drawing(thread);

        d.clear_background(Color::BLACK);

        // Draw UI
        d.draw_text(&format!("SCORE: {}", score), 10, 10, 20, Color::WHITE);
        d.draw_text(&format!("LIVES: {}", lives), 200, 10, 20, Color::WHITE);

        if game_over {
            d.draw_text(
                "GAME OVER",
                screen_width / 2 - 100,
                screen_height / 2,
                40,
                Color::RED,
            );
        } else if win {
            d.draw_text(
                "YOU WIN!",
                screen_width / 2 - 100,
                screen_height / 2,
                40,
                Color::GREEN,
            );
        }

        draw_map(&mut d, &map, &textures, map_offset);

        let pacman_symbol = map.symbols.get("pacman").unwrap();
        let pacman_texture = textures.get(pacman_symbol).unwrap();

        pacman.character.sprite.draw(
            &mut d,
            pacman_texture,
            pacman.character.position + map_offset,
            map.tile_size as f32,
        );

        // Draw ghosts
        let ghost_symbol = map.symbols.get("ghost").unwrap();
        let ghost_texture_normal = textures.get(ghost_symbol).unwrap();
        let ghost_texture_frightened = textures
            .get(&format!("{}_frightened", ghost_symbol))
            .unwrap_or(ghost_texture_normal);

        for spawner in &ghost_spawners {
            if let Some(ref ghost) = spawner.ghost {
                if ghost.is_active {
                    let mut current_texture = ghost_texture_normal;

                    if ghost.state == GhostState::Frightened {
                        if ghost.frightened_timer > 3.0 {
                            current_texture = ghost_texture_frightened;
                        } else {
                            // Blink in the last 3 seconds
                            // Blink frequency: every 0.2 seconds (5 times per second)
                            if (ghost.frightened_timer * 5.0) as i32 % 2 == 0 {
                                current_texture = ghost_texture_frightened;
                            }
                        }
                    }

                    let source_rec = Rectangle::new(
                        0.0,
                        0.0,
                        current_texture.width() as f32,
                        current_texture.height() as f32,
                    );
                    let dest_rec = Rectangle::new(
                        ghost.character.position.x + map_offset.x,
                        ghost.character.position.y + map_offset.y,
                        map.tile_size as f32,
                        map.tile_size as f32,
                    );
                    d.draw_texture_pro(
                        current_texture,
                        source_rec,
                        dest_rec,
                        Vector2::new(0.0, 0.0),
                        0.0,
                        Color::WHITE,
                    );
                }
            }
        }
        previous_time = current_time;
    }
}

fn draw_map(
    d: &mut RaylibDrawHandle,
    map: &Map2DModel,
    textures: &HashMap<String, Texture2D>,
    offset: Vector2,
) {
    let empty_symbol = map
        .symbols
        .get("empty")
        .unwrap_or(&" ".to_string())
        .to_string();
    let wall_symbol = map
        .symbols
        .get("wall")
        .unwrap_or(&"#".to_string())
        .to_string();
    let spawner_symbol = map
        .symbols
        .get("spawner")
        .unwrap_or(&"S".to_string())
        .to_string();

    // Layer 0: Draw Empty everywhere to handle background transparency
    if let Some(empty_texture) = textures.get(&empty_symbol) {
        for y in 0..map.data.len() {
            let row_len = map.data[y].len();
            for x in 0..row_len {
                let source_rec = Rectangle::new(
                    0.0,
                    0.0,
                    empty_texture.width() as f32,
                    empty_texture.height() as f32,
                );
                let dest_rec = Rectangle::new(
                    offset.x + (x as i32 * map.tile_size as i32) as f32,
                    offset.y + (y as i32 * map.tile_size as i32) as f32,
                    map.tile_size as f32,
                    map.tile_size as f32,
                );
                d.draw_texture_pro(
                    empty_texture,
                    source_rec,
                    dest_rec,
                    Vector2::new(0.0, 0.0),
                    0.0,
                    Color::WHITE,
                );
            }
        }
    }

    // Iterate map data for logic layers
    for (y, row) in map.data.iter().enumerate() {
        for (x, char) in row.chars().enumerate() {
            let char_str = char.to_string();

            // Skip P, G as they are entities.
            if char == 'P' || char == 'G' {
                continue;
            }

            // Layer 1: Wall
            if char_str == wall_symbol {
                if let Some(texture) = textures.get(&char_str) {
                    draw_tile(d, texture, x, y, map.tile_size, offset);
                }
            }
            // Layer 2: Spawner
            else if char_str == spawner_symbol {
                if let Some(texture) = textures.get(&char_str) {
                    draw_tile(d, texture, x, y, map.tile_size, offset);
                }
            }
            // Layer 3: Items (Food, Pill, etc) - basically everything else that is not empty/wall/spawner
            // map.json says food is 'o', pill is '*', path is '.'
            // But logic calls '.' -> Food. 'o' -> Pill.
            else if char_str != empty_symbol {
                if let Some(texture) = textures.get(&char_str) {
                    draw_tile(d, texture, x, y, map.tile_size, offset);
                }
            }
        }
    }
}

fn draw_tile(
    d: &mut RaylibDrawHandle,
    texture: &Texture2D,
    x: usize,
    y: usize,
    tile_size: u8,
    offset: Vector2,
) {
    let source_rec = Rectangle::new(0.0, 0.0, texture.width() as f32, texture.height() as f32);
    let dest_rec = Rectangle::new(
        offset.x + (x as i32 * tile_size as i32) as f32,
        offset.y + (y as i32 * tile_size as i32) as f32,
        tile_size as f32,
        tile_size as f32,
    );
    d.draw_texture_pro(
        texture,
        source_rec,
        dest_rec,
        Vector2::new(0.0, 0.0),
        0.0,
        Color::WHITE,
    );
}
