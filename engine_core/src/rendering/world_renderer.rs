use crate::physics_world::PhysicsWorld;
use rapier2d::prelude::ShapeType;
use raylib::prelude::*;

pub fn render(
    title: &str,
    physics_world: &mut PhysicsWorld,
    window: (i32, i32),
    world: (f32, f32),
) {
    let (window_width, window_height) = window;
    let (mut rl, thread) = raylib::init()
        .size(window_width, window_height)
        .title(title)
        .build();

    // Limit the frame rate
    rl.set_target_fps(60);

    // Fixed physics timestep (in seconds). 120 Hz gives good stability.
    let physics_hz: f32 = 240.0;
    let physics_dt: f32 = 1.0 / physics_hz;
    let mut accumulator: f32 = 0.0;

    // World dimensions in "meters" (choose a convention and stick to it)
    let (world_width, world_height) = world;

    // Map world (y-up) to screen (y-down)
    let to_screen = |x: f32, y: f32| -> (i32, i32) {
        let sx = ((x / world_width) * (window_width as f32 - 2.0)) as i32;
        let sy = window_height as f32 - ((y / world_height) * (window_height as f32 - 2.0)) as f32;
        (sx, sy as i32)
    };

    while !rl.window_should_close() {
        let paused = rl.is_window_minimized() || !rl.is_window_focused();
        let mut frame_dt = if paused { 0.0 } else { rl.get_frame_time() };

        // Prevent "spiral of death" if the window is hung
        if frame_dt > 0.25 {
            frame_dt = 0.25;
        }

        accumulator += frame_dt;

        // Advance the simulation in fixed steps of physics_dt
        while accumulator >= physics_dt {
            physics_world.step_with_dt(physics_dt);
            accumulator -= physics_dt;
        }

        // Render
        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::WHITE);

        // Bodies
        let rigid_bodies = &physics_world.rigid_body_set;
        // Collect soft-body node handles to avoid drawing them as standalone circles
        let mut soft_nodes = std::collections::HashSet::new();
        for soft in &physics_world.soft_rectangles {
            for h in &soft.nodes {
                soft_nodes.insert(*h);
            }
            // corners are also nodes
            for h in &soft.corners {
                soft_nodes.insert(*h);
            }
        }

        for (handle, body) in rigid_bodies.iter() {
            let position = body.position();
            let x = position.translation.vector.x;
            let y = position.translation.vector.y;
            let rotation = position.rotation.angle();

            for collider_handle in body.colliders() {
                if let Some(collider) = physics_world.collider_set.get(*collider_handle) {
                    let (sx, sy) = to_screen(x, y);

                    match collider.shape().shape_type() {
                        ShapeType::Ball => {
                            // Skip drawing balls used as soft-body nodes (we render the polygon instead)
                            if soft_nodes.contains(&handle) {
                                continue;
                            }
                            if let Some(ball) = collider.shape().as_ball() {
                                let screen_radius =
                                    ((ball.radius / world_width) * window_width as f32).max(5.0)
                                        as i32;
                                d.draw_circle(sx, sy, screen_radius as f32, Color::RED);
                            }
                        }
                        ShapeType::Cuboid => {
                            if let Some(cuboid) = collider.shape().as_cuboid() {
                                let half_extents = cuboid.half_extents;
                                let screen_width =
                                    ((half_extents.x * 2.0 / world_width) * window_width as f32)
                                        .max(2.0) as i32;
                                let screen_height =
                                    ((half_extents.y * 2.0 / world_height) * window_height as f32)
                                        .max(2.0) as i32;

                                // Create rectangle positioned at (sx, sy) - draw_rectangle_pro will center it with the pivot
                                let rect = Rectangle::new(
                                    sx as f32,
                                    sy as f32,
                                    screen_width as f32,
                                    screen_height as f32,
                                );

                                // Draw rotated rectangle with center pivot
                                d.draw_rectangle_pro(
                                    rect,
                                    Vector2::new(
                                        screen_width as f32 / 2.0,
                                        screen_height as f32 / 2.0,
                                    ),
                                    rotation.to_degrees(),
                                    Color::BLUE,
                                );
                            }
                        }
                        ShapeType::Triangle => {
                            if let Some(triangle) = collider.shape().as_triangle() {
                                let vertices = triangle.vertices();
                                let p1 = vertices[0];
                                let p2 = vertices[1];
                                let p3 = vertices[2];

                                // Transform vertices to world coordinates
                                let cos_r = rotation.cos();
                                let sin_r = rotation.sin();

                                // Apply rotation and translation to each vertex
                                let world_p1 = nalgebra::Vector2::new(
                                    x + p1.x * cos_r - p1.y * sin_r,
                                    y + p1.x * sin_r + p1.y * cos_r,
                                );
                                let world_p2 = nalgebra::Vector2::new(
                                    x + p2.x * cos_r - p2.y * sin_r,
                                    y + p2.x * sin_r + p2.y * cos_r,
                                );
                                let world_p3 = nalgebra::Vector2::new(
                                    x + p3.x * cos_r - p3.y * sin_r,
                                    y + p3.x * sin_r + p3.y * cos_r,
                                );

                                let (sx1, sy1) = to_screen(world_p1.x, world_p1.y);
                                let (sx2, sy2) = to_screen(world_p2.x, world_p2.y);
                                let (sx3, sy3) = to_screen(world_p3.x, world_p3.y);

                                d.draw_triangle(
                                    Vector2::new(sx1 as f32, sy1 as f32),
                                    Vector2::new(sx2 as f32, sy2 as f32),
                                    Vector2::new(sx3 as f32, sy3 as f32),
                                    Color::GREEN,
                                );
                            }
                        }
                        _ => {
                            // Handle other shape types or draw a default representation
                            d.draw_circle(sx, sy, 5.0, Color::GRAY);
                        }
                    }
                }
            }
        }

        // Render soft rectangles (filled polygon using two triangles)
        for soft in &physics_world.soft_rectangles {
            let color = Color::new(soft.color.0, soft.color.1, soft.color.2, soft.color.3);
            // Fetch positions of the 4 corners; if any missing, skip
            if let (Some(b0), Some(b1), Some(b2), Some(b3)) = (
                physics_world.rigid_body_set.get(soft.corners[0]),
                physics_world.rigid_body_set.get(soft.corners[1]),
                physics_world.rigid_body_set.get(soft.corners[2]),
                physics_world.rigid_body_set.get(soft.corners[3]),
            ) {
                let p0 = b0.position().translation.vector;
                let p1 = b1.position().translation.vector;
                let p2 = b2.position().translation.vector;
                let p3 = b3.position().translation.vector;

                let (x0, y0) = to_screen(p0.x, p0.y);
                let (x1, y1) = to_screen(p1.x, p1.y);
                let (x2, y2) = to_screen(p2.x, p2.y);
                let (x3, y3) = to_screen(p3.x, p3.y);

                // Draw quad fill via two triangles: (0,1,2) and (0,2,3)
                d.draw_triangle(
                    Vector2::new(x0 as f32, y0 as f32),
                    Vector2::new(x1 as f32, y1 as f32),
                    Vector2::new(x2 as f32, y2 as f32),
                    color,
                );
                d.draw_triangle(
                    Vector2::new(x0 as f32, y0 as f32),
                    Vector2::new(x2 as f32, y2 as f32),
                    Vector2::new(x3 as f32, y3 as f32),
                    color,
                );

                // Optional outline for clarity
                d.draw_line(x0, y0, x1, y1, Color::BLACK);
                d.draw_line(x1, y1, x2, y2, Color::BLACK);
                d.draw_line(x2, y2, x3, y3, Color::BLACK);
                d.draw_line(x3, y3, x0, y0, Color::BLACK);
            }
        }

        // Render springs/joints
        for joint_handle in &physics_world.joint_handles {
            if let Some(joint) = physics_world.impulse_joint_set.get(*joint_handle) {
                let body1_handle = joint.body1;
                let body2_handle = joint.body2;

                if let (Some(body1), Some(body2)) = (
                    physics_world.rigid_body_set.get(body1_handle),
                    physics_world.rigid_body_set.get(body2_handle),
                ) {
                    let pos1 = body1.position();
                    let pos2 = body2.position();

                    let (sx1, sy1) = to_screen(pos1.translation.x, pos1.translation.y);
                    let (sx2, sy2) = to_screen(pos2.translation.x, pos2.translation.y);

                    // Draw spring as a zigzag line
                    draw_spring_line(&mut d, sx1, sy1, sx2, sy2);
                }
            }
        }
    }
}

