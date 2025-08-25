use raylib::prelude::*;

#[derive(Debug, Clone, Copy)]
pub struct CollisionMesh {
    pub radius: f32,
    pub restitution: f32, // Bounciness factor (0.0 = no bounce, 1.0 = perfect bounce)
    pub enabled: bool,
}

impl CollisionMesh {
    pub fn new(radius: f32) -> Self {
        CollisionMesh {
            radius,
            restitution: 0.8,
            enabled: true,
        }
    }

    pub fn new_with_restitution(radius: f32, restitution: f32) -> Self {
        CollisionMesh {
            radius,
            restitution,
            enabled: true,
        }
    }
}

#[derive(Debug, Clone, Copy)]
pub struct VerletPoint {
    position: Vector2,
    previous: Vector2,
    acceleration: Vector2,
    mass: f32,
    size: f32,
    color: Color,
    collision_mesh: Option<CollisionMesh>,
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
            collision_mesh: None,
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
            collision_mesh: None,
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
            collision_mesh: None,
        }
    }

    pub fn new_with_collision(
        position: Vector2,
        mass: f32,
        size: f32,
        color: Color,
        collision_mesh: CollisionMesh,
    ) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            size,
            color,
            collision_mesh: Some(collision_mesh),
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

    pub fn collision_mesh(&self) -> Option<&CollisionMesh> {
        self.collision_mesh.as_ref()
    }

    pub fn set_collision_mesh(&mut self, collision_mesh: Option<CollisionMesh>) {
        self.collision_mesh = collision_mesh;
    }

    pub fn has_collision(&self) -> bool {
        self.collision_mesh.is_some() && self.collision_mesh.unwrap().enabled
    }

    // Collision detection between two points
    pub fn check_collision(&self, other: &VerletPoint) -> bool {
        if !self.has_collision() || !other.has_collision() {
            return false;
        }

        let self_mesh = self.collision_mesh.unwrap();
        let other_mesh = other.collision_mesh.unwrap();

        let distance_squared = (self.position.x - other.position.x).powi(2)
            + (self.position.y - other.position.y).powi(2);
        let min_distance = self_mesh.radius + other_mesh.radius;

        distance_squared < min_distance.powi(2)
    }

    // Resolve collision between two points
    pub fn resolve_collision(&mut self, other: &mut VerletPoint) {
        if !self.check_collision(other) {
            return;
        }

        let self_mesh = self.collision_mesh.unwrap();
        let other_mesh = other.collision_mesh.unwrap();

        // Calculate collision normal
        let dx = other.position.x - self.position.x;
        let dy = other.position.y - self.position.y;
        let distance = (dx * dx + dy * dy).sqrt();

        if distance == 0.0 {
            return; // Avoid division by zero
        }

        let nx = dx / distance;
        let ny = dy / distance;

        // Calculate overlap
        let overlap = (self_mesh.radius + other_mesh.radius) - distance;

        // Separate the points based on their masses
        let total_mass = self.mass + other.mass;
        let self_separation = overlap * (other.mass / total_mass);
        let other_separation = overlap * (self.mass / total_mass);

        // Move points apart
        self.position.x -= nx * self_separation;
        self.position.y -= ny * self_separation;
        other.position.x += nx * other_separation;
        other.position.y += ny * other_separation;

        // Calculate relative velocity
        let self_velocity = Vector2::new(
            self.position.x - self.previous.x,
            self.position.y - self.previous.y,
        );
        let other_velocity = Vector2::new(
            other.position.x - other.previous.x,
            other.position.y - other.previous.y,
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
        let impulse_scalar = impulse_scalar / (1.0 / self.mass + 1.0 / other.mass);

        // Apply impulse
        let impulse = Vector2::new(impulse_scalar * nx, impulse_scalar * ny);

        // Update previous positions to simulate velocity change
        self.previous.x += impulse.x / self.mass;
        self.previous.y += impulse.y / self.mass;
        other.previous.x -= impulse.x / other.mass;
        other.previous.y -= impulse.y / other.mass;
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
