use engine_core::tilemap::{Tilemap, Tileset};
use raylib::prelude::*;

/// Pacman-specific character to tile ID conversion
fn pacman_char_to_tile_id(ch: char) -> u32 {
    match ch {
        'w' => 1, // Wall
        '#' => 2, // Pill/Dot
        '"' => 3, // Power pill
        '1' => 4, // Ghost spawn 1
        '2' => 5, // Ghost spawn 2
        '3' => 6, // Ghost spawn 3
        '4' => 7, // Ghost spawn 4
        'e' => 8, // Empty space
        _ => 0,   // Default/unknown
    }
}

/// Creates a Pacman-style tileset texture
fn create_pacman_tileset_texture(
    rl: &mut RaylibHandle,
    thread: &RaylibThread,
    tile_size: i32,
) -> Result<Texture2D, String> {
    let tileset_width = tile_size * 9; // 9 different tile types (0-8)
    let tileset_height = tile_size;

    let mut tileset_image = Image::gen_image_color(tileset_width, tileset_height, Color::BLACK);

    // Tile 0: Default/Unknown (black)
    tileset_image.draw_rectangle(0, 0, tile_size, tile_size, Color::BLACK);

    // Tile 1: Wall (blue)
    tileset_image.draw_rectangle(tile_size, 0, tile_size, tile_size, Color::BLUE);

    // Tile 2: Pill/Dot (yellow circle on black background)
    tileset_image.draw_rectangle(tile_size * 2, 0, tile_size, tile_size, Color::BLACK);
    let pill_center_x = tile_size * 2 + tile_size / 2;
    let pill_center_y = tile_size / 2;
    tileset_image.draw_circle(pill_center_x, pill_center_y, 4, Color::YELLOW);

    // Tile 3: Power pill (larger yellow circle on black background)
    tileset_image.draw_rectangle(tile_size * 3, 0, tile_size, tile_size, Color::BLACK);
    let power_pill_center_x = tile_size * 3 + tile_size / 2;
    let power_pill_center_y = tile_size / 2;
    tileset_image.draw_circle(power_pill_center_x, power_pill_center_y, 8, Color::YELLOW);

    // Tile 4: Ghost spawn 1 (red)
    tileset_image.draw_rectangle(tile_size * 4, 0, tile_size, tile_size, Color::RED);

    // Tile 5: Ghost spawn 2 (pink)
    tileset_image.draw_rectangle(tile_size * 5, 0, tile_size, tile_size, Color::PINK);

    // Tile 6: Ghost spawn 3 (cyan)
    tileset_image.draw_rectangle(tile_size * 6, 0, tile_size, tile_size, Color::SKYBLUE);

    // Tile 7: Ghost spawn 4 (orange)
    tileset_image.draw_rectangle(tile_size * 7, 0, tile_size, tile_size, Color::ORANGE);

    // Tile 8: Empty space (dark gray)
    tileset_image.draw_rectangle(tile_size * 8, 0, tile_size, tile_size, Color::DARKGRAY);

    rl.load_texture_from_image(thread, &tileset_image)
        .map_err(|e| format!("Failed to create Pacman tileset: {}", e))
}

