use rapier2d::na::Vector2;
pub use rapier2d::prelude::*;

#[derive(Clone)]
pub struct SolidBody {
    pub(crate) body: RigidBody,
}

impl SolidBody {
    pub fn new(
        body_type: RigidBodyType,
        translation: Vector2<f32>,
        rotation: f32,
    ) -> Self {
        let rigid_body_builder = match body_type {
            RigidBodyType::Fixed => RigidBodyBuilder::fixed(),
            RigidBodyType::Dynamic => RigidBodyBuilder::dynamic(),
            RigidBodyType::KinematicVelocityBased => RigidBodyBuilder::kinematic_velocity_based(),
            RigidBodyType::KinematicPositionBased => RigidBodyBuilder::kinematic_position_based(),
        }
        .translation(translation)
        .rotation(rotation);

        let body = rigid_body_builder.build();
        SolidBody { body }
    }
}