// Helper function to draw a spring as a zigzag line
fn draw_spring_line(d: &mut RaylibDrawHandle, x1: i32, y1: i32, x2: i32, y2: i32) {
    let segments = 8; // Number of spring coils
    let amplitude = 8.0; // How wide the spring coils are

    let dx = (x2 - x1) as f32;
    let dy = (y2 - y1) as f32;
    let length = (dx * dx + dy * dy).sqrt();

    if length < 1.0 {
        return; // Too short to draw
    }

    let unit_x = dx / length;
    let unit_y = dy / length;
    let perp_x = -unit_y; // Perpendicular vector
    let perp_y = unit_x;

    let mut prev_x = x1 as f32;
    let mut prev_y = y1 as f32;

    for i in 1..=segments {
        let t = i as f32 / segments as f32;
        let base_x = x1 as f32 + dx * t;
        let base_y = y1 as f32 + dy * t;

        // Zigzag offset
        let offset = if i % 2 == 0 { amplitude } else { -amplitude };
        let spring_x = base_x + perp_x * offset;
        let spring_y = base_y + perp_y * offset;

        // Draw line segment
        d.draw_line(
            prev_x as i32,
            prev_y as i32,
            spring_x as i32,
            spring_y as i32,
            Color::ORANGE,
        );

        prev_x = spring_x;
        prev_y = spring_y;
    }

    // Draw final segment to end point
    d.draw_line(prev_x as i32, prev_y as i32, x2, y2, Color::ORANGE);
}
