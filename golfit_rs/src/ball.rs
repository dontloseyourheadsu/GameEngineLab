use raylib::prelude::*;
use crate::vector::Vector2D;
use crate::verlet::Verlet;
use crate::map::Map;
use crate::collision::Collision;

pub struct Ball {
    pub cell_size: i32,
    pub position: Vector2D,
    pub old_position: Vector2D,
    velocity: Vector2D,
    acceleration: Vector2D,
    push_velocity: Vector2D,
    friction: f32,
    pub radius: f32,
    pub mass: f32,
    is_pinned: bool,
}

impl Ball {
    pub fn new(cell_size: i32, position: Vector2D, velocity: Vector2D) -> Self {
        Self {
            cell_size,
            position,
            old_position: position,
            velocity,
            acceleration: Vector2D::zero(),
            push_velocity: Vector2D::zero(),
            friction: 0.99,
            radius: (cell_size / 2) as f32,
            mass: 1.0,
            is_pinned: false,
        }
    }

    pub fn render(&self, d: &mut RaylibDrawHandle) {
        let border_radius = 2.0;
        let x = self.position.x - self.cell_size as f32 / 2.0;
        let y = self.position.y - self.cell_size as f32 / 2.0;
        let size = self.cell_size as f32;

        // Draw border (black circle)
        d.draw_circle(
            self.position.x as i32,
            self.position.y as i32,
            self.radius + border_radius,
            Color::BLACK,
        );

        // Draw main ball (white circle)
        d.draw_circle(
            self.position.x as i32,
            self.position.y as i32,
            self.radius,
            Color::WHITE,
        );

        // Draw gray quarter circle for 3D effect
        d.draw_circle_sector(
            Vector2::new(self.position.x, self.position.y),
            self.radius,
            0.0,
            90.0,
            16,
            Color::GRAY,
        );

        // Draw inner white circle
        d.draw_circle(
            (self.position.x + border_radius / 2.0) as i32,
            (self.position.y + border_radius / 2.0) as i32,
            self.radius - border_radius,
            Color::WHITE,
        );
    }

    pub fn push_ball(&mut self, push_velocity: Vector2D) {
        self.push_velocity = push_velocity;
    }

    pub fn is_moving(&self) -> bool {
        let current_velocity = self.position - self.old_position;
        let speed_threshold = 0.01;
        current_velocity.length() > speed_threshold
    }

    pub fn can_enter_goal(&self) -> bool {
        let current_velocity = self.position - self.old_position;
        let speed_threshold = 0.5;
        current_velocity.length() < speed_threshold
    }

    fn check_wall_collision(&self, map: &Map) -> Collision {
        let radius = self.cell_size as f32 / 2.0 - 3.0;

        let top = map.is_wall(self.position.x as i32, (self.position.y - radius) as i32);
        let bottom = map.is_wall(self.position.x as i32, (self.position.y + radius) as i32);
        let left = map.is_wall((self.position.x - radius) as i32, self.position.y as i32);
        let right = map.is_wall((self.position.x + radius) as i32, self.position.y as i32);
        let top_left = map.is_wall((self.position.x - radius) as i32, (self.position.y - radius) as i32);
        let top_right = map.is_wall((self.position.x + radius) as i32, (self.position.y - radius) as i32);
        let bottom_left = map.is_wall((self.position.x - radius) as i32, (self.position.y + radius) as i32);
        let bottom_right = map.is_wall((self.position.x + radius) as i32, (self.position.y + radius) as i32);

        if top { return Collision::Top; }
        if bottom { return Collision::Bottom; }
        if left { return Collision::Left; }
        if right { return Collision::Right; }
        if top_left { return Collision::TopLeft; }
        if top_right { return Collision::TopRight; }
        if bottom_left { return Collision::BottomLeft; }
        if bottom_right { return Collision::BottomRight; }

        Collision::None
    }

    fn check_sand_collision(&self, map: &Map) -> bool {
        let radius = self.cell_size as f32 / 2.0 + 2.0;

        let top = map.is_sand(self.position.x as i32, (self.position.y - radius) as i32);
        let bottom = map.is_sand(self.position.x as i32, (self.position.y + radius) as i32);
        let left = map.is_sand((self.position.x - radius) as i32, self.position.y as i32);
        let right = map.is_sand((self.position.x + radius) as i32, self.position.y as i32);
        let top_left = map.is_sand((self.position.x - radius) as i32, (self.position.y - radius) as i32);
        let top_right = map.is_sand((self.position.x + radius) as i32, (self.position.y - radius) as i32);
        let bottom_left = map.is_sand((self.position.x - radius) as i32, (self.position.y + radius) as i32);
        let bottom_right = map.is_sand((self.position.x + radius) as i32, (self.position.y + radius) as i32);

        top || bottom || left || right || top_left || top_right || bottom_left || bottom_right
    }
}

impl Verlet for Ball {
    fn update(&mut self, d: &mut RaylibDrawHandle, map: &Map) {
        if !self.is_pinned {
            // Verlet integration
            self.acceleration = self.push_velocity / self.mass;
            self.push_velocity = Vector2D::zero();

            let new_position = self.position + (self.position - self.old_position) + self.acceleration;
            self.old_position = self.position;
            self.position = new_position;

            self.velocity = self.position - self.old_position;

            // Check sand collision for friction
            let sand_collision = self.check_sand_collision(map);
            if sand_collision {
                self.friction = 0.9;
            } else {
                self.friction = 0.99;
            }

            // Apply friction
            self.velocity = self.velocity * self.friction;

            // Check canvas boundaries
            if self.position.x < 8.0 + self.radius || self.position.x > (WINDOW_WIDTH - 8) as f32 - self.radius {
                self.velocity.x = -self.velocity.x;
            }
            if self.position.y < 34.0 + self.radius || self.position.y > (WINDOW_HEIGHT - 8) as f32 - self.radius {
                self.velocity.y = -self.velocity.y;
            }

            // Keep ball within bounds
            self.position.x = self.position.x.clamp(8.0 + self.radius, (WINDOW_WIDTH - 8) as f32 - self.radius);
            self.position.y = self.position.y.clamp(34.0 + self.radius, (WINDOW_HEIGHT - 8) as f32 - self.radius);

            // Check wall collisions
            let wall_collision = self.check_wall_collision(map);
            match wall_collision {
                Collision::TopLeft | Collision::TopRight | Collision::BottomLeft | Collision::BottomRight => {
                    self.velocity = -self.velocity;
                }
                Collision::Top => {
                    self.velocity.y = self.velocity.y.abs();
                }
                Collision::Bottom => {
                    self.velocity.y = -self.velocity.y.abs();
                }
                Collision::Left => {
                    self.velocity.x = self.velocity.x.abs();
                }
                Collision::Right => {
                    self.velocity.x = -self.velocity.x.abs();
                }
                _ => {}
            }

            // Update position with new velocity
            self.position = self.old_position + self.velocity;
        }

        self.render(d);
    }
}

// Import constants from main.rs (we'll need to make these available)
use crate::{WINDOW_WIDTH, WINDOW_HEIGHT};
