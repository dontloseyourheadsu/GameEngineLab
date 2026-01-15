use raylib::prelude::*;

use crate::settings::GameSettings;

use super::models::{get_tile_definitions, EMPTY_CHAR};
use super::storage::{evaluate_map, load_stored_map_groups, save_map_groups, MapGroup};

pub fn run_editor(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    settings: &GameSettings,
    mut map_group: MapGroup,
) {
    let base_width = 800;
    let base_height = 600;
    let scale = settings.scale;

    rl.set_window_size(
        (base_width as f32 * scale) as i32,
        (base_height as f32 * scale) as i32,
    );
    rl.set_window_title(
        thread,
        &format!("Pacman-rs - Map Editor - {}", map_group.name),
    );

    let tile_defs = get_tile_definitions();
    let mut selected_tile_idx = 0;

    let grid_area = Rectangle::new(20.0, 80.0, 520.0, 480.0);
    let palette_x = 560.0;
    let palette_y = 100.0;
    let palette_item_height = 36.0;

    while !rl.window_should_close() {
        if rl.is_key_pressed(KeyboardKey::KEY_ESCAPE) {
            break;
        }

        let target_width = (base_width as f32 * scale) as i32;
        let target_height = (base_height as f32 * scale) as i32;
        if rl.get_screen_width() != target_width || rl.get_screen_height() != target_height {
            rl.set_window_size(target_width, target_height);
        }

        let tile_size = compute_tile_size(&map_group, grid_area);
        let grid_rect = compute_grid_rect(&map_group, grid_area, tile_size);

        let mouse_pos = rl.get_mouse_position() / scale;

        let save_btn = Rectangle::new(560.0, 520.0, 220.0, 40.0);
        let back_btn = Rectangle::new(560.0, 565.0, 220.0, 30.0);

        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if save_btn.check_collision_point_rec(mouse_pos) {
                map_group.is_done = map_group.check_is_done();
                let mut all_groups = load_stored_map_groups();
                if let Some(pos) = all_groups.iter().position(|g| g.name == map_group.name) {
                    all_groups[pos] = map_group.clone();
                } else {
                    all_groups.push(map_group.clone());
                }
                save_map_groups(&all_groups);
                break;
            }

            if back_btn.check_collision_point_rec(mouse_pos) {
                break;
            }

            for (i, _tile) in tile_defs.iter().enumerate() {
                let item_rect = Rectangle::new(
                    palette_x,
                    palette_y + i as f32 * palette_item_height,
                    220.0,
                    palette_item_height - 4.0,
                );
                if item_rect.check_collision_point_rec(mouse_pos) {
                    selected_tile_idx = i;
                }
            }

            if grid_rect.check_collision_point_rec(mouse_pos) {
                let rel_x = mouse_pos.x - grid_rect.x;
                let rel_y = mouse_pos.y - grid_rect.y;
                let x = (rel_x / tile_size as f32) as usize;
                let y = (rel_y / tile_size as f32) as usize;
                if y < map_group.data.len() {
                    if let Some(row) = map_group.data.get_mut(y) {
                        set_cell(row, x, tile_defs[selected_tile_idx].symbol);
                    }
                }
            }
        }

        let validation = evaluate_map(&map_group.data);
        let warnings = validation.missing_messages();

        let mut d = rl.begin_drawing(thread);
        d.clear_background(Color::RAYWHITE);

        {
            let mut d = d.begin_mode2D(Camera2D {
                offset: Vector2::zero(),
                target: Vector2::zero(),
                rotation: 0.0,
                zoom: scale,
            });

            d.draw_text(
                &format!("Map Editor - {}", map_group.name),
                20,
                20,
                30,
                Color::DARKBLUE,
            );

            d.draw_text(
                &format!("Size: {} x {}", map_group.width, map_group.height),
                20,
                55,
                20,
                Color::DARKGRAY,
            );

            draw_grid(
                &mut d,
                &map_group.data,
                grid_rect,
                tile_size,
                EMPTY_CHAR,
            );

            draw_palette(
                &mut d,
                &tile_defs,
                selected_tile_idx,
                palette_x,
                palette_y,
                palette_item_height,
                mouse_pos,
            );

            draw_warnings(&mut d, &warnings, palette_x, 360.0);

            draw_button(&mut d, "Save", save_btn, mouse_pos);
            draw_button(&mut d, "Back", back_btn, mouse_pos);
        }
    }
}

