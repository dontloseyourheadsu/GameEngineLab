use raylib::prelude::*;
use crate::constants::game_details::*;

fn draw_checkerboard_tiles(
    drawing: &mut RaylibDrawHandle,
    window_width: i32,
    window_height: i32,
    smallest_orientation: i32,
) {
    let tiles_count = 15;
    let tile_size = smallest_orientation / tiles_count;
    let horizontal_tiles = window_width / tile_size;
    let vertical_tiles = window_height / tile_size;

    for i in 0..horizontal_tiles + 1 {
        for j in 0..vertical_tiles {
            let x = i * tile_size;
            let y = j * tile_size;

            if (i + j) % 2 == 0 {
                drawing.draw_rectangle(x, y, tile_size, tile_size, Color::GREEN);
            } else {
                drawing.draw_rectangle(x, y, tile_size, tile_size, Color::YELLOWGREEN);
            }
        }
    }
}

fn draw_menu_background(
    drawing: &mut RaylibDrawHandle,
    window_width: i32,
    window_height: i32,
) -> (i32, i32, i32, i32) {
    // Draw a 80% tall 60% wide dark green rectangle in the middle
    let rect_width = (window_width as f32 * 0.6) as i32;
    let rect_height = (window_height as f32 * 0.8) as i32;

    let rect_x = (window_width - rect_width) / 2;
    let rect_y = (window_height - rect_height) / 2;

    drawing.draw_rectangle(rect_x, rect_y, rect_width, rect_height, Color::DARKGREEN);

    // Draw a 2-lined border, golden color for the rectangle, 20px
    // Top lines
    drawing.draw_rectangle(rect_x, rect_y, rect_width, 15, Color::GOLD);
    drawing.draw_rectangle(rect_x + 30, rect_y + 30, rect_width - 60, 15, Color::GOLD);
    // Bottom lines
    drawing.draw_rectangle(
        rect_x,
        rect_y + rect_height - 15,
        rect_width,
        15,
        Color::GOLD,
    );
    drawing.draw_rectangle(
        rect_x + 30,
        rect_y + rect_height - 45,
        rect_width - 60,
        15,
        Color::GOLD,
    );
    // Left lines
    drawing.draw_rectangle(rect_x, rect_y, 15, rect_height, Color::GOLD);
    drawing.draw_rectangle(rect_x + 30, rect_y + 30, 15, rect_height - 60, Color::GOLD);
    // Right lines
    drawing.draw_rectangle(
        rect_x + rect_width - 15,
        rect_y,
        15,
        rect_height,
        Color::GOLD,
    );
    drawing.draw_rectangle(
        rect_x + rect_width - 45,
        rect_y + 30,
        15,
        rect_height - 60,
        Color::GOLD,
    );

    (rect_x, rect_y, rect_width, rect_height)
}

fn draw_menu_title(
    drawing: &mut RaylibDrawHandle,
    window_width: i32,
    rect_y: i32,
    rect_height: i32,
    custom_font: &Font,
) {
    let title = GAME_NAME;
    let font_size = 150;

    // For custom font, we'll use a simple approach since measure_text_ex doesn't exist
    // We'll estimate the width or just center it manually
    let estimated_text_width = title.len() as i32 * (font_size / 3); // Rough estimation
    let title_x = (window_width - estimated_text_width) / 2;
    let title_y = rect_y + rect_height / 2 - font_size / 2;
    drawing.draw_text_ex(
        custom_font,
        title,
        Vector2::new(title_x as f32, title_y as f32),
        font_size as f32,
        1.0,
        Color::GOLD,
    );
}

pub fn draw_home_menu(
    drawing: &mut RaylibDrawHandle,
    window_width: i32,
    window_height: i32,
    smallest_orientation: i32,
    custom_font: &Font,
) {
    draw_checkerboard_tiles(drawing, window_width, window_height, smallest_orientation);

    let (rect_x, rect_y, rect_width, rect_height) =
        draw_menu_background(drawing, window_width, window_height);

    draw_menu_title(drawing, window_width, rect_y, rect_height, custom_font);
}
