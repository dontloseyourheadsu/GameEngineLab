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

    pub fn disable(&mut self) {
        self.enabled = false;
    }

    pub fn enable(&mut self) {
        self.enabled = true;
    }
}
