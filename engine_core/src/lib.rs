use raylib::prelude::*;

#[derive(Debug, Clone, Copy)]
pub struct VerletPoint {
    position: Vector2,
    previous: Vector2,
    acceleration: Vector2,
    mass: f32,
    size: f32,
    color: Color,
}

impl VerletPoint {
    pub fn new(position: Vector2) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass: 1.0,
            size: 5.0,
            color: Color::WHITE,
        }
    }

    pub fn new_with_properties(position: Vector2, mass: f32, size: f32) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            size,
            color: Color::WHITE,
        }
    }

    pub fn new_full(position: Vector2, mass: f32, size: f32, color: Color) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            size,
            color,
        }
    }

    pub fn update(&mut self, delta_time: f32, world_bounds: Rectangle) {
        let temp = self.position;
        self.position +=
            self.position - self.previous + self.acceleration * delta_time * delta_time;
        self.previous = temp;
        self.acceleration = Vector2::zero();

        // Apply world bounds constraints
        if self.position.x - self.size < world_bounds.x {
            self.position.x = world_bounds.x + self.size;
        } else if self.position.x + self.size > world_bounds.x + world_bounds.width {
            self.position.x = world_bounds.x + world_bounds.width - self.size;
        }

        if self.position.y - self.size < world_bounds.y {
            self.position.y = world_bounds.y + self.size;
        } else if self.position.y + self.size > world_bounds.y + world_bounds.height {
            self.position.y = world_bounds.y + world_bounds.height - self.size;
        }
    }

    pub fn apply_force(&mut self, force: Vector2) {
        // F = ma, so a = F/m
        self.acceleration += force / self.mass;
    }

    pub fn position(&self) -> Vector2 {
        self.position
    }

    pub fn mass(&self) -> f32 {
        self.mass
    }

    pub fn size(&self) -> f32 {
        self.size
    }

    pub fn color(&self) -> Color {
        self.color
    }

    pub fn set_mass(&mut self, mass: f32) {
        self.mass = mass;
    }

    pub fn set_size(&mut self, size: f32) {
        self.size = size;
    }

    pub fn set_color(&mut self, color: Color) {
        self.color = color;
    }
}
