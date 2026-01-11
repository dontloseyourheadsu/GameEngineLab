use raylib::prelude::*;

pub fn run_editor(rl: &mut RaylibHandle, thread: &RaylibThread) {
    let screen_width = 800;
    let screen_height = 600;

    // Set window for editor
    rl.set_window_size(screen_width, screen_height);
    rl.set_window_title(thread, "Pacman-rs - Asset Editor");

    let canvas_size = 256;
    let canvas_x = (screen_width - canvas_size) / 2;
    let canvas_y = (screen_height - canvas_size) / 2;
    let canvas_rect = Rectangle::new(
        canvas_x as f32,
        canvas_y as f32,
        canvas_size as f32,
        canvas_size as f32,
    );

    // Create a RenderTexture to draw on
    let mut target = rl
        .load_render_texture(thread, canvas_size as u32, canvas_size as u32)
        .expect("Could not create render texture");

    // Initialize target with transparent
    {
        let mut d = rl.begin_texture_mode(thread, &mut target);
        d.clear_background(Color::BLANK);
    }

    let mut selected_color = Color::BLACK;

    // Palette colors
    let palette_colors = vec![
        Color::RED,
        Color::ORANGE,
        Color::YELLOW,
        Color::GREEN,
        Color::BLUE,
        Color::PURPLE,
        Color::VIOLET,
        Color::BLACK,
        Color::WHITE,
    ]; // Using standard rainbowish + B&W

    let palette_x = canvas_rect.x + canvas_rect.width + 20.0;
    let palette_y = canvas_rect.y;
    let color_box_size = 30.0;
    let box_spacing = 5.0;

    let back_btn = Rectangle::new(20.0, 20.0, 100.0, 40.0);

    while !rl.window_should_close() {
        if rl.is_key_pressed(KeyboardKey::KEY_ESCAPE) {
            break;
        }

        let mouse_pos = rl.get_mouse_position();

        // Handle Back Button
        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if back_btn.check_collision_point_rec(mouse_pos) {
                break;
            }

            // Handle Palette Selection
            for (i, &color) in palette_colors.iter().enumerate() {
                let box_rec = Rectangle::new(
                    palette_x,
                    palette_y + (i as f32 * (color_box_size + box_spacing)),
                    color_box_size,
                    color_box_size,
                );
                if box_rec.check_collision_point_rec(mouse_pos) {
                    selected_color = color;
                }
            }
        }

        // Handle Drawing (Drag)
        if rl.is_mouse_button_down(MouseButton::MOUSE_BUTTON_LEFT) {
            if canvas_rect.check_collision_point_rec(mouse_pos) {
                // Calculate position relative to canvas top-left
                let rel_x = (mouse_pos.x - canvas_rect.x) as i32;
                let rel_y = (mouse_pos.y - canvas_rect.y) as i32;

                // Draw onto the render texture
                // Note: RenderTexture origin is Top-Left (0,0) as well for drawing commands
                let mut d = rl.begin_texture_mode(thread, &mut target);
                // Draw a small circle for the brush
                d.draw_circle(rel_x, rel_y, 2.0, selected_color);
            }
        }

        // --- Drawing to Screen ---
        let mut d = rl.begin_drawing(thread);
        d.clear_background(Color::RAYWHITE);

        // Draw Header
        d.draw_text(
            "Asset Editor",
            screen_width / 2 - 60,
            20,
            20,
            Color::DARKGRAY,
        );

        // Draw Back Button
        let btn_color = if back_btn.check_collision_point_rec(mouse_pos) {
            Color::SKYBLUE
        } else {
            Color::LIGHTGRAY
        };
        d.draw_rectangle_rec(back_btn, btn_color);
        d.draw_rectangle_lines_ex(back_btn, 2.0, Color::DARKGRAY);
        d.draw_text(
            "BACK",
            back_btn.x as i32 + 25,
            back_btn.y as i32 + 10,
            20,
            Color::BLACK,
        );

        // Draw Transparency Grid (Checkerboard)
        let check_size = 16.0;
        let cols = (canvas_size as f32 / check_size).ceil() as i32;
        let rows = (canvas_size as f32 / check_size).ceil() as i32;

        for x in 0..cols {
            for y in 0..rows {
                // Check bounds to not draw outside canvas area if not divisible (it is 256/16=16 though)
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
        let texture = target.texture();
        let source_rec = Rectangle::new(0.0, 0.0, texture.width() as f32, -texture.height() as f32);
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

        // Draw Palette
        for (i, &color) in palette_colors.iter().enumerate() {
            let box_rec = Rectangle::new(
                palette_x,
                palette_y + (i as f32 * (color_box_size + box_spacing)),
                color_box_size,
                color_box_size,
            );

            d.draw_rectangle_rec(box_rec, color);
            d.draw_rectangle_lines_ex(box_rec, 1.0, Color::BLACK);

            // Highlight selected
            // Simple color equality check
            let is_selected = color.r == selected_color.r
                && color.g == selected_color.g
                && color.b == selected_color.b
                && color.a == selected_color.a;

            if is_selected {
                d.draw_rectangle_lines_ex(
                    Rectangle::new(
                        box_rec.x - 3.0,
                        box_rec.y - 3.0,
                        box_rec.width + 6.0,
                        box_rec.height + 6.0,
                    ),
                    3.0,
                    Color::GOLD, // Gold outline for selection
                );
            }
        }

        // Draw current color indicator below palette
        d.draw_text(
            "Color:",
            palette_x as i32,
            (palette_y + (palette_colors.len() as f32 * (color_box_size + box_spacing))) as i32
                + 10,
            10,
            Color::BLACK,
        );
        d.draw_rectangle(
            palette_x as i32,
            (palette_y + (palette_colors.len() as f32 * (color_box_size + box_spacing))) as i32
                + 25,
            color_box_size as i32,
            color_box_size as i32,
            selected_color,
        );
        d.draw_rectangle_lines(
            palette_x as i32,
            (palette_y + (palette_colors.len() as f32 * (color_box_size + box_spacing))) as i32
                + 25,
            color_box_size as i32,
            color_box_size as i32,
            Color::BLACK,
        );
    }
}
