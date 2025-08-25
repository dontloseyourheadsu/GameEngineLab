use engine_core::VerletPoint;
use raylib::prelude::*;

fn main() {
    let (mut handler, thread) = raylib::init()
        .size(600, 600)
        .title("Collision Simulator")
        .build();

    let mut points = Vec::new();
    for _ in 0..50 {
        let x = rand::random::<f32>() * 600.0;
        let y = rand::random::<f32>() * 600.0;
        points.push(VerletPoint::new(Vector2::new(x, y)));
    }

    for point in &mut points {
        point.apply_force(Vector2::new(0.0, 9.81));
    }

    while !handler.window_should_close() {
        let mut drawing = handler.begin_drawing(&thread);
        drawing.clear_background(Color::BLACK);

        for point in &mut points {
            point.update(1.0 / 60.0);
            let pos = point.position();
            drawing.draw_circle(pos.x as i32, pos.y as i32, 5.0, Color::WHITE);
        }
    }
}
