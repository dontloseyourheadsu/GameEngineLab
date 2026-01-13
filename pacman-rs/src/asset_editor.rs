use crate::settings::GameSettings;
use raylib::prelude::*;

pub fn run_editor(rl: &mut RaylibHandle, thread: &RaylibThread, settings: &GameSettings) {
    let base_width = 800;
    let base_height = 600;
    let scale = settings.scale;

    // Set initial window size
    rl.set_window_size(
        (base_width as f32 * scale) as i32,
        (base_height as f32 * scale) as i32,
    );
    rl.set_window_title(thread, "Pacman-rs - Asset Editor");

    let canvas_size = 256;
    let canvas_x = (base_width - canvas_size) / 2;
    let canvas_y = (base_height - canvas_size) / 2;
    let canvas_rect = Rectangle::new(
        canvas_x as f32,
        canvas_y as f32,
        canvas_size as f32,
        canvas_size as f32,
    );

    // Assets Definition
    struct AssetDef {
        name: &'static str,
        frames: usize,
    }
    let assets = vec![
        AssetDef {
            name: "Pacman",
            frames: 4,
        },
        AssetDef {
            name: "Ghost",
            frames: 1,
        },
        AssetDef {
            name: "Wall",
            frames: 1,
        },
        AssetDef {
            name: "Food",
            frames: 1,
        },
        AssetDef {
            name: "Pill",
            frames: 1,
        },
    ];

    // Initialize all render textures
    // We use a Vec<Vec<RenderTexture2D>> to store frames for each asset
    let mut asset_storage: Vec<Vec<RenderTexture2D>> = Vec::new();
    for asset in &assets {
        let mut frames = Vec::new();
        for _ in 0..asset.frames {
            let mut rt = rl
                .load_render_texture(thread, canvas_size as u32, canvas_size as u32)
                .expect("Could not create render texture");

            // Clear to transparent
            {
                let mut d = rl.begin_texture_mode(thread, &mut rt);
                d.clear_background(Color::BLANK);
            }
            frames.push(rt);
        }
        asset_storage.push(frames);
    }

    let mut current_asset_idx = 0;
    let mut current_frame_idx = 0;

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

    let mut last_mouse_pos: Option<Vector2> = None;

    // UI Constants
    let ui_base_y = canvas_rect.y + canvas_rect.height + 30.0;
    let frame_box_size = 50.0;
    let frame_box_spacing = 10.0;
    let arrow_size = 20.0;

    while !rl.window_should_close() {
        if rl.is_key_pressed(KeyboardKey::KEY_ESCAPE) {
            break;
        }

        // Handle Scaling Resizes logic if scale changed outside (not likely here)
        // or just enforce it
        let target_width = (base_width as f32 * scale) as i32;
        let target_height = (base_height as f32 * scale) as i32;
        if rl.get_screen_width() != target_width || rl.get_screen_height() != target_height {
            rl.set_window_size(target_width, target_height);
        }

        let mouse_pos_screen = rl.get_mouse_position();
        let mouse_pos = mouse_pos_screen / scale; // Logic coordinates

        // Calculate dynamic UI positions
        let current_frames_count = assets[current_asset_idx].frames;
        let total_frames_width = current_frames_count as f32 * frame_box_size
            + (current_frames_count as f32 - 1.0).max(0.0) * frame_box_spacing;

        // Center the frames block
        let frames_start_x = (base_width as f32 - total_frames_width) / 2.0;
        let frames_y = ui_base_y + 25.0; // Space for name

        // Arrows to the left of frames
        // Center arrows vertically relative to frame boxes?
        let arrow_x = frames_start_x - 40.0;
        let arrow_up_rec = Rectangle::new(arrow_x, frames_y, arrow_size, arrow_size);
        let arrow_down_rec = Rectangle::new(
            arrow_x,
            frames_y + arrow_size + 10.0,
            arrow_size,
            arrow_size,
        );

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

            // Asset Switching
            if arrow_up_rec.check_collision_point_rec(mouse_pos) {
                if current_asset_idx > 0 {
                    current_asset_idx -= 1;
                } else {
                    current_asset_idx = assets.len() - 1;
                }
                current_frame_idx = 0;
            }
            if arrow_down_rec.check_collision_point_rec(mouse_pos) {
                if current_asset_idx < assets.len() - 1 {
                    current_asset_idx += 1;
                } else {
                    current_asset_idx = 0;
                }
                current_frame_idx = 0;
            }

            // Frame Switching
            for i in 0..current_frames_count {
                let rec = Rectangle::new(
                    frames_start_x + i as f32 * (frame_box_size + frame_box_spacing),
                    frames_y,
                    frame_box_size,
                    frame_box_size,
                );
                if rec.check_collision_point_rec(mouse_pos) {
                    current_frame_idx = i;
                }
            }
        }

        // Handle Drawing (Drag)
        if rl.is_mouse_button_down(MouseButton::MOUSE_BUTTON_LEFT) {
            if canvas_rect.check_collision_point_rec(mouse_pos) {
                // Calculate position relative to canvas top-left
                let rel_pos = mouse_pos - Vector2::new(canvas_rect.x, canvas_rect.y);

                // Draw onto the render texture
                let target = &mut asset_storage[current_asset_idx][current_frame_idx];
                let mut d = rl.begin_texture_mode(thread, target);

                if let Some(last) = last_mouse_pos {
                    // Draw Line for continuity
                    d.draw_line_ex(last, rel_pos, 4.0, selected_color);
                    d.draw_circle_v(rel_pos, 2.0, selected_color); // Cap
                } else {
                    d.draw_circle_v(rel_pos, 2.0, selected_color);
                }

                last_mouse_pos = Some(rel_pos);
            } else {
                last_mouse_pos = None;
            }
        } else {
            last_mouse_pos = None;
        }

        // --- Drawing to Screen ---
        let mut d = rl.begin_drawing(thread);
        d.clear_background(Color::RAYWHITE);

        {
            let mut d = d.begin_mode2D(Camera2D {
                offset: Vector2::zero(),
                target: Vector2::zero(),
                rotation: 0.0,
                zoom: scale,
            });

            // Draw Header
            d.draw_text("Asset Editor", base_width / 2 - 60, 20, 20, Color::DARKGRAY);

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
            let texture = asset_storage[current_asset_idx][current_frame_idx].texture();
            let source_rec =
                Rectangle::new(0.0, 0.0, texture.width() as f32, -texture.height() as f32);
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

            // --- NEW UI ---
            // Recalculate layout variables for drawing as asset might have changed
            let current_frames_count = assets[current_asset_idx].frames;
            let total_frames_width = current_frames_count as f32 * frame_box_size
                + (current_frames_count as f32 - 1.0).max(0.0) * frame_box_spacing;
            let frames_start_x = (base_width as f32 - total_frames_width) / 2.0;

            // Asset Name
            let asset_name = assets[current_asset_idx].name;
            let name_width = d.measure_text(asset_name, 20);
            d.draw_text(
                asset_name,
                (base_width / 2) - (name_width / 2),
                ui_base_y as i32,
                20,
                Color::BLACK,
            );

            // Arrows
            // Up
            d.draw_rectangle_rec(arrow_up_rec, Color::LIGHTGRAY);
            d.draw_rectangle_lines_ex(arrow_up_rec, 1.0, Color::DARKGRAY);
            d.draw_triangle(
                Vector2::new(arrow_up_rec.x + arrow_size / 2.0, arrow_up_rec.y + 3.0),
                Vector2::new(arrow_up_rec.x + 3.0, arrow_up_rec.y + arrow_size - 3.0),
                Vector2::new(
                    arrow_up_rec.x + arrow_size - 3.0,
                    arrow_up_rec.y + arrow_size - 3.0,
                ),
                Color::BLACK,
            );

            // Down
            d.draw_rectangle_rec(arrow_down_rec, Color::LIGHTGRAY);
            d.draw_rectangle_lines_ex(arrow_down_rec, 1.0, Color::DARKGRAY);
            d.draw_triangle(
                Vector2::new(arrow_down_rec.x + 3.0, arrow_down_rec.y + 3.0),
                Vector2::new(
                    arrow_down_rec.x + arrow_size / 2.0,
                    arrow_down_rec.y + arrow_size - 3.0,
                ),
                Vector2::new(arrow_down_rec.x + arrow_size - 3.0, arrow_down_rec.y + 3.0),
                Color::BLACK,
            );

            // Frames
            for i in 0..current_frames_count {
                let rec = Rectangle::new(
                    frames_start_x + i as f32 * (frame_box_size + frame_box_spacing),
                    frames_y,
                    frame_box_size,
                    frame_box_size,
                );

                // Draw background check/white
                d.draw_rectangle_rec(rec, Color::RAYWHITE);

                // Draw Thumbnail
                let thumb_tex = asset_storage[current_asset_idx][i].texture();
                let thumb_src = Rectangle::new(
                    0.0,
                    0.0,
                    thumb_tex.width() as f32,
                    -thumb_tex.height() as f32,
                );
                d.draw_texture_pro(
                    thumb_tex,
                    thumb_src,
                    rec,
                    Vector2::zero(),
                    0.0,
                    Color::WHITE,
                );

                // Border
                if i == current_frame_idx {
                    d.draw_rectangle_lines_ex(rec, 3.0, Color::BLUE);
                } else {
                    d.draw_rectangle_lines_ex(rec, 1.0, Color::BLACK);
                }

                // Number
                let num_str = format!("{}", i + 1);
                let num_width = d.measure_text(&num_str, 10);
                d.draw_text(
                    &num_str,
                    (rec.x + rec.width / 2.0) as i32 - num_width / 2,
                    (rec.y + rec.height + 2.0) as i32,
                    10,
                    Color::BLACK,
                );
            }
        } // End Camera Mode
    }
}
