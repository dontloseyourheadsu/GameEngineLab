use super::asset_editor::run_editor;
use super::models::{get_assets_definitions, AssetGroupSummary};
use super::storage::{load_stored_asset_groups, AssetGroup};
use crate::settings::GameSettings;
use raylib::prelude::*;
use raylib::ffi;

pub fn load_asset_summaries(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
) -> (Vec<AssetGroupSummary>, Vec<AssetGroup>) {
    let stored_groups = load_stored_asset_groups();
    let mut summaries = Vec::new();

    for group in &stored_groups {
        let mut thumbnails = Vec::new();
        for asset in &group.assets {
            // Get first frame
            if let Some(frame_data) = asset.frames.first() {
                let width = asset.width;
                let height = asset.height;

                // Create image from raw data
                // Safety checks needed?
                if !frame_data.is_empty() {
                    unsafe {
                        let raw_img = ffi::GenImageColor(width, height, ffi::Color { r:0, g:0, b:0, a:0 });
                        if frame_data.len() == (width * height * 4) as usize {
                            std::ptr::copy_nonoverlapping(frame_data.as_ptr(), raw_img.data as *mut u8, frame_data.len());
                        }
                        
                        let img = Image::from_raw(raw_img);
                        let tex = rl
                            .load_texture_from_image(thread, &img)
                            .expect("Failed thumb texture");
                        thumbnails.push(tex);
                    }
                }
            }
        }

        summaries.push(AssetGroupSummary {
            name: group.name.clone(),
            thumbnails,
            is_done: group.is_done,
        });
    }

    (summaries, stored_groups)
}

