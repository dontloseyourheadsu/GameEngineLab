use super::ghost::{Ghost, GhostBehavior, GhostState};
use engine_core::{
    character::character_2d::Character2D,
    physics::{collisions_2d::simple_collision_body::SimpleCollisionBody, velocity::Velocity},
    rendering::sprites_2d::sprite_2d::Sprite2D,
};
use raylib::math::Vector2;
use std::collections::{HashMap, VecDeque};
use std::time::Duration;

pub struct GhostSpawner {
    pub spawn_position: Vector2,
    pub ghost: Option<Ghost>,
}

impl GhostSpawner {
    pub fn new(position: Vector2) -> Self {
        GhostSpawner {
            spawn_position: position,
            ghost: None,
        }
    }

    pub fn spawn_ghost(&mut self, tile_size: f32, behavior: GhostBehavior) {
        if self.ghost.is_some() {
            return; // Already spawned
        }

        let mut ghost_animation_mapper = HashMap::new();
        ghost_animation_mapper.insert("idle".to_string(), vec![0]);

        let ghost = Ghost {
            character: Character2D {
                position: self.spawn_position,
                velocity: Velocity(Vector2::zero()),
                collision_body: SimpleCollisionBody::Box {
                    width: tile_size,
                    height: tile_size,
                },
                sprite: Sprite2D::new(
                    256.0,
                    256.0,
                    Duration::from_millis(100),
                    ghost_animation_mapper,
                ),
            },
            target_position: Vector2::zero(),
            is_active: true,
            grid_position: (
                (self.spawn_position.x / tile_size) as usize,
                (self.spawn_position.y / tile_size) as usize,
            ),
            state: GhostState::Chase,
            frightened_timer: 0.0,
            spawn_position: self.spawn_position,
            behavior,
            current_direction: (0, 0),
            is_moving: false,
            move_progress: 0.0,
            previous_grid_position: (
                (self.spawn_position.x / tile_size) as usize,
                (self.spawn_position.y / tile_size) as usize,
            ),
            next_grid_position: (
                (self.spawn_position.x / tile_size) as usize,
                (self.spawn_position.y / tile_size) as usize,
            ),
            return_path: VecDeque::new(),
        };

        self.ghost = Some(ghost);
    }
}
