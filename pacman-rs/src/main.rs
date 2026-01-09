use engine_core::{
    character::character_2d::Character2D,
    maps::map_2d_model::Map2DModel,
    parsers::maps_parsers::map_2d_loader::load_map_from_json,
    physics::{collisions_2d::simple_collision_body::SimpleCollisionBody, velocity::Velocity},
    rendering::{coloring::texture_source::TextureSource, sprites_2d::sprite_2d::Sprite2D},
};
use raylib::prelude::*;
use std::collections::HashMap;
use std::time::{Duration, Instant};

mod clay_filter;
use clay_filter::apply_clay_filter;

mod ghosts;
mod pacman;

use crate::ghosts::ghost::GhostState;
use crate::ghosts::ghost_spawner::GhostSpawner;
use crate::pacman::{
    controller::handle_input,
    movement::{PacmanEvent, move_pacman},
    pacman::Pacman,
};

const MAP_PATH: &str = "data/map.json";

fn main() {
    let mut map = load_map_from_json(MAP_PATH);

    let screen_width = map.data[0].len() as i32 * map.tile_size as i32;
    let screen_height = map.data.len() as i32 * map.tile_size as i32;

    let (mut rl, thread) = raylib::init()
        .size(screen_width, screen_height)
        .title("Pacman-rs")
        .build();

    let mut textures = HashMap::new();
    for (name, source) in &map.textures {
        match source {
            TextureSource::File(filename) => {
                if !filename.is_empty() {
                    if let Some(symbol) = map.symbols.get(name) {
                        let full_path = format!("{}{}", map.textures_path, filename);

                        // Load with image crate
                        let img = image::open(&full_path)
                            .expect("Failed to load image")
                            .to_rgb8();

                        // Load normal texture
                        let mut buffer = std::io::Cursor::new(Vec::new());
                        img.write_to(&mut buffer, image::ImageOutputFormat::Png)
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
                            let processed_img = apply_clay_filter(&img);
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

    // Set Pacman on map
    if let Some(row) = map.data.get_mut(pacman.grid_position.1) {
        row.replace_range(pacman.grid_position.0..pacman.grid_position.0 + 1, "P");
    }

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

    for (x, y) in s_positions {
        let spawn_pos = Vector2::new(
            (x as i32 * map.tile_size as i32) as f32,
            (y as i32 * map.tile_size as i32) as f32,
        );
        let mut spawner = GhostSpawner::new(spawn_pos);
        spawner.spawn_ghost(map.tile_size as f32);

        // Initialize ghost on map
        if let Some(ref mut ghost) = spawner.ghost {
            // Replace 'S' with ' ' (empty space) effectively, but store it as ' '
            // We assume 'S' is just a spawn point and becomes empty.
            ghost.stored_tile = 'S';

            // Update map to 'G'
            if let Some(row) = map.data.get_mut(y) {
                row.replace_range(x..x + 1, "G");
            }
        }

        ghost_spawners.push(spawner);
    }

    let mut last_move_time = Instant::now();
    let move_interval = Duration::from_millis(200);
    let mut previous_time = Instant::now();

    while !rl.window_should_close() {
        let current_time = Instant::now();
        let delta_time = (current_time - previous_time).as_secs_f32();

        handle_input(&rl, &mut pacman);

        // Handle movement on tick
        if last_move_time.elapsed() >= move_interval {
            let event = move_pacman(&mut pacman, &mut map);

            if let PacmanEvent::EatenPill = event {
                for spawner in ghost_spawners.iter_mut() {
                    if let Some(ref mut ghost) = spawner.ghost {
                        ghost.state = GhostState::Frightened;
                        ghost.frightened_timer = 10.0;
                    }
                }
            }

            // Update ghosts
            for spawner in ghost_spawners.iter_mut() {
                if let Some(ref mut ghost) = spawner.ghost {
                    ghost.update(
                        delta_time,
                        pacman.character.position,
                        map.tile_size as f32,
                        &mut map,
                    );
                }
            }

            last_move_time = Instant::now();
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

        let mut d = rl.begin_drawing(&thread);

        d.clear_background(Color::BLACK);

        draw_map(&mut d, &map, &textures);

        let pacman_symbol = map.symbols.get("pacman").unwrap();
        let pacman_texture = textures.get(pacman_symbol).unwrap();

        pacman.character.sprite.draw(
            &mut d,
            pacman_texture,
            pacman.character.position,
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
                        ghost.character.position.x,
                        ghost.character.position.y,
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

fn draw_map(d: &mut RaylibDrawHandle, map: &Map2DModel, textures: &HashMap<String, Texture2D>) {
    for (y, row) in map.data.iter().enumerate() {
        for (x, char) in row.chars().enumerate() {
            if char == 'P' || char == 'G' {
                continue;
            }
            let char_str = char.to_string();
            if let Some(texture) = textures.get(&char_str) {
                let source_rec =
                    Rectangle::new(0.0, 0.0, texture.width() as f32, texture.height() as f32);
                let dest_rec = Rectangle::new(
                    (x as i32 * map.tile_size as i32) as f32,
                    (y as i32 * map.tile_size as i32) as f32,
                    map.tile_size as f32,
                    map.tile_size as f32,
                );
                d.draw_texture_pro(
                    texture,
                    source_rec,
                    dest_rec,
                    Vector2::new(0.0, 0.0), // origin
                    0.0,                    // rotation
                    Color::WHITE,
                );
            }
        }
    }
}
