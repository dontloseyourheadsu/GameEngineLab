use raylib::prelude::*;

use crate::settings::GameSettings;

use super::editor::run_editor;
use super::models::{MAX_MAP_SIZE, MapGroupSummary};
use super::storage::{MapGroup, load_stored_map_groups};

pub fn load_map_summaries() -> (Vec<MapGroupSummary>, Vec<MapGroup>) {
    let stored_groups = load_stored_map_groups();
    let summaries = stored_groups
        .iter()
        .map(|group| MapGroupSummary {
            name: group.name.clone(),
            width: group.width,
            height: group.height,
            is_done: group.is_done,
        })
        .collect();
    (summaries, stored_groups)
}

pub fn run_map_group_selector(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    settings: &GameSettings,
) {
    let base_width = 800;
    let base_height = 600;
    let scale = settings.scale;

    rl.set_window_size(
        (base_width as f32 * scale) as i32,
        (base_height as f32 * scale) as i32,
    );
    rl.set_window_title(thread, "Pacman-rs - Map Selector");

    let (mut map_summaries, mut map_groups_data) = load_map_summaries();
    let mut selected_index: Option<usize> = None;
    let mut scroll_offset = 0.0;
    let mut new_width: usize = 20;
    let mut new_height: usize = 20;
    let mut hold_timer = 0.0;

    let list_rect = Rectangle::new(100.0, 90.0, 600.0, 260.0);
    let item_height = 50.0;

    while !rl.window_should_close() {
        if rl.is_key_pressed(KeyboardKey::KEY_ESCAPE) {
            break;
        }

        let current_scale = settings.scale;
        let target_width = (base_width as f32 * current_scale) as i32;
        let target_height = (base_height as f32 * current_scale) as i32;
        if rl.get_screen_width() != target_width || rl.get_screen_height() != target_height {
            rl.set_window_size(target_width, target_height);
        }

        let mouse_pos = rl.get_mouse_position() / current_scale;

        let content_height = (map_summaries.len() as f32 * item_height).max(list_rect.height);

        let mouse_wheel = rl.get_mouse_wheel_move();
        if list_rect.check_collision_point_rec(mouse_pos) {
            scroll_offset += mouse_wheel * 20.0;
        }

        let max_scroll = (content_height - list_rect.height).max(0.0);
        scroll_offset = scroll_offset.clamp(-max_scroll, 0.0);

        let dt = rl.get_frame_time();

        let width_minus = Rectangle::new(100.0, 380.0, 30.0, 30.0);
        let width_plus = Rectangle::new(250.0, 380.0, 30.0, 30.0);
        let height_minus = Rectangle::new(360.0, 380.0, 30.0, 30.0);
        let height_plus = Rectangle::new(510.0, 380.0, 30.0, 30.0);

        let load_btn = Rectangle::new(100.0, 440.0, 150.0, 50.0);
        let new_btn = Rectangle::new(270.0, 440.0, 150.0, 50.0);
        let back_btn = Rectangle::new(440.0, 440.0, 150.0, 50.0);

        if rl.is_mouse_button_down(MouseButton::MOUSE_BUTTON_LEFT) {
            let pressed = rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT);
            if hold_timer <= 0.0 || pressed {
                let mut changed = false;
                if width_minus.check_collision_point_rec(mouse_pos) {
                    new_width = new_width.saturating_sub(1).max(5);
                    changed = true;
                } else if width_plus.check_collision_point_rec(mouse_pos) {
                    new_width = (new_width + 1).min(MAX_MAP_SIZE);
                    changed = true;
                } else if height_minus.check_collision_point_rec(mouse_pos) {
                    new_height = new_height.saturating_sub(1).max(5);
                    changed = true;
                } else if height_plus.check_collision_point_rec(mouse_pos) {
                    new_height = (new_height + 1).min(MAX_MAP_SIZE);
                    changed = true;
                }

                if changed {
                    hold_timer = if pressed { 0.4 } else { 0.05 };
                }
            } else {
                hold_timer -= dt;
            }
        } else {
            hold_timer = 0.0;
        }

        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if back_btn.check_collision_point_rec(mouse_pos) {
                break;
            }
            if new_btn.check_collision_point_rec(mouse_pos) {
                let name = format!("Map {}", map_summaries.len() + 1);
                let new_group = MapGroup::new_empty(name, new_width, new_height);
                run_editor(rl, thread, settings, new_group);

                let (new_summaries, new_data) = load_map_summaries();
                map_summaries = new_summaries;
                map_groups_data = new_data;

                rl.set_window_size(target_width, target_height);
                rl.set_window_title(thread, "Pacman-rs - Map Selector");
            }
            if load_btn.check_collision_point_rec(mouse_pos) {
                if let Some(idx) = selected_index {
                    if idx < map_groups_data.len() {
                        let group = map_groups_data[idx].clone();
                        run_editor(rl, thread, settings, group);

                        let (new_summaries, new_data) = load_map_summaries();
                        map_summaries = new_summaries;
                        map_groups_data = new_data;

                        rl.set_window_size(target_width, target_height);
                        rl.set_window_title(thread, "Pacman-rs - Map Selector");
                    }
                }
            }

            if list_rect.check_collision_point_rec(mouse_pos) {
                let rel_y = mouse_pos.y - list_rect.y - scroll_offset;
                if rel_y >= 0.0 && rel_y < content_height {
                    let idx = (rel_y / item_height) as usize;
                    if idx < map_summaries.len() {
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

            d.draw_text("Map Groups", 100, 40, 30, Color::DARKBLUE);

            d.draw_rectangle_rec(list_rect, Color::LIGHTGRAY);
            d.draw_rectangle_lines_ex(list_rect, 2.0, Color::DARKGRAY);

            {
                let mut d = d.begin_scissor_mode(
                    (list_rect.x * current_scale) as i32,
                    (list_rect.y * current_scale) as i32,
                    (list_rect.width * current_scale) as i32,
                    (list_rect.height * current_scale) as i32,
                );

                for (i, group) in map_summaries.iter().enumerate() {
                    let item_y = list_rect.y + scroll_offset + (i as f32 * item_height);
                    let item_rect =
                        Rectangle::new(list_rect.x, item_y, list_rect.width, item_height);

                    let bg_color = if Some(i) == selected_index {
                        Color::SKYBLUE
                    } else {
                        Color::WHITE
                    };

                    d.draw_rectangle_rec(item_rect, bg_color);

                    let status = if group.is_done { "Done" } else { "WIP" };
                    let label = format!(
                        "{} ({}x{}) - {}",
                        group.name, group.width, group.height, status
                    );

                    d.draw_text(
                        &label,
                        (item_rect.x + 10.0) as i32,
                        (item_rect.y + 15.0) as i32,
                        20,
                        Color::BLACK,
                    );
                    d.draw_rectangle_lines_ex(item_rect, 1.0, Color::GRAY);
                }
            }

            d.draw_text("New map size", 100, 360, 20, Color::DARKGRAY);

            draw_button(&mut d, "-", width_minus, mouse_pos);
            draw_button(&mut d, "+", width_plus, mouse_pos);
            d.draw_text(&format!("Width: {}", new_width), 140, 385, 20, Color::BLACK);

            draw_button(&mut d, "-", height_minus, mouse_pos);
            draw_button(&mut d, "+", height_plus, mouse_pos);
            d.draw_text(
                &format!("Height: {}", new_height),
                400,
                385,
                20,
                Color::BLACK,
            );

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