fn main() -> Result<(), Box<dyn std::error::Error>> {
    const WINDOW_WIDTH: i32 = 1400;
    const WINDOW_HEIGHT: i32 = 1200;

    let (mut rl, thread) = raylib::init()
        .size(WINDOW_WIDTH, WINDOW_HEIGHT)
        .title("Pacman RS")
        .build();

    rl.set_target_fps(60);
    rl.toggle_fullscreen();

    // Tile size in pixels
    let tile_size = 32.0;

    // Define the Pacman level using the provided character array
    let level = [
        [
            'w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', 'w', 'w', 'w',
        ],
        [
            'w', 'e', '#', '#', '#', '"', '#', '#', 'w', '"', '#', 'e', 'e', '1', 'w',
        ],
        [
            'w', '#', '#', '#', '#', '#', 'w', '#', 'w', '#', '#', 'w', 'e', '2', 'w',
        ],
        [
            'w', '#', 'w', 'w', 'w', '#', 'w', '#', '#', '#', '#', 'w', 'e', '3', 'w',
        ],
        [
            'w', '#', '#', 'w', '#', '#', 'w', '#', 'w', '#', '#', 'w', 'e', '4', 'w',
        ],
        [
            'w', '#', '#', '#', '#', '#', '#', '#', 'w', '#', '"', 'w', 'e', 'e', 'w',
        ],
        [
            'w', 'w', 'w', 'w', '#', '#', '#', '#', 'w', 'w', '#', 'w', 'w', 'w', 'w',
        ],
        [
            '#', '#', '#', '#', '"', 'w', '#', '#', '#', '#', '#', 'w', '#', '#', '#',
        ],
        [
            'w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', '#', '#', 'w', '#', 'w',
        ],
        [
            'w', 'w', '#', 'w', 'w', '#', '#', '#', 'w', '#', '#', 'w', '#', '#', 'w',
        ],
        [
            'w', 'w', '#', 'w', '#', '#', 'w', '#', '#', '#', '#', '#', '#', '#', 'w',
        ],
        [
            'w', '#', '#', '#', '#', '#', 'w', '#', 'w', 'w', '#', 'w', '#', '#', 'w',
        ],
        [
            'w', '#', 'w', 'w', 'w', '#', 'w', '#', 'w', 'w', '#', 'w', '#', '#', 'w',
        ],
        [
            'w', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', 'w',
        ],
        [
            'w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', 'w', 'w', 'w',
        ],
    ];

    // Convert level array to the format expected by tilemap
    let level_refs: Vec<&[char]> = level.iter().map(|row| row.as_slice()).collect();

    // Create tileset texture
    let tileset_texture = create_pacman_tileset_texture(&mut rl, &thread, tile_size as i32)?;

    // Create tileset and add tiles
    let mut tileset = Tileset::new(tileset_texture);
    tileset.add_tile(0, Rectangle::new(0.0, 0.0, tile_size, tile_size), false); // Default
    tileset.add_tile(
        1,
        Rectangle::new(tile_size, 0.0, tile_size, tile_size),
        true,
    ); // Wall
    tileset.add_tile(
        2,
        Rectangle::new(tile_size * 2.0, 0.0, tile_size, tile_size),
        false,
    ); // Pill
    tileset.add_tile(
        3,
        Rectangle::new(tile_size * 3.0, 0.0, tile_size, tile_size),
        false,
    ); // Power pill
    tileset.add_tile(
        4,
        Rectangle::new(tile_size * 4.0, 0.0, tile_size, tile_size),
        false,
    ); // Ghost 1
    tileset.add_tile(
        5,
        Rectangle::new(tile_size * 5.0, 0.0, tile_size, tile_size),
        false,
    ); // Ghost 2
    tileset.add_tile(
        6,
        Rectangle::new(tile_size * 6.0, 0.0, tile_size, tile_size),
        false,
    ); // Ghost 3
    tileset.add_tile(
        7,
        Rectangle::new(tile_size * 7.0, 0.0, tile_size, tile_size),
        false,
    ); // Ghost 4
    tileset.add_tile(
        8,
        Rectangle::new(tile_size * 8.0, 0.0, tile_size, tile_size),
        false,
    ); // Empty

    // Create the tilemap from the level array using the generic converter function
    let tilemap =
        Tilemap::from_char_array_with_converter(&level_refs, tileset, pacman_char_to_tile_id);

    // Calculate offset to center the map
    let map_width_pixels = tilemap.width as f32 * tile_size;
    let map_height_pixels = tilemap.height as f32 * tile_size;
    let offset_x = (WINDOW_WIDTH as f32 - map_width_pixels) / 2.0;
    let offset_y = (WINDOW_HEIGHT as f32 - map_height_pixels) / 2.0;

    // Game loop
    while !rl.window_should_close() {
        let mut d = rl.begin_drawing(&thread);

        d.clear_background(Color::BLACK);

        // Draw the tilemap centered on screen
        tilemap.draw_with_offset(&mut d, tile_size, offset_x, offset_y);

        // Draw title
        d.draw_text("Pacman RS - Tilemap Demo", 10, 10, 20, Color::WHITE);
    }

    Ok(())
}
