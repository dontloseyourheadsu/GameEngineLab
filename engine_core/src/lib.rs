use raylib::prelude::*;

#[derive(Debug, Clone, Copy)]
pub struct VerletPoint {
    position: Vector2,
    previous: Vector2,
    acceleration: Vector2,
}

impl VerletPoint {
    pub fn new(position: Vector2) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
        }
    }

    pub fn update(&mut self, delta_time: f32) {
        let temp = self.position;
        self.position +=
            self.position - self.previous + self.acceleration * delta_time * delta_time;
        self.previous = temp;
        self.acceleration = Vector2::zero();
    }

    pub fn apply_force(&mut self, force: Vector2) {
        self.acceleration += force;
    }

    pub fn position(&self) -> Vector2 {
        self.position
    }
}
