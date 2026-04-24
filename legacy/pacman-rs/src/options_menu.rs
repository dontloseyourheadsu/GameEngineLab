use crate::settings::GameSettings;
use raylib::prelude::*;

pub fn run_options(rl: &mut RaylibHandle, thread: &RaylibThread, settings: &mut GameSettings) {
    let base_width = 800;
    let base_height = 600;

    // Track pending changes
    let mut pending_settings = *settings;

    // We use the *current* acting settings for the UI scale/window size
    // until the user hits "Apply".
    let mut current_ui_scale = settings.scale;

    rl.set_window_size(
        (base_width as f32 * current_ui_scale) as i32,
        (base_height as f32 * current_ui_scale) as i32,
    );
    rl.set_window_title(thread, "Pacman-rs - Options");

    let back_btn = Rectangle::new(20.0, 20.0, 100.0, 40.0);

    // Slider configurations
    let slider_width = 300.0;
    let slider_height = 20.0;
    let start_x = (base_width as f32 - slider_width) / 2.0;
    let start_y = 200.0;
    let y_spacing = 80.0;

    let music_slider_rec = Rectangle::new(start_x, start_y, slider_width, slider_height);
    let sfx_slider_rec = Rectangle::new(start_x, start_y + y_spacing, slider_width, slider_height);
    let scale_slider_rec = Rectangle::new(
        start_x,
        start_y + y_spacing * 2.0,
        slider_width,
        slider_height,
    );

    // Apply Button
    let apply_btn = Rectangle::new(
        start_x + (slider_width - 100.0) / 2.0,
        start_y + y_spacing * 3.0,
        100.0,
        40.0,
    );

    let mut dragging_music = false;
    let mut dragging_sfx = false;
    let mut dragging_scale = false;

    while !rl.window_should_close() {
        if rl.is_key_pressed(KeyboardKey::KEY_ESCAPE) {
            break;
        }

        let mouse_pos_screen = rl.get_mouse_position();
        // Mouse logic is based on current UI scale
        let mouse_pos = mouse_pos_screen / current_ui_scale;

        // --- Update Logic ---

        // Back Button
        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if back_btn.check_collision_point_rec(mouse_pos) {
                break;
            }
        }

        // Handle Mouse Down
        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            let extended_music_rec = Rectangle::new(
                music_slider_rec.x - 10.0,
                music_slider_rec.y - 10.0,
                music_slider_rec.width + 20.0,
                music_slider_rec.height + 20.0,
            );
            if extended_music_rec.check_collision_point_rec(mouse_pos) {
                dragging_music = true;
            }

            let extended_sfx_rec = Rectangle::new(
                sfx_slider_rec.x - 10.0,
                sfx_slider_rec.y - 10.0,
                sfx_slider_rec.width + 20.0,
                sfx_slider_rec.height + 20.0,
            );
            if extended_sfx_rec.check_collision_point_rec(mouse_pos) {
                dragging_sfx = true;
            }

            let extended_scale_rec = Rectangle::new(
                scale_slider_rec.x - 10.0,
                scale_slider_rec.y - 10.0,
                scale_slider_rec.width + 20.0,
                scale_slider_rec.height + 20.0,
            );
            if extended_scale_rec.check_collision_point_rec(mouse_pos) {
                dragging_scale = true;
            }

            // Apply Button Click
            if apply_btn.check_collision_point_rec(mouse_pos) {
                // Update the actual settings
                *settings = pending_settings;
                settings.save();

                // Apply Scale Effect
                current_ui_scale = settings.scale;
                let target_w = (base_width as f32 * current_ui_scale) as i32;
                let target_h = (base_height as f32 * current_ui_scale) as i32;
                if rl.get_screen_width() != target_w || rl.get_screen_height() != target_h {
                    rl.set_window_size(target_w, target_h);
                }
            }
        }

        if rl.is_mouse_button_released(MouseButton::MOUSE_BUTTON_LEFT) {
            dragging_music = false;
            dragging_sfx = false;
            dragging_scale = false;
        }

        // Apply Dragging to Pending Settings
        if dragging_music {
            let mut val = (mouse_pos.x - music_slider_rec.x) / music_slider_rec.width;
            val = val.clamp(0.0, 1.0);
            pending_settings.music_volume = val;
        }

        if dragging_sfx {
            let mut val = (mouse_pos.x - sfx_slider_rec.x) / sfx_slider_rec.width;
            val = val.clamp(0.0, 1.0);
            pending_settings.sfx_volume = val;
        }

        if dragging_scale {
            let mut val = (mouse_pos.x - scale_slider_rec.x) / scale_slider_rec.width;
            val = val.clamp(0.0, 1.0);
            pending_settings.scale = 0.25 + (val * 1.75);
            // NOTE: We do NOT update current_ui_scale here. The window size remains same until Apply.
        }

        // --- Drawing ---
        let mut d = rl.begin_drawing(thread);
        d.clear_background(Color::RAYWHITE);

        {
            let mut d = d.begin_mode2D(Camera2D {
                offset: Vector2::zero(),
                target: Vector2::zero(),
                rotation: 0.0,
                zoom: current_ui_scale,
            });

            d.draw_text("Options", base_width / 2 - 60, 50, 40, Color::DARKBLUE);

            // Back Button
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

            // Apply Button
            let apply_hover = apply_btn.check_collision_point_rec(mouse_pos);
            let apply_color = if apply_hover {
                Color::GREEN
            } else {
                Color::LIGHTGRAY
            };
            d.draw_rectangle_rec(apply_btn, apply_color);
            d.draw_rectangle_lines_ex(apply_btn, 2.0, Color::DARKGRAY);
            d.draw_text(
                "APPLY",
                apply_btn.x as i32 + 20,
                apply_btn.y as i32 + 10,
                20,
                Color::BLACK,
            );

            // Sliders Helper
            let draw_slider = |d: &mut RaylibDrawHandle,
                               label: &str,
                               rec: Rectangle,
                               value: f32,
                               val_text: String| {
                d.draw_text(
                    label,
                    rec.x as i32,
                    (rec.y - 30.0) as i32,
                    20,
                    Color::DARKGRAY,
                );
                d.draw_rectangle_rec(rec, Color::LIGHTGRAY);
                d.draw_rectangle_lines_ex(rec, 1.0, Color::GRAY);

                // Filled part logic
                let normalized = if label.contains("Scale") {
                    (value - 0.25) / 1.75
                } else {
                    value
                };

                let fill_width = rec.width * normalized;
                d.draw_rectangle(
                    rec.x as i32,
                    rec.y as i32,
                    fill_width as i32,
                    rec.height as i32,
                    Color::BLUE,
                );

                // Handle
                d.draw_rectangle(
                    (rec.x + fill_width - 5.0) as i32,
                    (rec.y - 5.0) as i32,
                    10,
                    (rec.height + 10.0) as i32,
                    Color::DARKBLUE,
                );

                d.draw_text(
                    &val_text,
                    (rec.x + rec.width + 20.0) as i32,
                    rec.y as i32,
                    20,
                    Color::BLACK,
                );
            };

            // We draw the PENDING settings, so user can see what they are setting
            draw_slider(
                &mut d,
                "Music Volume",
                music_slider_rec,
                pending_settings.music_volume,
                format!("{:.0}%", pending_settings.music_volume * 100.0),
            );
            draw_slider(
                &mut d,
                "SFX Volume",
                sfx_slider_rec,
                pending_settings.sfx_volume,
                format!("{:.0}%", pending_settings.sfx_volume * 100.0),
            );
            draw_slider(
                &mut d,
                "Scale",
                scale_slider_rec,
                pending_settings.scale,
                format!("{:.2}x", pending_settings.scale),
            );
        }
    }
}
