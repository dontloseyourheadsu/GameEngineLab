use crate::rigid_bodies::solid_body_build::SolidBodyBuild;
use rapier2d::na::Vector2;
pub use rapier2d::prelude::*;

#[derive(Clone)]
pub struct CircleRigidBodyBuilder {
    pub(crate) body: RigidBody,
    pub(crate) collider: Collider,
}

impl SolidBodyBuild for CircleRigidBodyBuilder {
    fn body(&self) -> &RigidBody {
        &self.body
    }

    fn collider(&self) -> &Collider {
        &self.collider
    }
}

impl CircleRigidBodyBuilder {
    pub fn new(body_type: RigidBodyType, position: Vector2<f32>, radius: f32) -> Self {
        let solid_body = RigidBodyBuilder::new(body_type)
            .translation(position)
            .build();

        let collider = ColliderBuilder::ball(radius).build();

        CircleRigidBodyBuilder {
            body: solid_body,
            collider: collider,
        }
    }
}