pub fn run_asset_group_selector(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    settings: &GameSettings,
) {
    let base_width = 800;
    let base_height = 600;

    // Ensure window size
    let scale = settings.scale;
    let target_width = (base_width as f32 * scale) as i32;
    let target_height = (base_height as f32 * scale) as i32;

    rl.set_window_size(target_width, target_height);
    rl.set_window_title(thread, "Pacman-rs - Asset Group Selector");

    let (mut asset_summaries, mut asset_groups_data) = load_asset_summaries(rl, thread);
    let mut selected_index: Option<usize> = None;
    let mut scroll_offset = 0.0;

    let list_rect = Rectangle::new(100.0, 100.0, 600.0, 300.0);
    let item_height = 60.0;

    while !rl.window_should_close() {
        if rl.is_key_pressed(KeyboardKey::KEY_ESCAPE) {
            break;
        }

        // Handle Scaling update
        let current_scale = settings.scale;
        let t_w = (base_width as f32 * current_scale) as i32;
        let t_h = (base_height as f32 * current_scale) as i32;
        if rl.get_screen_width() != t_w || rl.get_screen_height() != t_h {
            rl.set_window_size(t_w, t_h);
        }

        // Layout calc
        let content_height = (asset_summaries.len() as f32 * item_height).max(list_rect.height);

        // Input - Buttons
        let mouse_pos = rl.get_mouse_position() / current_scale;
        let load_btn = Rectangle::new(100.0, 420.0, 150.0, 50.0);
        let new_btn = Rectangle::new(270.0, 420.0, 150.0, 50.0);
        let back_btn = Rectangle::new(440.0, 420.0, 150.0, 50.0);

        let mouse_wheel = rl.get_mouse_wheel_move();
        if list_rect.check_collision_point_rec(mouse_pos) {
            scroll_offset += mouse_wheel * 20.0;
        }

        // Clamp scroll
        // scroll_offset should be negative or zero to move content up
        // Range: [-(content_height - list_rect.height), 0]
        let max_scroll = (content_height - list_rect.height).max(0.0);
        scroll_offset = scroll_offset.clamp(-max_scroll, 0.0);

        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if back_btn.check_collision_point_rec(mouse_pos) {
                break;
            }
            if new_btn.check_collision_point_rec(mouse_pos) {
                // Open editor with new assets
                let name = format!("Group {}", asset_summaries.len() + 1);
                let new_group = AssetGroup::new_empty(name, &get_assets_definitions());

                run_editor(rl, thread, settings, new_group);

                // Reload
                let (new_summaries, new_data) = load_asset_summaries(rl, thread);
                asset_summaries = new_summaries;
                asset_groups_data = new_data;

                // Restore window properties after returning
                rl.set_window_size(t_w, t_h);
                rl.set_window_title(thread, "Pacman-rs - Asset Group Selector");
            }
            if load_btn.check_collision_point_rec(mouse_pos) {
                if let Some(idx) = selected_index {
                    // Logic to load would go here, currently just calling editor
                    if idx < asset_groups_data.len() {
                        let group = asset_groups_data[idx].clone();
                        run_editor(rl, thread, settings, group);

                        // Reload
                        let (new_summaries, new_data) = load_asset_summaries(rl, thread);
                        asset_summaries = new_summaries;
                        asset_groups_data = new_data;

                        rl.set_window_size(t_w, t_h);
                        rl.set_window_title(thread, "Pacman-rs - Asset Group Selector");
                    }
                }
            }

            // List Selection
            if list_rect.check_collision_point_rec(mouse_pos) {
                let rel_y = mouse_pos.y - list_rect.y - scroll_offset;
                if rel_y >= 0.0 && rel_y < content_height {
                    let idx = (rel_y / item_height) as usize;
                    if idx < asset_summaries.len() {
                        selected_index = Some(idx);
                    }
                }
            }
        }

        let mut d = rl.begin_drawing(thread);
        d.clear_background(Color::RAYWHITE);

        {
            let mut d = d.begin_mode2D(Camera2D {
                offset: Vector2::zero(),
                target: Vector2::zero(),
                rotation: 0.0,
                zoom: current_scale,
            });

            d.draw_text("Asset Groups", 100, 50, 30, Color::DARKBLUE);

            // Draw List
            d.draw_rectangle_rec(list_rect, Color::LIGHTGRAY);
            d.draw_rectangle_lines_ex(list_rect, 2.0, Color::DARKGRAY);

            {
                let mut d = d.begin_scissor_mode(
                    (list_rect.x * current_scale) as i32,
                    (list_rect.y * current_scale) as i32,
                    (list_rect.width * current_scale) as i32,
                    (list_rect.height * current_scale) as i32,
                );

                for (i, group) in asset_summaries.iter().enumerate() {
                    let item_y = list_rect.y + scroll_offset + (i as f32 * item_height);
                    let item_rect =
                        Rectangle::new(list_rect.x, item_y, list_rect.width, item_height);

                    let bg_color = if Some(i) == selected_index {
                        Color::SKYBLUE
                    } else {
                        Color::WHITE
                    };

                    // Contrast background
                    d.draw_rectangle_rec(item_rect, bg_color);
                    
                    let label = if group.is_done {
                        format!("{} (Done)", group.name)
                    } else {
                        format!("{} (WIP)", group.name)
                    };
                    
                    d.draw_text(
                        &label,
                        (item_rect.x + 10.0) as i32,
                        (item_rect.y + 20.0) as i32,
                        20,
                        Color::BLACK,
                    );
                    d.draw_rectangle_lines_ex(item_rect, 1.0, Color::GRAY);

                    // Draw thumbnails
                    // "display only a text and to the right the very first frame drawn per asset... very small"
                    let mut thumb_x = item_rect.x + 200.0;
                    let thumb_size = item_height - 10.0;

                    for tex in &group.thumbnails {
                        let src = Rectangle::new(0.0, 0.0, tex.width as f32, tex.height as f32);
                        let dest =
                            Rectangle::new(thumb_x, item_rect.y + 5.0, thumb_size, thumb_size);
                        d.draw_texture_pro(tex, src, dest, Vector2::zero(), 0.0, Color::WHITE);
                        thumb_x += thumb_size + 5.0;
                    }
                }
            }

            // Draw Buttons
            draw_button(&mut d, "Load", load_btn, mouse_pos);
            draw_button(&mut d, "New", new_btn, mouse_pos);
            draw_button(&mut d, "Back", back_btn, mouse_pos);
        }
    }
}

fn draw_button(d: &mut RaylibDrawHandle, text: &str, rec: Rectangle, mouse_pos: Vector2) {
    let hover = rec.check_collision_point_rec(mouse_pos);
    let color = if hover {
        Color::SKYBLUE
    } else {
        Color::LIGHTGRAY
    };

    d.draw_rectangle_rec(rec, color);
    d.draw_rectangle_lines_ex(rec, 2.0, Color::DARKGRAY);

    let text_width = d.measure_text(text, 20);
    d.draw_text(
        text,
        rec.x as i32 + (rec.width as i32 - text_width) / 2,
        rec.y as i32 + (rec.height as i32 - 20) / 2,
        20,
        Color::BLACK,
    );
}
