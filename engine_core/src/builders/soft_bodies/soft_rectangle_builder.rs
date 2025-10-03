use nalgebra::{Vector2, vector};
use rapier2d::prelude::*;

use crate::builders::rigid_bodies::circle_rigid_body_builder::{
    CircleRigidBodyBuilder, RigidBodyType,
};
use crate::builders::rigid_bodies::spring_builder::SpringBuilder;
use crate::physics::physics_world::PhysicsWorld;

/// Handle to a soft rectangle created in the world for later rendering/management
#[derive(Clone, Copy, Debug)]
pub struct SoftRectangleHandle(pub usize);

/// Runtime instance data stored inside PhysicsWorld
#[derive(Clone, Debug)]
pub struct SoftRectangleInstance {
    pub corners: [RigidBodyHandle; 4],
    pub color: (u8, u8, u8, u8),
}

/// Builder for a soft rectangle made of 4 masses connected by springs
#[derive(Clone, Debug)]
pub struct SoftRectangleBuilder {
    position: Vector2<f32>,
    size: Vector2<f32>,
    body_type: RigidBodyType,
    mass_radius: f32,
    friction: f32,
    stiffness: f32,
    damping: f32,
    color: (u8, u8, u8, u8),
}

impl SoftRectangleBuilder {
    pub fn new(position: Vector2<f32>, size: Vector2<f32>) -> Self {
        Self {
            position,
            size,
            body_type: RigidBodyType::Dynamic,
            mass_radius: 0.6,
            friction: 0.7,
            stiffness: 800.0,
            damping: 30.0,
            color: (30, 144, 255, 255), // DodgerBlue default
        }
    }

    pub fn with_body_type(mut self, body_type: RigidBodyType) -> Self {
        self.body_type = body_type;
        self
    }

    pub fn with_mass_radius(mut self, r: f32) -> Self {
        self.mass_radius = r;
        self
    }

    pub fn with_friction(mut self, friction: f32) -> Self {
        self.friction = friction;
        self
    }

    /// Controls the stiffness of the springs (consistency). Higher = stiffer.
    pub fn with_stiffness(mut self, k: f32) -> Self {
        self.stiffness = k;
        self
    }

    pub fn with_damping(mut self, c: f32) -> Self {
        self.damping = c;
        self
    }

    /// Set polygon color as RGBA
    pub fn with_color(mut self, r: u8, g: u8, b: u8, a: u8) -> Self {
        self.color = (r, g, b, a);
        self
    }

    /// Build into the provided world, returning a handle for rendering
    pub fn build(self, world: &mut PhysicsWorld) -> SoftRectangleHandle {
        let half = self.size / 2.0;
        let cx = self.position.x;
        let cy = self.position.y;

        // Corner positions (clockwise: TL, TR, BR, BL)
        let pts = [
            vector![cx - half.x, cy + half.y],
            vector![cx + half.x, cy + half.y],
            vector![cx + half.x, cy - half.y],
            vector![cx - half.x, cy - half.y],
        ];

        // Create 4 masses
        let mut corners: [RigidBodyHandle; 4] = [RigidBodyHandle::invalid(); 4];
        for (i, p) in pts.iter().enumerate() {
            let mass = CircleRigidBodyBuilder::new(self.body_type, *p, self.mass_radius)
                .with_friction(self.friction);
            corners[i] = world.add_solid_body(mass);
        }

        // Edge springs with axis aligned to the edge direction: (0-1), (1-2), (2-3), (3-0)
        let edge_pairs = [(0, 1), (1, 2), (2, 3), (3, 0)];
        for (a, b) in edge_pairs {
            let delta = pts[b] - pts[a];
            let rest = delta.norm();
            let axis = if rest > 1e-4 {
                delta / rest
            } else {
                vector![1.0, 0.0]
            };
            let spring = SpringBuilder::new_linear(
                axis,
                rapier2d::na::Point2::origin(),
                rapier2d::na::Point2::origin(),
                self.stiffness,
                self.damping,
            )
            .with_target_position(rest);
            world.add_spring(spring, corners[a], corners[b]);
        }

        // Cross braces for shape rigidity with appropriate axes: (0-2) and (1-3)
        let cross_pairs = [(0, 2), (1, 3)];
        for (a, b) in cross_pairs {
            let delta = pts[b] - pts[a];
            let rest = delta.norm();
            let axis = if rest > 1e-4 {
                delta / rest
            } else {
                vector![1.0, 0.0]
            };
            let spring = SpringBuilder::new_linear(
                axis,
                rapier2d::na::Point2::origin(),
                rapier2d::na::Point2::origin(),
                self.stiffness,
                self.damping,
            )
            .with_target_position(rest);
            world.add_spring(spring, corners[a], corners[b]);
        }

        // Register for rendering
        let instance = SoftRectangleInstance {
            corners,
            color: self.color,
        };
        SoftRectangleHandle(world.register_soft_rectangle(instance))
    }
}
