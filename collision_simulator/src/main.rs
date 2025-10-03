use engine_core::{
    physics_world::PhysicsWorld,
    rigid_bodies::{
        circle_rigid_body_builder::{CircleRigidBodyBuilder, RigidBodyType},
        rectangle_rigid_body_builder::RectangleRigidBodyBuilder,
        spring_builder::SpringBuilder,
        triangle_rigid_body_builder::TriangleRigidBodyBuilder,
    },
    world_renderer,
};
use nalgebra::{vector, Point2};
use engine_core::builders::soft_bodies::soft_rectangle_builder::SoftRectangleBuilder;

fn main() {
    // World size 100x100, gravity downwards
    let mut physics_world = PhysicsWorld::new(100, 100, vector![0.0, -9.81]);

    // Create the original falling shapes (circle, square, triangle)
    let ball = CircleRigidBodyBuilder::new(RigidBodyType::Dynamic, vector![40.0, 85.0], 3.0)
        .with_friction(0.3);

    let square = RectangleRigidBodyBuilder::new(
        RigidBodyType::Dynamic,
        vector![50.0, 90.0],
        vector![6.0, 6.0],
    )
    .with_friction(0.3);

    let triangle = TriangleRigidBodyBuilder::new(
        RigidBodyType::Dynamic,
        vector![60.0, 95.0], // position
        vector![0.0, 4.0],   // vertex 1 (relative to center)
        vector![-3.0, -2.0], // vertex 2 (relative to center)
        vector![3.0, -2.0],  // vertex 3 (relative to center)
    )
    .with_friction(0.3);

    // Add the falling shapes
    physics_world.add_solid_body(ball);
    physics_world.add_solid_body(square);
    physics_world.add_solid_body(triangle);

    // Create a horizontal spring line to catch the shapes
    // Simple line with fixed endpoints and dynamic middle points
    let mut net_handles = Vec::new();
    let num_points = 40; // Number of points in the line (increased for more dynamic points)
    let line_spacing = 1.5; // Reduced spacing to fit more points
    let line_start_x = 20.0;
    let line_y = 50.0; // Horizontal line at this height

    // Create the line masses
    for i in 0..num_points {
        let x = line_start_x + (i as f32) * line_spacing;

        // Fix only the first and last points
        let body_type = if i == 0 || i == num_points - 1 {
            RigidBodyType::Fixed // Fixed endpoints
        } else {
            RigidBodyType::Dynamic // Dynamic middle points
        };

        let mass =
            CircleRigidBodyBuilder::new(body_type, vector![x, line_y], 0.5).with_friction(0.8);

        let handle = physics_world.add_solid_body(mass);
        net_handles.push(handle);
    }

    // Connect adjacent masses with springs
    for i in 0..(num_points - 1) {
        let spring = SpringBuilder::new_linear(
            vector![1.0, 0.0], // Horizontal direction
            Point2::new(0.0, 0.0),
            Point2::new(0.0, 0.0),
            400.0, // Spring strength
            20.0,  // Damping
        )
        .with_target_position(line_spacing);

        physics_world.add_spring(spring, net_handles[i], net_handles[i + 1]);
    }

    // Add a soft rectangle using the new builder
    // Position near top-center, size 12x8, medium stiffness, custom color
    let _soft_rect = SoftRectangleBuilder::new(vector![50.0, 75.0], vector![12.0, 8.0])
        .with_stiffness(900.0) // stiffer
        .with_damping(40.0)
        .with_mass_radius(0.6)
        .with_color(200, 100, 240, 200) // RGBA
        .build(&mut physics_world);

    world_renderer::render(
        "Spring Line Catching Falling Shapes",
        &mut physics_world,
        (1000, 1000),
        (100.0, 100.0),
    );
}
