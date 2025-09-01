// world_renderer.rs
use crate::physics_world::PhysicsWorld;
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

    // Limita el render para no quemar CPU/GPU; 60 es razonable.
    rl.set_target_fps(60);

    // Paso fijo de física (en segundos). 120 Hz da buena estabilidad.
    let physics_hz: f32 = 240.0;
    let physics_dt: f32 = 1.0 / physics_hz;
    let mut accumulator: f32 = 0.0;

    // Dimensiones del mundo en "metros" (elige una convención y sé consistente)
    let (world_width, world_height) = world;

    // Mapea mundo (y-up) a pantalla (y-down)
    let to_screen = |x: f32, y: f32| -> (i32, i32) {
        let sx = ((x / world_width) * (window_width as f32 - 2.0)) as i32;
        let sy = window_height as f32 - ((y / world_height) * (window_height as f32 - 2.0)) as f32;
        (sx, sy as i32)
    };

    while !rl.window_should_close() {
        let paused = rl.is_window_minimized() || !rl.is_window_focused();
        let mut frame_dt = if paused { 0.0 } else { rl.get_frame_time() };

        // Evita el "spiral of death" si la ventana se quedó colgada
        if frame_dt > 0.25 {
            frame_dt = 0.25;
        }

        accumulator += frame_dt;

        // Avanza la simulación en pasos fijos de physics_dt
        while accumulator >= physics_dt {
            physics_world.step_with_dt(physics_dt);
            accumulator -= physics_dt;
        }

        // Render
        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::WHITE);

        // Cuerpos
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
