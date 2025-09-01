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
        for (_handle, body) in rigid_bodies.iter() {
            let position = body.position();
            let x = position.translation.vector.x;
            let y = position.translation.vector.y;
            let rotation = position.rotation.angle();

            for collider_handle in body.colliders() {
                if let Some(collider) = physics_world.collider_set.get(*collider_handle) {
                    let (sx, sy) = to_screen(x, y);

                    match collider.shape().shape_type() {
                        ShapeType::Ball => {
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
                        _ => {
                            // Handle other shape types or draw a default representation
                            d.draw_circle(sx, sy, 5.0, Color::GRAY);
                        }
                    }
                }
            }
        }
    }
}
