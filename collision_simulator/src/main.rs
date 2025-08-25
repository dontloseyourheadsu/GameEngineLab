use engine_core::{resolve_all_collisions, CollisionMesh, VerletPoint};
use raylib::prelude::*;

fn main() {
    let (mut handler, thread) = raylib::init()
        .size(600, 600)
        .title("Collision Simulator")
        .build();

    let mut points = Vec::new();
    for _ in 0..25 {
        let x = rand::random::<f32>() * 500.0 + 50.0; // Keep away from edges
        let y = rand::random::<f32>() * 500.0 + 50.0;
        let mass = rand::random::<f32>() * 5.0 + 0.5; // Mass between 0.5 and 5.5
        let size = rand::random::<f32>() * 10.0 + 5.0; // Size between 5.0 and 15.0
        let color = Color::new(
            rand::random::<u8>(),
            rand::random::<u8>(),
            rand::random::<u8>(),
            255,
        );

        // Create collision mesh with radius slightly smaller than visual size
        let collision_mesh = CollisionMesh::new_with_restitution(size * 0.9, 0.7);

        points.push(VerletPoint::new_with_collision(
            Vector2::new(x, y),
            mass,
            size,
            color,
            collision_mesh,
        ));
    }
    
    let world_bounds = Rectangle::new(0.0, 0.0, 600.0, 600.0);
    
    while !handler.window_should_close() {
        // Apply initial gravity force to all points
        for point in &mut points {
            point.apply_force(Vector2::new(0.0, 0.981));
        }

        let mut drawing = handler.begin_drawing(&thread);
        drawing.clear_background(Color::BLACK);

        // Update physics and resolve collisions
        for point in &mut points {
            point.update(1.0 / 60.0, world_bounds);
        }

        // Resolve collisions between all points
        resolve_all_collisions(&mut points);

        // Draw all points
        for point in &points {
            let pos = point.position();
            drawing.draw_circle(pos.x as i32, pos.y as i32, point.size(), point.color());

            // Optional: Draw collision radius as a thin outline
            if let Some(collision_mesh) = point.collision_mesh() {
                drawing.draw_circle_lines(
                    pos.x as i32,
                    pos.y as i32,
                    collision_mesh.radius,
                    Color::new(255, 255, 255, 50),
                );
            }
        }

        // Draw instructions
        drawing.draw_text(
            "Collision Simulator with Verlet Integration",
            10,
            10,
            16,
            Color::WHITE,
        );
        drawing.draw_text(
            "White circles show collision radius",
            10,
            30,
            12,
            Color::GRAY,
        );
    }
}