fn compute_tile_size(map_group: &MapGroup, grid_area: Rectangle) -> i32 {
    let max_tile_width = grid_area.width / map_group.width.max(1) as f32;
    let max_tile_height = grid_area.height / map_group.height.max(1) as f32;
    let tile_size = max_tile_width.min(max_tile_height).floor().max(8.0);
    tile_size as i32
}

fn compute_grid_rect(map_group: &MapGroup, grid_area: Rectangle, tile_size: i32) -> Rectangle {
    let grid_width = map_group.width as f32 * tile_size as f32;
    let grid_height = map_group.height as f32 * tile_size as f32;
    let grid_x = grid_area.x + (grid_area.width - grid_width) / 2.0;
    let grid_y = grid_area.y + (grid_area.height - grid_height) / 2.0;
    Rectangle::new(grid_x, grid_y, grid_width, grid_height)
}

fn set_cell(row: &mut String, x: usize, symbol: char) {
    if x < row.len() {
        let replacement = symbol.to_string();
        row.replace_range(x..x + 1, &replacement);
    }
}

fn draw_grid(
    d: &mut RaylibDrawHandle,
    data: &[String],
    grid_rect: Rectangle,
    tile_size: i32,
    empty_symbol: char,
) {
    let font_size = (tile_size as f32 * 0.7).clamp(8.0, 20.0) as i32;
    for (y, row) in data.iter().enumerate() {
        for (x, ch) in row.chars().enumerate() {
            let cell_x = grid_rect.x + x as f32 * tile_size as f32;
            let cell_y = grid_rect.y + y as f32 * tile_size as f32;
            let cell_rect =
                Rectangle::new(cell_x, cell_y, tile_size as f32, tile_size as f32);
            d.draw_rectangle_rec(cell_rect, Color::WHITE);
            d.draw_rectangle_lines_ex(cell_rect, 1.0, Color::LIGHTGRAY);

            let mut buffer = [0; 4];
            let symbol = if ch == empty_symbol {
                " "
            } else {
                ch.encode_utf8(&mut buffer)
            };
            let text_width = d.measure_text(symbol, font_size);
            d.draw_text(
                symbol,
                cell_rect.x as i32 + (tile_size - text_width) / 2,
                cell_rect.y as i32 + (tile_size - font_size) / 2,
                font_size,
                Color::BLACK,
            );
        }
    }
}

fn draw_palette(
    d: &mut RaylibDrawHandle,
    tile_defs: &[super::models::TileDef],
    selected_idx: usize,
    palette_x: f32,
    palette_y: f32,
    item_height: f32,
    mouse_pos: Vector2,
) {
    d.draw_text("Tiles", palette_x as i32, (palette_y - 30.0) as i32, 20, Color::DARKBLUE);
    for (i, tile) in tile_defs.iter().enumerate() {
        let item_rect = Rectangle::new(palette_x, palette_y + i as f32 * item_height, 220.0, item_height - 4.0);
        let selected = i == selected_idx;
        let hover = item_rect.check_collision_point_rec(mouse_pos);
        let bg_color = if selected {
            Color::SKYBLUE
        } else if hover {
            Color::LIGHTGRAY
        } else {
            Color::WHITE
        };
        d.draw_rectangle_rec(item_rect, bg_color);
        d.draw_rectangle_lines_ex(item_rect, 1.0, Color::GRAY);

        let label = format!("{} ({})", tile.name, tile.symbol);
        d.draw_text(
            &label,
            (item_rect.x + 10.0) as i32,
            (item_rect.y + 6.0) as i32,
            18,
            Color::BLACK,
        );
    }
}

fn draw_warnings(d: &mut RaylibDrawHandle, warnings: &[String], x: f32, start_y: f32) {
    d.draw_text("Requirements", x as i32, start_y as i32, 20, Color::DARKBLUE);
    if warnings.is_empty() {
        d.draw_text("All requirements met.", x as i32, (start_y + 25.0) as i32, 18, Color::GREEN);
    } else {
        for (i, warning) in warnings.iter().enumerate() {
            d.draw_text(
                warning,
                x as i32,
                (start_y + 25.0 + i as f32 * 20.0) as i32,
                16,
                Color::MAROON,
            );
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
