use crate::physics::VerletPoint;
use raylib::prelude::*;

impl VerletPoint {
    // Collision detection between two points
    pub fn check_collision(&self, other: &VerletPoint) -> bool {
        if !self.has_collision() || !other.has_collision() {
            return false;
        }

        let self_mesh = self.collision_mesh().unwrap();
        let other_mesh = other.collision_mesh().unwrap();

        let distance_squared = (self.position().x - other.position().x).powi(2)
            + (self.position().y - other.position().y).powi(2);
        let min_distance = self_mesh.radius + other_mesh.radius;

        distance_squared < min_distance.powi(2)
    }

    // Resolve collision between two points
    pub fn resolve_collision(&mut self, other: &mut VerletPoint) {
        if !self.check_collision(other) {
            return;
        }

        let self_mesh = *self.collision_mesh().unwrap();
        let other_mesh = *other.collision_mesh().unwrap();

        // Calculate collision normal
        let dx = other.position().x - self.position().x;
        let dy = other.position().y - self.position().y;
        let distance = (dx * dx + dy * dy).sqrt();

        if distance == 0.0 {
            return; // Avoid division by zero
        }

        let nx = dx / distance;
        let ny = dy / distance;

        // Calculate overlap
        let overlap = (self_mesh.radius + other_mesh.radius) - distance;

        // Separate the points based on their masses
        let total_mass = self.mass() + other.mass();
        let self_separation = overlap * (other.mass() / total_mass);
        let other_separation = overlap * (self.mass() / total_mass);

        // Move points apart
        let mut self_pos = self.position();
        let mut other_pos = other.position();

        self_pos.x -= nx * self_separation;
        self_pos.y -= ny * self_separation;
        other_pos.x += nx * other_separation;
        other_pos.y += ny * other_separation;

        self.set_position(self_pos);
        other.set_position(other_pos);

        // Calculate relative velocity
        let self_velocity = Vector2::new(
            self.position().x - self.previous_position().x,
            self.position().y - self.previous_position().y,
        );
        let other_velocity = Vector2::new(
            other.position().x - other.previous_position().x,
            other.position().y - other.previous_position().y,
        );

        let relative_velocity = Vector2::new(
            other_velocity.x - self_velocity.x,
            other_velocity.y - self_velocity.y,
        );

        // Calculate relative velocity in collision normal direction
        let velocity_along_normal = relative_velocity.x * nx + relative_velocity.y * ny;

        // Do not resolve if velocities are separating
        if velocity_along_normal > 0.0 {
            return;
        }

        // Calculate restitution
        let restitution = (self_mesh.restitution + other_mesh.restitution) * 0.5;

        // Calculate impulse scalar
        let impulse_scalar = -(1.0 + restitution) * velocity_along_normal;
        let impulse_scalar = impulse_scalar / (1.0 / self.mass() + 1.0 / other.mass());

        // Apply impulse
        let impulse = Vector2::new(impulse_scalar * nx, impulse_scalar * ny);

        // Update previous positions to simulate velocity change
        self.apply_impulse_to_previous(Vector2::new(
            impulse.x / self.mass(),
            impulse.y / self.mass(),
        ));
        other.apply_impulse_to_previous(Vector2::new(
            -impulse.x / other.mass(),
            -impulse.y / other.mass(),
        ));
    }
}

// Helper function to resolve collisions between all points in a collection
pub fn resolve_all_collisions(points: &mut [VerletPoint]) {
    for i in 0..points.len() {
        for j in (i + 1)..points.len() {
            if points[i].has_collision() && points[j].has_collision() {
                let (left, right) = points.split_at_mut(j);
                left[i].resolve_collision(&mut right[0]);
            }
        }
    }
}
