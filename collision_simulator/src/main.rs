use engine_core::{physics_world::PhysicsWorld, rigid_bodies::circle_rigid_body_builder::{CircleRigidBodyBuilder, RigidBodyType}, world_renderer};
use nalgebra::vector;

fn main() {
    // World size 100x100, gravity downwards
    let mut physics_world = PhysicsWorld::new(100, 100, vector![0.0, -9.81]);

    // Place ball above the floor, in the middle
    let ball = CircleRigidBodyBuilder::new(
        RigidBodyType::Dynamic,
        vector![50.0, 80.0],
        5.0,
    );

    physics_world.add_solid_body(ball);

    world_renderer::render("Collision Simulator", &mut physics_world, (1000, 1000), (200.0, 200.0));
}