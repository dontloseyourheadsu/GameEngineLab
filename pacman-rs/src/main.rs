use raylib::prelude::*;
use engine_core::{
    character::character_2d::Character2D,
    maps::map_2d_loader::load_map_from_json,
    physics::{
        collisions_2d::simple_collision_body::SimpleCollisionBody,
        velocity::Velocity,
    },
    rendering::sprites_2d::sprite_2d::Sprite2D,
};
use std::collections::HashMap;
use std::time::{Duration, Instant};

mod pacman;
use crate::pacman::{pacman::Pacman, controller::handle_input, movement::move_pacman};


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
    for (name, filename) in &map.textures {
        if !filename.is_empty() {
            if let Some(symbol) = map.symbols.get(name) {
                let full_path = format!("{}{}", map.textures_path, filename);
                let texture = rl.load_texture(&thread, &full_path).unwrap();
                textures.insert(symbol.clone(), texture);
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
        row.replace_range(x..x+1, ".");
    }
    
    let mut pacman = Pacman {
        character: Character2D {
            position: pacman_start_pos,
            velocity: Velocity(Vector2::zero()),
            collision_body: SimpleCollisionBody::Box {
                width: map.tile_size as f32,
                height: map.tile_size as f32,
            },
            sprite: Sprite2D {
                animation_state: "idle".to_string(),
                animation_frame: 0,
                animation_mapper: HashMap::new(),
            },
        },
        current_direction: Vector2::zero(),
        desired_direction: Vector2::zero(),
    };

    let mut last_move_time = Instant::now();
    let move_interval = Duration::from_millis(200);

    while !rl.window_should_close() {
        handle_input(&rl, &mut pacman);

        if last_move_time.elapsed() >= move_interval {
            move_pacman(&mut pacman, &map);
            last_move_time = Instant::now();
        }

        let mut d = rl.begin_drawing(&thread);

        d.clear_background(Color::BLACK);

        for (y, row) in map.data.iter().enumerate() {
            for (x, char) in row.chars().enumerate() {
                let char_str = char.to_string();
                if let Some(texture) = textures.get(&char_str) {
                    let source_rec = Rectangle::new(0.0, 0.0, texture.width() as f32, texture.height() as f32);
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
                        0.0, // rotation
                        Color::WHITE,
                    );
                }
            }
        }

        let pacman_symbol = map.symbols.get("pacman").unwrap();
        let pacman_texture = textures.get(pacman_symbol).unwrap();
        let source_rec = Rectangle::new(0.0, 0.0, pacman_texture.width() as f32, pacman_texture.height() as f32);
        let dest_rec = Rectangle::new(
            pacman.character.position.x,
            pacman.character.position.y,
            map.tile_size as f32,
            map.tile_size as f32,
        );
        d.draw_texture_pro(
            pacman_texture,
            source_rec,
            dest_rec,
            Vector2::new(0.0, 0.0), // origin
            0.0, // rotation
            Color::WHITE,
        );
    }
}
