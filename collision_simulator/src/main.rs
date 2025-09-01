use engine_core::{
    physics_world::PhysicsWorld,
    rigid_bodies::{
        circle_rigid_body_builder::{CircleRigidBodyBuilder, RigidBodyType},
        rectangle_rigid_body_builder::RectangleRigidBodyBuilder, triangle_rigid_body_builder::TriangleRigidBodyBuilder,
    },
    world_renderer,
};
use nalgebra::vector;

fn main() {
    // World size 100x100, gravity downwards
    let mut physics_world = PhysicsWorld::new(100, 100, vector![0.0, -9.81]);

    // Place ball above the floor, in the middle with high friction (will slide less)
    let ball = CircleRigidBodyBuilder::new(RigidBodyType::Dynamic, vector![50.0, 60.0], 5.0)
        .with_friction(0.00001); // High friction ball

    // Square with low friction (will slide more)
    let square = RectangleRigidBodyBuilder::new(
        RigidBodyType::Dynamic,
        vector![48.0, 80.0],
        vector![10.0, 10.0],
    )
    .with_friction(0.00001)
    .with_rotation(18.0); // Low friction square

    let triangle = TriangleRigidBodyBuilder::new(
        RigidBodyType::Dynamic,
        vector![50.0, 120.0], // position
        vector![0.0, 10.0],  // vertex 1 (relative to center)
        vector![-8.0, -5.0], // vertex 2 (relative to center)
        vector![8.0, -5.0],  // vertex 3 (relative to center)
    )
    .with_friction(0.00001)
    .with_rotation(18.0);

    physics_world.add_solid_body(ball);
    physics_world.add_solid_body(square);
    physics_world.add_solid_body(triangle);

    world_renderer::render(
        "Collision Simulator",
        &mut physics_world,
        (1000, 1000),
        (100.0, 100.0),
    );
}
