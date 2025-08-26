use raylib::prelude::*;

/// Creates a solid color texture of specified dimensions
pub fn create_solid_color_texture(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    width: i32,
    height: i32,
    color: Color,
) -> Result<Texture2D, String> {
    let image = Image::gen_image_color(width, height, color);
    rl.load_texture_from_image(thread, &image)
        .map_err(|e| format!("Failed to create texture: {}", e))
}

/// Creates a texture atlas with individual colored tiles
/// Returns a single texture containing multiple tile graphics arranged horizontally
pub fn create_texture_atlas(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    tile_size: i32,
    tiles: &[(Color, Option<(i32, i32, Color)>)], // (background_color, optional (x, y, shape_color) for shapes)
) -> Result<Texture2D, String> {
    let tileset_width = tile_size * tiles.len() as i32;
    let tileset_height = tile_size;

    let mut tileset_image = Image::gen_image_color(tileset_width, tileset_height, Color::BLACK);

    for (i, (bg_color, shape)) in tiles.iter().enumerate() {
        let tile_x = i as i32 * tile_size;

        // Draw background
        tileset_image.draw_rectangle(tile_x, 0, tile_size, tile_size, *bg_color);

        // Draw optional shape (for circles, etc.)
        if let Some((offset_x, offset_y, shape_color)) = shape {
            let center_x = tile_x + tile_size / 2 + offset_x;
            let center_y = tile_size / 2 + offset_y;
            tileset_image.draw_circle(center_x, center_y, (tile_size / 8).min(8), *shape_color);
        }
    }

    rl.load_texture_from_image(thread, &tileset_image)
        .map_err(|e| format!("Failed to create tileset texture: {}", e))
}

/// Creates a square texture with a circle drawn on it
/// Useful for creating simple circular sprites or UI elements
pub fn create_circle_texture(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    size: i32,
    radius: i32,
    bg_color: Color,
    circle_color: Color,
) -> Result<Texture2D, String> {
    let mut image = Image::gen_image_color(size, size, bg_color);
    let center = size / 2;
    image.draw_circle(center, center, radius, circle_color);

    rl.load_texture_from_image(thread, &image)
        .map_err(|e| format!("Failed to create circle texture: {}", e))
}
