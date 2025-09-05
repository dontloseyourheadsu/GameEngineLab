// Example usage of SpringBuilder
//
// This example demonstrates how to create and use springs in the physics world.
// Springs connect two rigid bodies and apply forces to maintain a desired distance.

use engine_core::builders::rigid_bodies::{
    circle_rigid_body_builder::CircleRigidBodyBuilder,
    spring_builder::SpringBuilder,
};
use engine_core::physics::physics_world::PhysicsWorld;
use rapier2d::na::{Point2, Vector2};
use rapier2d::prelude::RigidBodyType;

fn main() {
    // Create a physics world
    let mut physics_world = PhysicsWorld::new(800, 600, Vector2::new(0.0, -9.81));

    // Create two circle bodies
    let body1_builder = CircleRigidBodyBuilder::new(
        RigidBodyType::Dynamic,
        Vector2::new(100.0, 300.0),
        20.0,
    );
    
    let body2_builder = CircleRigidBodyBuilder::new(
        RigidBodyType::Dynamic,
        Vector2::new(200.0, 300.0),
        20.0,
    );

    // Add bodies to the physics world
    physics_world.add_solid_body(body1_builder);
    physics_world.add_solid_body(body2_builder);

    // Get the handles of the added bodies (they are the last two added)
    let body1_handle = physics_world.rigid_body_handles[physics_world.rigid_body_handles.len() - 2];
    let body2_handle = physics_world.rigid_body_handles[physics_world.rigid_body_handles.len() - 1];

    // Create different types of springs

    // 1. Default spring connecting the centers of the bodies
    let spring1 = SpringBuilder::default_spring(
        Point2::new(0.0, 0.0), // Center of body1
        Point2::new(0.0, 0.0), // Center of body2
        100.0, // Rest length: maintain 100 units distance
    );

    // 2. Soft spring with custom parameters
    let spring2 = SpringBuilder::soft_spring(
        Point2::new(0.0, 0.0),
        Point2::new(0.0, 0.0),
        80.0,
    )
    .with_stiffness(300.0)
    .with_damping(60.0);

    // 3. Stiff spring
    let spring3 = SpringBuilder::stiff_spring(
        Point2::new(0.0, 0.0),
        Point2::new(0.0, 0.0),
        120.0,
    );

    // Add springs to the physics world
    let _spring_handle1 = physics_world.add_spring(spring1, body1_handle, body2_handle);

    // For a physics simulation, you would now step the world in a loop:
    // loop {
    //     physics_world.step_with_dt(1.0 / 60.0); // 60 FPS
    //     
    //     // Update graphics/rendering based on body positions
    //     // ...
    // }

    println!("Spring demo created! Two bodies connected with a spring.");
    println!("Body 1 handle: {:?}", body1_handle);
    println!("Body 2 handle: {:?}", body2_handle);
    println!("Spring handle: {:?}", _spring_handle1);
}
