use raylib::prelude::*;

use crate::settings::GameSettings;

use super::canvas::{draw_canvas, handle_canvas_drawing};
use super::models::get_assets_definitions;
use super::storage::{AssetGroup, load_stored_asset_groups, save_asset_groups};
use super::ui::{EditorLayout, PALETTE_COLORS, draw_ui};
use raylib::ffi;

pub fn run_editor(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    settings: &GameSettings,
    mut asset_group: AssetGroup,
) {
    let mut layout = EditorLayout::new(800, 600);
    let scale = settings.scale;

    rl.set_window_size(
        (layout.base_width as f32 * scale) as i32,
        (layout.base_height as f32 * scale) as i32,
    );
    // Include group name in title
    rl.set_window_title(
        thread,
        &format!("Pacman-rs - Asset Editor - {}", asset_group.name),
    );

    let assets_def = get_assets_definitions();

    // Initialize storage from input AssetGroup
    let canvas_size = 256;
    let mut asset_storage: Vec<Vec<RenderTexture2D>> = Vec::new();

    // We assume asset_group.assets matches the order of get_assets_definitions()
    // In a real app we'd map by name, but for this lab we trust the order or indices
    for (_i, asset_data) in asset_group.assets.iter().enumerate() {
        let mut frames = Vec::new();
        for (_j, frame_data) in asset_data.frames.iter().enumerate() {
            let mut rt = rl
                .load_render_texture(thread, canvas_size as u32, canvas_size as u32)
                .expect("Could not create render texture");

            // If frame_data is not empty (all zeros), load it into texture
            // To do this: Create Image from data -> Load Texture -> Draw Texture to RT
            // However, initializing from blank is easier if data is empty
            {
                let mut d = rl.begin_texture_mode(thread, &mut rt);
                d.clear_background(Color::BLANK);

                // If we have data, draw it
                // Check if arbitrary byte is different than 0 to optimize?
                // Nay, just try to load if we saved it.
                // Construct image from raw data
                if frame_data.len() == (canvas_size * canvas_size * 4) as usize {
                    // We need to be careful. Creating an image from Vec<u8> in raylib-rs might require
                    // interacting with FFI or specific loaders.
                    // Safe way: create image, loop pixels? Too slow.
                    // The `Image::from_raw` logic checks format.
                    // Let's rely on standard Image creation if possible or just use `from_image`.
                    // Raylib-rs doesn't easily expose raw buffer constructor without FFI sometimes.
                    // let img = Image::gen_image_color(canvas_size, canvas_size, Color::BLANK);
                    // Actually, `Image::load_image_from_mem` expects file format (png).
                    // `Image::from_data` ?
                }
            }
            // Hack for now: We won't pre-load pixels in this step because Raylib-rs complexity with raw buffers
            // without Image::from_pixels (which might not be public or easy).
            // Actually, let's fix this properly below.
            frames.push(rt);
        }
        asset_storage.push(frames);
    }

    // Correctly loading pixels:
    // Since we are in the main thread content, we can do GPU operations.
    // Iterating asset_group again to upload pixels
    for (i, asset_data) in asset_group.assets.iter().enumerate() {
        for (j, frame_data) in asset_data.frames.iter().enumerate() {
            // Only load if it's not empty? Or always load.
            let is_empty = frame_data.iter().all(|&x| x == 0);
            if !is_empty {
                unsafe {
                    let raw_img = ffi::GenImageColor(
                        canvas_size,
                        canvas_size,
                        ffi::Color {
                            r: 0,
                            g: 0,
                            b: 0,
                            a: 0,
                        },
                    );
                    if frame_data.len() == (canvas_size * canvas_size * 4) as usize {
                        std::ptr::copy_nonoverlapping(
                            frame_data.as_ptr(),
                            raw_img.data as *mut u8,
                            frame_data.len(),
                        );
                    }
                    let img = Image::from_raw(raw_img);

                    let tex = rl
                        .load_texture_from_image(thread, &img)
                        .expect("Failed to load texture from data");

                    let mut d = rl.begin_texture_mode(thread, &mut asset_storage[i][j]);
                    d.clear_background(Color::BLANK);
                    d.draw_texture(&tex, 0, 0, Color::WHITE);
                }
            }
        }
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

        // Dynamic Layout Calculation
        layout.update_dynamic_layout(assets_def[current_asset_idx].frames);

        let mouse_pos = rl.get_mouse_position() / scale;

        // Input Handling
        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if layout.save_btn.check_collision_point_rec(mouse_pos) {
                // Save Logic
                // Update asset_group struct with new frame data
                for (i, asset_datum) in asset_group.assets.iter_mut().enumerate() {
                    for (j, frame_datum) in asset_datum.frames.iter_mut().enumerate() {
                        // Get image from GPU
                        let texture = asset_storage[i][j].texture();

                        unsafe {
                            let raw_img = ffi::LoadImageFromTexture(*texture.as_ref());
                            // raw_img is ffi::Image { data, width, height, mipmaps, format }
                            // We access data directly
                            let size = (raw_img.width * raw_img.height * 4) as usize;
                            if !raw_img.data.is_null() {
                                let slice =
                                    std::slice::from_raw_parts(raw_img.data as *const u8, size);
                                *frame_datum = slice.to_vec();
                            } else {
                                *frame_datum = vec![0; size];
                            }

                            // Important: Unload image to prevent leak
                            ffi::UnloadImage(raw_img);
                        }
                    }
                }

                // Update Done Status
                asset_group.is_done = asset_group.check_is_done();

                // Save to Storage
                // We need to load ALL groups, replace/add current, and save back.
                let mut all_groups = load_stored_asset_groups();
                if let Some(pos) = all_groups.iter().position(|g| g.name == asset_group.name) {
                    all_groups[pos] = asset_group.clone();
                } else {
                    all_groups.push(asset_group.clone());
                }
                save_asset_groups(&all_groups);

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
                    current_asset_idx = assets_def.len() - 1;
                }
                current_frame_idx = 0;
            }
            if layout.arrow_down_rec.check_collision_point_rec(mouse_pos) {
                if current_asset_idx < assets_def.len() - 1 {
                    current_asset_idx += 1;
                } else {
                    current_asset_idx = 0;
                }
                current_frame_idx = 0;
            }

            // Frame Switching
            for i in 0..assets_def[current_asset_idx].frames {
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
                &assets_def,
                &asset_storage,
                current_asset_idx,
                current_frame_idx,
                selected_color,
            );
        }
    }
}
