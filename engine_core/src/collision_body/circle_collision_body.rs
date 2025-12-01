use super::collision_body::CollisionBody;
use raylib::prelude::*;

pub struct CircleCollisionBody {
    pub position: Vector2,
    pub old_position: Vector2,
    pub radius: f32,
    pub damping: f32,
}

impl CircleCollisionBody {
    pub fn new(position: Vector2, radius: f32, damping: f32) -> Self {
        Self {
            position,
            old_position: position,
            radius,
            damping,
        }
    }
}

impl CollisionBody for CircleCollisionBody {
    fn update(
        &mut self,
        dt: f32,
        gravity: Vector2,
        friction: f32,
        screen_width: f32,
        screen_height: f32,
    ) {
        let velocity = self.position - self.old_position;

        // Apply friction
        let velocity = velocity * (1.0 - friction);

        self.old_position = self.position;
        self.position = self.position + velocity + gravity * dt * dt;

        // World boundaries
        // Left
        if self.position.x - self.radius < 0.0 {
            let vx = self.position.x - self.old_position.x;
            self.position.x = self.radius;
            self.old_position.x = self.position.x + vx * self.damping;
        }
        // Right
        if self.position.x + self.radius > screen_width {
            let vx = self.position.x - self.old_position.x;
            self.position.x = screen_width - self.radius;
            self.old_position.x = self.position.x + vx * self.damping;
        }
        // Top
        if self.position.y - self.radius < 0.0 {
            let vy = self.position.y - self.old_position.y;
            self.position.y = self.radius;
            self.old_position.y = self.position.y + vy * self.damping;
        }
        // Bottom
        if self.position.y + self.radius > screen_height {
            let vy = self.position.y - self.old_position.y;
            self.position.y = screen_height - self.radius;
            self.old_position.y = self.position.y + vy * self.damping;
        }
    }

    fn get_position(&self) -> Vector2 {
        self.position
    }
}
