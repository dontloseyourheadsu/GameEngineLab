use engine_core::{
    resolve_all_collisions, resolve_collision, BoxObject, CollisionMesh, PhysicsObject, VerletPoint,
};
use raylib::prelude::*;

fn main() {
    let (mut handler, thread) = raylib::init()
        .size(1000, 1000)
        .title("Collision Simulator - Circles and Rectangles")
        .build();

    let mut circles: Vec<VerletPoint> = Vec::new(); // Specify type for temporarily unused vector
    let mut rectangles = Vec::new();

    // TEMPORARILY DISABLED: Create some circles
    /*
    for _ in 0..15 {
        let x = rand::random::<f32>() * 450.0 + 50.0; // Left side of screen
        let y = rand::random::<f32>() * 900.0 + 50.0;
        let mass = rand::random::<f32>() * 3.0 + 0.5; // Mass between 0.5 and 3.5
        let size = rand::random::<f32>() * 15.0 + 10.0; // Size between 10.0 and 25.0 (bigger)
        let color = Color::new(
            rand::random::<u8>(),
            rand::random::<u8>(),
            rand::random::<u8>(),
            255,
        );

        // Create collision mesh with same size as visual representation
        let collision_mesh = CollisionMesh::new_circle_with_restitution(size, 0.7);

        circles.push(VerletPoint::new_with_collision(
            Vector2::new(x, y),
            mass,
            size,
            color,
            collision_mesh,
        ));
    }
    */

    // Create MANY rectangles across the entire screen for collision testing
    for _ in 0..25 {
        let x = rand::random::<f32>() * 900.0 + 50.0; // Entire screen width
        let y = rand::random::<f32>() * 900.0 + 50.0; // Entire screen height
        let mass = rand::random::<f32>() * 4.0 + 1.0; // Mass between 1.0 and 5.0
        let width = rand::random::<f32>() * 50.0 + 30.0; // Width between 30.0 and 80.0 (bigger)
        let height = rand::random::<f32>() * 50.0 + 30.0; // Height between 30.0 and 80.0 (bigger)
        let can_rotate = rand::random::<bool>();
        let color = Color::new(
            rand::random::<u8>(),
            rand::random::<u8>(),
            rand::random::<u8>(),
            255,
        );

        // Create rectangle collision mesh with same size as visual representation
        let collision_mesh = CollisionMesh::new_rectangle_full(
            width, // Same size as visual representation
            height, 0.0, 0.7, can_rotate,
        );

        rectangles.push(BoxObject::new_with_collision(
            Vector2::new(x, y),
            width,
            height,
            mass,
            color,
            collision_mesh,
            can_rotate,
        ));
    }

    let world_bounds = Rectangle::new(0.0, 0.0, 1000.0, 1000.0);

    while !handler.window_should_close() {
        // Apply initial gravity force to all objects (even slower gravity)
        // TEMPORARILY DISABLED: Circle physics
        /*
        for circle in &mut circles {
            circle.apply_force(Vector2::new(0.0, 0.981 * 0.1)); // Even slower gravity
        }
        */
        for rectangle in &mut rectangles {
            rectangle.apply_force(Vector2::new(0.0, 0.981 * 0.1));
            if rectangle.can_rotate() {
                // Even more reduced random torque for better observation
                rectangle.apply_torque(rand::random::<f32>() * 0.05 - 0.025);
            }
        }

        let mut drawing = handler.begin_drawing(&thread);
        drawing.clear_background(Color::BLACK);

        // Update physics for all objects
        // TEMPORARILY DISABLED: Circle updates
        /*
        for circle in &mut circles {
            circle.update(1.0 / 60.0, world_bounds);
        }
        */
        for rectangle in &mut rectangles {
            rectangle.update(1.0 / 60.0, world_bounds);
        }

        // TEMPORARILY DISABLED: Circle collisions
        /*
        // Resolve collisions between circles
        resolve_all_collisions(&mut circles);
        
        // Resolve collisions between circles and rectangles
        for circle in &mut circles {
            for rectangle in &mut rectangles {
                resolve_collision(circle, rectangle);
            }
        }
        */

        // Resolve collisions between rectangles (MAIN TEST)
        resolve_all_collisions(&mut rectangles);

        // TEMPORARILY DISABLED: Draw all circles
        /*
        for circle in &circles {
            let pos = circle.position();
            drawing.draw_circle(pos.x as i32, pos.y as i32, circle.size(), circle.color());

            // Draw collision mesh outline (same size as visual)
            if let Some(collision_mesh) = circle.collision_mesh() {
                if let Some(circle_mesh) = collision_mesh.as_circle() {
                    drawing.draw_circle_lines(
                        pos.x as i32,
                        pos.y as i32,
                        circle_mesh.radius,
                        Color::new(255, 255, 255, 100),
                    );
                }
            }
        }
        */

        // Draw all rectangles (MAIN TEST)
        for rectangle in &rectangles {
            rectangle.draw_simple(&mut drawing);

            // Draw collision mesh outline (same size as visual)
            if let Some(collision_mesh) = rectangle.collision_mesh() {
                if let Some(rect_mesh) = collision_mesh.as_rectangle() {
                    let corners = rect_mesh.get_corners(rectangle.position());

                    // Draw collision mesh outline
                    for i in 0..4 {
                        let start = corners[i];
                        let end = corners[(i + 1) % 4];
                        drawing.draw_line(
                            start.x as i32,
                            start.y as i32,
                            end.x as i32,
                            end.y as i32,
                            Color::new(255, 255, 255, 100),
                        );
                    }
                }
            }
        }

        // Draw instructions
        drawing.draw_text(
            "Collision Simulator - Circles (Left) and Rectangles (Right)",
            10,
            10,
            16,
            Color::WHITE,
        );
        drawing.draw_text(
            &format!(
                "Circles: {} | Rectangles: {}",
                circles.len(),
                rectangles.len()
            ),
            10,
            30,
            12,
            Color::YELLOW,
        );
        // Display debug information for rectangle collision testing
        drawing.draw_text(
            &format!("RECTANGLE COLLISION TEST - Count: {}", rectangles.len()),
            10,
            10,
            20,
            Color::WHITE,
        );
        drawing.draw_text(
            "Watch for disappearing rectangles during collisions!",
            10,
            35,
            16,
            Color::YELLOW,
        );

        // Debug: show position of first few rectangles
        for (i, rect) in rectangles.iter().take(3).enumerate() {
            drawing.draw_text(
                &format!(
                    "Rect {}: ({:.1}, {:.1})",
                    i,
                    rect.position().x,
                    rect.position().y
                ),
                10,
                60 + i as i32 * 25,
                20,
                Color::WHITE,
            );
        }

        drawing.draw_text(
            "RECTANGLE-ONLY COLLISION TEST - Circles temporarily disabled",
            10,
            60,
            16,
            Color::ORANGE,
        );
        drawing.draw_text(
            "White outlines show collision meshes - should match rectangle edges",
            10,
            80,
            12,
            Color::GRAY,
        );
        drawing.draw_text(
            "Some rectangles can rotate during collisions",
            10,
            100,
            12,
            Color::GRAY,
        );
    }
}
