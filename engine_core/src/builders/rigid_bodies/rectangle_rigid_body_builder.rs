use crate::rigid_bodies::solid_body_build::SolidBodyBuild;
use rapier2d::na::Vector2;
pub use rapier2d::prelude::*;

#[derive(Clone)]
pub struct RectangleRigidBodyBuilder {
    pub(crate) body: RigidBody,
    pub(crate) collider: Collider,
}

impl SolidBodyBuild for RectangleRigidBodyBuilder {
    fn body(&self) -> &RigidBody {
        &self.body
    }

    fn collider(&self) -> &Collider {
        &self.collider
    }
}

impl RectangleRigidBodyBuilder {
    pub fn new(body_type: RigidBodyType, position: Vector2<f32>, size: Vector2<f32>) -> Self {
        let solid_body = RigidBodyBuilder::new(body_type)
            .translation(position)
            .build();

        let collider = ColliderBuilder::cuboid(size.x / 2.0, size.y / 2.0)
            .friction(0.7) // Default friction coefficient
            .build();

        RectangleRigidBodyBuilder {
            body: solid_body,
            collider: collider,
        }
    }

    pub fn with_friction(mut self, friction: f32) -> Self {
        // Rebuild the collider with new friction
        let cuboid = self.collider.shape().as_cuboid().unwrap();
        let half_extents = cuboid.half_extents;
        self.collider = ColliderBuilder::cuboid(half_extents.x, half_extents.y)
            .friction(friction)
            .build();
        self
    }
}
