use raylib::prelude::*;

use crate::physics_world::PhysicsWorld;

pub fn render(physics_world: &mut PhysicsWorld) {
    let window_width = 500;
    let window_height = 500;
    let margin = 20.0; // margin in pixels for better visibility
    let (mut rl, thread) = raylib::init()
        .size(window_width, window_height)
        .title("Collision Simulator")
        .build();

    rl.set_target_fps(60);

    // Physics world dimensions (for mapping physics to screen)
    let world_width = 100.0;
    let world_height = 100.0;

    // Helper to convert world coordinates to screen coordinates (y-up to y-down, with margin)
    let to_screen = |x: f32, y: f32| -> (i32, i32) {
        let sx = ((x / world_width) * (window_width as f32 - 2.0 * margin) + margin) as i32;
        // Invert y for screen (0,0 at bottom left of world)
        let sy = (window_height as f32 - margin)
            - ((y / world_height) * (window_height as f32 - 2.0 * margin)) as f32;
        (sx, sy as i32)
    };

    while !rl.window_should_close() {
        let paused = rl.is_window_minimized() || !rl.is_window_focused();
        if paused {
            continue;
        }

        // Step physics
        physics_world.step();

        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::WHITE);

        // Draw floor (at y=0)
        let (fx1, fy1) = to_screen(0.0, 0.0);
        let (fx2, fy2) = to_screen(world_width, 0.0);
        d.draw_line(fx1, fy1, fx2, fy2, Color::BLACK);

        // Draw all balls
        let rigid_bodies = &physics_world.rigid_body_set;
        for (_handle, body) in rigid_bodies.iter() {
            let position = body.position();
            let x = position.translation.vector.x;
            let y = position.translation.vector.y;
            for collider_handle in body.colliders() {
                if let Some(collider) = physics_world.collider_set.get(*collider_handle) {
                    if let Some(ball) = collider.shape().as_ball() {
                        let (sx, sy) = to_screen(x, y);
                        let screen_radius =
                            ((ball.radius / world_width) * window_width as f32).max(5.0) as i32;
                        d.draw_circle(sx, sy, screen_radius as f32, Color::RED);
                    }
                }
            }
        }
    }
}
