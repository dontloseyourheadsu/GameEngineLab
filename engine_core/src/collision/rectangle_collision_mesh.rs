use raylib::prelude::*;

#[derive(Debug, Clone, Copy)]
pub struct RectangleCollisionMesh {
    pub width: f32,
    pub height: f32,
    pub rotation: f32,    // Rotation in radians
    pub restitution: f32, // Bounciness factor (0.0 = no bounce, 1.0 = perfect bounce)
    pub enabled: bool,
    pub can_rotate: bool, // Whether this mesh can rotate during collisions
}

impl RectangleCollisionMesh {
    pub fn new(width: f32, height: f32) -> Self {
        RectangleCollisionMesh {
            width,
            height,
            rotation: 0.0,
            restitution: 0.8,
            enabled: true,
            can_rotate: false,
        }
    }

    pub fn new_with_restitution(width: f32, height: f32, restitution: f32) -> Self {
        RectangleCollisionMesh {
            width,
            height,
            rotation: 0.0,
            restitution,
            enabled: true,
            can_rotate: false,
        }
    }

    pub fn new_rotatable(width: f32, height: f32, can_rotate: bool) -> Self {
        RectangleCollisionMesh {
            width,
            height,
            rotation: 0.0,
            restitution: 0.8,
            enabled: true,
            can_rotate,
        }
    }

    pub fn new_full(
        width: f32,
        height: f32,
        rotation: f32,
        restitution: f32,
        can_rotate: bool,
    ) -> Self {
        RectangleCollisionMesh {
            width,
            height,
            rotation,
            restitution,
            enabled: true,
            can_rotate,
        }
    }

    pub fn disable(&mut self) {
        self.enabled = false;
    }

    pub fn enable(&mut self) {
        self.enabled = true;
    }

    pub fn set_rotation(&mut self, rotation: f32) {
        self.rotation = rotation;
    }

    pub fn rotate(&mut self, delta_rotation: f32) {
        if self.can_rotate {
            self.rotation += delta_rotation;
            // Keep rotation between 0 and 2π
            while self.rotation < 0.0 {
                self.rotation += 2.0 * std::f32::consts::PI;
            }
            while self.rotation >= 2.0 * std::f32::consts::PI {
                self.rotation -= 2.0 * std::f32::consts::PI;
            }
        }
    }

    // Get the four corners of the rectangle relative to center
    pub fn get_corners(&self, center: Vector2) -> [Vector2; 4] {
        let half_width = self.width / 2.0;
        let half_height = self.height / 2.0;

        let cos_r = self.rotation.cos();
        let sin_r = self.rotation.sin();

        // Calculate the corners relative to center
        let corners = [
            Vector2::new(-half_width, -half_height), // Top-left
            Vector2::new(half_width, -half_height),  // Top-right
            Vector2::new(half_width, half_height),   // Bottom-right
            Vector2::new(-half_width, half_height),  // Bottom-left
        ];

        // Rotate corners around center
        corners.map(|corner| {
            Vector2::new(
                center.x + corner.x * cos_r - corner.y * sin_r,
                center.y + corner.x * sin_r + corner.y * cos_r,
            )
        })
    }

    // Get the axes for Separated Axis Theorem (SAT)
    pub fn get_axes(&self) -> [Vector2; 2] {
        let cos_r = self.rotation.cos();
        let sin_r = self.rotation.sin();

        [
            Vector2::new(cos_r, sin_r),  // X-axis of the rectangle
            Vector2::new(-sin_r, cos_r), // Y-axis of the rectangle
        ]
    }
}
