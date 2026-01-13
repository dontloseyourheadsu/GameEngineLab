use raylib::prelude::*;

pub fn draw_canvas(
    d: &mut RaylibMode2D<RaylibDrawHandle>,
    texture: &impl AsRef<raylib::ffi::Texture2D>,
    canvas_rect: Rectangle,
    canvas_size: i32,
) {
    // Draw Transparency Grid (Checkerboard)
    let check_size = 16.0;
    let cols = (canvas_size as f32 / check_size).ceil() as i32;
    let rows = (canvas_size as f32 / check_size).ceil() as i32;

    for x in 0..cols {
        for y in 0..rows {
            let px = canvas_rect.x + x as f32 * check_size;
            let py = canvas_rect.y + y as f32 * check_size;

            let color = if (x + y) % 2 == 0 {
                Color::LIGHTGRAY
            } else {
                Color::DARKGRAY
            };
            d.draw_rectangle(
                px as i32,
                py as i32,
                check_size as i32,
                check_size as i32,
                color,
            );
        }
    }

    // Draw the Canvas Texture
    // Essential: Flip vertically because OpenGL textures are inverted
    let tex_ref = texture.as_ref();
    let source_rec = Rectangle::new(0.0, 0.0, tex_ref.width as f32, -tex_ref.height as f32);
    d.draw_texture_pro(
        texture,
        source_rec,
        canvas_rect,
        Vector2::zero(),
        0.0,
        Color::WHITE,
    );

    // Draw Canvas Border
    d.draw_rectangle_lines_ex(canvas_rect, 2.0, Color::BLACK);
}

pub fn handle_canvas_drawing(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    canvas_rect: Rectangle,
    mouse_pos: Vector2,
    selected_color: Color,
    target: &mut RenderTexture2D,
    last_mouse_pos: Option<Vector2>,
) -> Option<Vector2> {
    if rl.is_mouse_button_down(MouseButton::MOUSE_BUTTON_LEFT) {
        if canvas_rect.check_collision_point_rec(mouse_pos) {
            // Calculate position relative to canvas top-left
            let rel_pos = mouse_pos - Vector2::new(canvas_rect.x, canvas_rect.y);

            // Draw onto the render texture
            let mut d = rl.begin_texture_mode(thread, target);

            if let Some(last) = last_mouse_pos {
                // Draw Line for continuity
                d.draw_line_ex(last, rel_pos, 4.0, selected_color);
                d.draw_circle_v(rel_pos, 2.0, selected_color); // Cap
            } else {
                d.draw_circle_v(rel_pos, 2.0, selected_color);
            }

            Some(rel_pos)
        } else {
            None
        }
    } else {
        None
    }
}
