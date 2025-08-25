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
        let mass = rand::random::<f32>() * 5.0 + 0.5; // Mass between 0.5 and 5.5
        let size = rand::random::<f32>() * 10.0 + 3.0; // Size between 3.0 and 13.0
        let color = Color::new(
            rand::random::<u8>(),
            rand::random::<u8>(),
            rand::random::<u8>(),
            255,
        );
        points.push(VerletPoint::new_full(Vector2::new(x, y), mass, size, color));
    }

    for point in &mut points {
        point.apply_force(Vector2::new(0.0, 100.0));
    }

    let world_bounds = Rectangle::new(0.0, 0.0, 600.0, 600.0);

    while !handler.window_should_close() {
        let mut drawing = handler.begin_drawing(&thread);
        drawing.clear_background(Color::BLACK);

        for point in &mut points {
            point.update(1.0 / 60.0, world_bounds);
            let pos = point.position();
            drawing.draw_circle(pos.x as i32, pos.y as i32, point.size(), point.color());
        }
    }
}
