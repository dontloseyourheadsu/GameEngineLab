use engine_core::collision_body::circle_collision_body::CircleCollisionBody;
use engine_core::rigid_body::rigid_body::RigidBody;
use engine_core::spring::spring::Spring;
use engine_core::textures::sprite2d::{Shape, Sprite2D};
use raylib::prelude::*;

fn main() {
    let screen_width = 800.0;
    let screen_height = 600.0;

    let (mut rl, thread) = raylib::init()
        .size(screen_width as i32, screen_height as i32)
        .title("Collision Simulator Demo")
        .build();

    rl.set_target_fps(60);

    let radius = 20.0;
    let damping = 0.7;
    let gravity = Vector2::new(0.0, 1000.0);
    let friction = 0.01;

    // 1. Colliding Pair (Red & Blue)
    // They start apart and move towards each other
    let pos1 = Vector2::new(200.0, 300.0);
    let mut body1 = CircleCollisionBody::new(pos1, radius, damping, true, true);
    // Initial velocity kick (Right, Up)
    body1.old_position = pos1 - Vector2::new(10.0, -10.0);
    let ball1 = RigidBody::new(
        body1,
        Sprite2D {
            color: Color::RED,
            shape: Shape::Circle { radius },
        },
    );

    let pos2 = Vector2::new(300.0, 300.0);
    let mut body2 = CircleCollisionBody::new(pos2, radius, damping, true, true);
    // Initial velocity kick (Left, Up)
    body2.old_position = pos2 - Vector2::new(-10.0, -10.0);
    let ball2 = RigidBody::new(
        body2,
        Sprite2D {
            color: Color::BLUE,
            shape: Shape::Circle { radius },
        },
    );

    // 2. Non-Colliding Pair (Green & Yellow)
    // They start apart and move towards each other but will pass through
    let pos3 = Vector2::new(500.0, 300.0);
    let mut body3 = CircleCollisionBody::new(pos3, radius, damping, false, true);
    body3.old_position = pos3 - Vector2::new(10.0, -10.0);
    let ball3 = RigidBody::new(
        body3,
        Sprite2D {
            color: Color::GREEN,
            shape: Shape::Circle { radius },
        },
    );

    let pos4 = Vector2::new(600.0, 300.0);
    let mut body4 = CircleCollisionBody::new(pos4, radius, damping, false, true);
    body4.old_position = pos4 - Vector2::new(-10.0, -10.0);
    let ball4 = RigidBody::new(
        body4,
        Sprite2D {
            color: Color::YELLOW,
            shape: Shape::Circle { radius },
        },
    );

    // 3. No World Boundaries (Purple)
    // Falls through the floor
    let pos5 = Vector2::new(400.0, 100.0);
    let body5 = CircleCollisionBody::new(pos5, radius, damping, true, false);
    let ball5 = RigidBody::new(
        body5,
        Sprite2D {
            color: Color::PURPLE,
            shape: Shape::Circle { radius },
        },
    );

    // 4. Spring Connected (Orange & Pink)
    let pos6 = Vector2::new(100.0, 100.0);
    let body6 = CircleCollisionBody::new(pos6, radius, damping, true, true);
    let ball6 = RigidBody::new(
        body6,
        Sprite2D {
            color: Color::ORANGE,
            shape: Shape::Circle { radius },
        },
    );

    let pos7 = Vector2::new(101.0, 250.0);
    let body7 = CircleCollisionBody::new(pos7, radius, damping, true, true);
    let ball7 = RigidBody::new(
        body7,
        Sprite2D {
            color: Color::PINK,
            shape: Shape::Circle { radius },
        },
    );

    let spring = Spring::new(100.0, 25.0, 0.5);

    let mut balls = vec![ball1, ball2, ball3, ball4, ball5, ball6, ball7];

    while !rl.window_should_close() {
        let dt = rl.get_frame_time();

        // Update all
        for ball in &mut balls {
            ball.update(dt, gravity, friction, screen_width, screen_height);
        }

        // Apply Spring
        let (left, right) = balls.split_at_mut(6);
        let b6 = &mut left[5];
        let b7 = &mut right[0];
        spring.update(&mut b6.collision_body, &mut b7.collision_body, dt);

        // Resolve collisions
        for i in 0..balls.len() {
            let (left, right) = balls.split_at_mut(i + 1);
            let b1 = &mut left[i];
            for b2 in right {
                CircleCollisionBody::resolve_collision(
                    &mut b1.collision_body,
                    &mut b2.collision_body,
                );
            }
        }

        // Draw
        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::WHITE);

        d.draw_text("Colliding", 220, 250, 20, Color::BLACK);
        d.draw_text("Non-Colliding", 520, 250, 20, Color::BLACK);
        d.draw_text("No Bounds", 350, 50, 20, Color::BLACK);
        d.draw_text("Spring", 50, 50, 20, Color::BLACK);

        // Draw spring line
        spring.draw(&mut d, &balls[5].collision_body, &balls[6].collision_body);

        for ball in &balls {
            ball.draw(&mut d);
        }

        d.draw_fps(10, 10);
    }
}
