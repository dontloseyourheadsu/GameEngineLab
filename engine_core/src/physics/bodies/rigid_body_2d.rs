use raylib::math::Vector2;

use crate::physics::collisions_2d::complex_collision_body::ComplexCollisionBody;
use crate::physics::velocity::Velocity;

pub struct RigidBody2D {
    pub position: Vector2,
    pub velocity: Velocity,
    pub collider: ComplexCollisionBody,
    // Rotation is typically needed if the body rotates, but if the ComplexCollisionBody::Box has an 'angle',
    // we might need to sync them. However, ComplexCollisionBody is just data.
    // If the body rotates, the collider's angle should be updated or stored here.
    // Let's assume the body has a rotation state.
    pub rotation: f32,
}

impl RigidBody2D {
    pub fn new(position: Vector2, collider: ComplexCollisionBody) -> Self {
        Self {
            position,
            velocity: Velocity(Vector2::zero()),
            collider,
            rotation: 0.0,
        }
    }

    /// Checks collisions between a list of rigid bodies.
    /// Returns a list of indices pairs (i, j) that are colliding.
    pub fn check_collisions(bodies: &[RigidBody2D]) -> Vec<(usize, usize)> {
        // Collect world-space colliders first
        let world_colliders: Vec<(Vector2, ComplexCollisionBody)> = bodies
            .iter()
            .map(|b| (b.position, b.get_world_collider()))
            .collect();

        // Convert to references for the solver
        let collider_refs: Vec<(Vector2, &ComplexCollisionBody)> =
            world_colliders.iter().map(|(p, c)| (*p, c)).collect();

        crate::physics::collisions_2d::complex_collisions::check_complex_collisions(&collider_refs)
    }

    fn get_world_collider(&self) -> ComplexCollisionBody {
        match &self.collider {
            ComplexCollisionBody::Circle { .. } => self.collider.clone(),
            ComplexCollisionBody::Box {
                width,
                height,
                angle,
            } => {
                // Combine body rotation with collider local angle
                ComplexCollisionBody::Box {
                    width: *width,
                    height: *height,
                    angle: angle + self.rotation,
                }
            }
        }
    }
}
