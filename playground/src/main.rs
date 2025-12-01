use engine_core::collision_body::circle_collision_body::CircleCollisionBody;
use engine_core::rigid_body::rigid_body::RigidBody;
use engine_core::textures::sprite2d::{Shape, Sprite2D};
use raylib::prelude::*;

fn main() {
    let screen_width = 800.0;
    let screen_height = 600.0;

    let (mut rl, thread) = raylib::init()
        .size(screen_width as i32, screen_height as i32)
        .title("Collision Simulator")
        .build();

    rl.set_target_fps(60);

    let radius = 20.0;
    let position = Vector2::new(screen_width / 2.0, 100.0);
    let damping = 0.7;

    let collision_body = CircleCollisionBody::new(position, radius, damping);
    let texture = Sprite2D {
        color: Color::RED,
        shape: Shape::Circle { radius },
    };

    let mut ball = RigidBody::new(collision_body, texture);

    let gravity = Vector2::new(0.0, 1000.0); // Gravity acceleration
    let friction = 0.01; // Air resistance / friction

    while !rl.window_should_close() {
        let dt = rl.get_frame_time();

        // Update
        ball.update(dt, gravity, friction, screen_width, screen_height);

        // Draw
        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::WHITE);

        ball.draw(&mut d);

        d.draw_fps(10, 10);
    }
}
