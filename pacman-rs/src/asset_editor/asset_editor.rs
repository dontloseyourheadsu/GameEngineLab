use raylib::prelude::*;

use crate::settings::GameSettings;

use super::canvas::{draw_canvas, handle_canvas_drawing};
use super::models::get_assets_definitions;
use super::ui::{EditorLayout, PALETTE_COLORS, draw_ui};

pub fn run_editor(rl: &mut RaylibHandle, thread: &RaylibThread, settings: &GameSettings) {
    let mut layout = EditorLayout::new(800, 600);
    let scale = settings.scale;

    rl.set_window_size(
        (layout.base_width as f32 * scale) as i32,
        (layout.base_height as f32 * scale) as i32,
    );
    rl.set_window_title(thread, "Pacman-rs - Asset Editor");

    let assets = get_assets_definitions();

    // Initialize storage
    let canvas_size = 256;
    let mut asset_storage: Vec<Vec<RenderTexture2D>> = Vec::new();
    for asset in &assets {
        let mut frames = Vec::new();
        for _ in 0..asset.frames {
            let mut rt = rl
                .load_render_texture(thread, canvas_size as u32, canvas_size as u32)
                .expect("Could not create render texture");
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
    let mut last_mouse_pos: Option<Vector2> = None;

    while !rl.window_should_close() {
        if rl.is_key_pressed(KeyboardKey::KEY_ESCAPE) {
            break;
        }

        // Handle Scaling
        let target_width = (layout.base_width as f32 * scale) as i32;
        let target_height = (layout.base_height as f32 * scale) as i32;
        if rl.get_screen_width() != target_width || rl.get_screen_height() != target_height {
            rl.set_window_size(target_width, target_height);
        }

        let mouse_pos = rl.get_mouse_position() / scale;

        // Dynamic Layout Calculation
        layout.update_dynamic_layout(assets[current_asset_idx].frames);

        // Input Handling
        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if layout.save_btn.check_collision_point_rec(mouse_pos) {
                println!("TODO: Save Asset Group");
                break;
            }
            if layout.discard_btn.check_collision_point_rec(mouse_pos) {
                break;
            }

            // Palette
            for (i, &color) in PALETTE_COLORS.iter().enumerate() {
                let box_rec = Rectangle::new(
                    layout.palette_x,
                    layout.palette_y + (i as f32 * (layout.color_box_size + layout.box_spacing)),
                    layout.color_box_size,
                    layout.color_box_size,
                );
                if box_rec.check_collision_point_rec(mouse_pos) {
                    selected_color = color;
                }
            }

            // Asset Switching
            if layout.arrow_up_rec.check_collision_point_rec(mouse_pos) {
                if current_asset_idx > 0 {
                    current_asset_idx -= 1;
                } else {
                    current_asset_idx = assets.len() - 1;
                }
                current_frame_idx = 0;
            }
            if layout.arrow_down_rec.check_collision_point_rec(mouse_pos) {
                if current_asset_idx < assets.len() - 1 {
                    current_asset_idx += 1;
                } else {
                    current_asset_idx = 0;
                }
                current_frame_idx = 0;
            }

            // Frame Switching
            for i in 0..assets[current_asset_idx].frames {
                let rec = Rectangle::new(
                    layout.frames_start_x
                        + i as f32 * (layout.frame_box_size + layout.frame_box_spacing),
                    layout.frames_y,
                    layout.frame_box_size,
                    layout.frame_box_size,
                );
                if rec.check_collision_point_rec(mouse_pos) {
                    current_frame_idx = i;
                }
            }
        }

        // Drawing Logic (Canvas)
        let target = &mut asset_storage[current_asset_idx][current_frame_idx];
        last_mouse_pos = handle_canvas_drawing(
            rl,
            thread,
            layout.canvas_rect,
            mouse_pos,
            selected_color,
            target,
            last_mouse_pos,
        );

        // Render
        let mut d = rl.begin_drawing(thread);
        d.clear_background(Color::RAYWHITE);

        {
            let mut d = d.begin_mode2D(Camera2D {
                offset: Vector2::zero(),
                target: Vector2::zero(),
                rotation: 0.0,
                zoom: scale,
            });

            draw_canvas(
                &mut d,
                asset_storage[current_asset_idx][current_frame_idx].texture(),
                layout.canvas_rect,
                canvas_size,
            );

            draw_ui(
                &mut d,
                &layout,
                mouse_pos,
                &assets,
                &asset_storage,
                current_asset_idx,
                current_frame_idx,
                selected_color,
            );
        }
    }
}
