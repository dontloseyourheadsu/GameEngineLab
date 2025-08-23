use raylib::prelude::*;
use crate::constants::game_details::*;
use crate::utilities::wrappers::*;

/// Draws a checkerboard pattern of tiles to fill the entire window.
///
/// # Arguments
/// 
/// * `drawing` - The Raylib drawing handle.
/// * `window_width` - The width of the window.
/// * `window_height` - The height of the window.
/// * `smallest_orientation` - The smallest orientation of the window (width or height).
pub fn draw_checkerboard_tiles(
    drawing: &mut RaylibDrawHandle,
    window_width: i32,
    window_height: i32
) {
    let tiles_count = 15;
    let tile_size = window_height / tiles_count;
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

/// Draws the menu card background.
/// 
/// # Arguments
/// 
/// * `drawing` - The Raylib drawing handle.
/// * `window_width` - The width of the window.
/// * `window_height` - The height of the window.
/// 
/// # Returns
///
/// A vector containing the x, y, width, and height of the menu card background.
///
/// # Examples
/// 
/// ```
/// let menu_card_background = draw_menu_card_background(drawing, window_width, window_height);
/// ```
pub fn draw_menu_card_background(
    drawing: &mut RaylibDrawHandle,
    window_width: f32,
    window_height: f32,
) -> Vector4 {
    // Draw a 80% tall 60% wide dark green rectangle in the middle
    let card_width = (window_width * 0.6) as i32;
    let card_height = (window_height * 0.8) as i32;

    let card_x = (window_width as i32 - card_width) / 2;
    let card_y = (window_height as i32 - card_height) / 2;

    drawing.draw_rectangle(card_x, card_y, card_width, card_height, Color::DARKGREEN);

    // Draw a 2-lined border, golden color for the rectangle. Top, bottom, right, left.
    drawing.draw_rectangle(card_x, card_y, card_width, 15, Color::GOLD);
    drawing.draw_rectangle(card_x + 30, card_y + 30, card_width - 60, 15, Color::GOLD);

    drawing.draw_rectangle(card_x, card_y + card_height - 15, card_width, 15, Color::GOLD);
    drawing.draw_rectangle(card_x + 30, card_y + card_height - 45, card_width - 60, 15, Color::GOLD);

    drawing.draw_rectangle(card_x, card_y, 15, card_height, Color::GOLD);
    drawing.draw_rectangle(card_x + 30, card_y + 30, 15, card_height - 60, Color::GOLD);

    drawing.draw_rectangle(card_x + card_width - 15, card_y, 15, card_height, Color::GOLD);
    drawing.draw_rectangle(card_x + card_width - 45, card_y + 30, 15, card_height - 60, Color::GOLD);

    Vector4::new(card_x as f32, card_y as f32, card_width as f32, card_height as f32)
}

pub fn draw_change_scene_button(
    drawing: &mut RaylibDrawHandle,
    rectangle: Vector4,
    label: &str,
    custom_font: &Font,
    mouse_click: bool,
    mouse_pos: Option<Vector2>
) {
    let x = rectangle.x as i32;
    let y = rectangle.y as i32;
    let width = rectangle.z as i32;
    let height = rectangle.w as i32;

    // Draw button background
    drawing.draw_rectangle(x, y, width, height, Color::DARKGREEN);

    // Draw button borders top, bottom, left, right
    drawing.draw_rectangle(x, y, width, 5, Color::GOLD);
    drawing.draw_rectangle(x, y + height - 5, width, 5, Color::GOLD);
    drawing.draw_rectangle(x, y, 5, height, Color::GOLD);
    drawing.draw_rectangle(x + width - 5, y, 5, height, Color::GOLD);

    let font_size = 60;
    let gap = 1.0;

    let button_center_x = x as f32 + (width as f32 / 2.0);
    let button_center_y = y as f32 + (height as f32 / 2.0);

    // Draw button label
    drawing.draw_text_ex(
        custom_font,
        label,
        Vector2::new(button_center_x - (measure_text_ex_safe(custom_font, label, font_size as f32, gap).x / 2.0), button_center_y - (font_size as f32 / 2.0)),
        font_size as f32,
        gap,
        Color::GOLD,
    );

    if mouse_click {
        if let Some(mouse_position) = mouse_pos {
            if mouse_position.x >= x as f32 && mouse_position.x <= (x + width) as f32 &&
               mouse_position.y >= y as f32 && mouse_position.y <= (y + height) as f32 {
                println!("Button '{}' clicked!", label);
                // Here you would change the scene based on the button clicked
            }
        }
    }
}

/// Draws the action buttons for the menu.
/// 
/// # Arguments
/// * `drawing` - The Raylib drawing handle.
/// * `menu_card` - A Vector4 representing the x, y, width, and height of the menu card.
/// * `custom_font` - The custom font to use for the buttons
/// * `handler` - The Raylib input handler
/// * `scene` - The current game scene
/// * `level` - The current game level
pub fn handle_menu_action_buttons(
    drawing: &mut RaylibDrawHandle,
    card_menu: Vector4,
    custom_font: &Font,
    mouse_click: bool,
    mouse_pos: Option<Vector2>
) -> Vector4 {
    let menu_card_width = card_menu.z;
    let menu_card_height = card_menu.w;
    
    let button_width = menu_card_width / 3.0;
    let button_height = 90.0;

    let bottom_margin = 20.0;

    // Calculate the center positions
    // X axis is the start position of the menu card + its width in the middle, minus half the button width
    // Y axis is the start position of the menu card + its height in the middle, minus half the button height and the border of the card (45.0)
    let center_x = (card_menu.x + (menu_card_width / 2.0)) - (button_width / 2.0);
    let center_y = card_menu.y + menu_card_height - button_height - 45.0 - bottom_margin;

    let options_button = Vector4::new(center_x, center_y, button_width, button_height);
    let levels_button = Vector4::new(center_x, center_y - button_height - bottom_margin, button_width, button_height);
    let continue_button = Vector4::new(center_x, center_y - 2.0 * button_height - 2.0 * bottom_margin, button_width, button_height);

    draw_change_scene_button(drawing, continue_button, "Continue", custom_font, mouse_click, mouse_pos);
    draw_change_scene_button(drawing, levels_button, "Levels", custom_font, mouse_click, mouse_pos);
    draw_change_scene_button(drawing, options_button, "Options", custom_font, mouse_click, mouse_pos);

    Vector4::new(card_menu.x, center_y - 2.0 * button_height - 2.0 * bottom_margin, menu_card_width, 3.0 * button_height + 2.0 * bottom_margin)
}

/// Draws the name of the game with custom font in card menu above the buttons.
/// 
/// # Arguments
/// * `drawing` - The Raylib drawing handle.
/// * `rect_x` - The x position of the menu card.
/// * `rect_y` - The y position of the menu card.
/// * `rect_width` - The width of the menu card.
/// * `rect_height` - The height of the menu card.
/// * `button_section_x` - The x position of the button section.
/// * `button_section_y` - The y position of the button section.
/// * `button_section_width` - The width of the button section.
/// * `button_section_height` - The height of the button section.
/// * `custom_font` - The custom font to use for the title.
/// 
/// # Returns
/// A tuple containing the x, y, width, and height of the title text.
/// 
/// # Examples
/// 
/// ```
/// let (x, y, width, height) = draw_menu_title(drawing, rect_x, rect_y, rect_width, rect_height, button_section_x, button_section_y, button_section_width, button_section_height, custom_font);
/// ```
pub fn draw_menu_title(
    drawing: &mut RaylibDrawHandle,
    card_menu: Vector4,
    button_section: Vector4,
    custom_font: &Font,
) {
    let title = GAME_NAME;
    let font_size = 150;
    let gap = 1.0;

    let rect_x = card_menu.x as i32;
    let rect_y = card_menu.y as i32;
    let rect_width = card_menu.z as i32;
    let rect_height = card_menu.w as i32;

    let button_section_y = button_section.y as i32;

    let text_width = measure_text_ex_safe(custom_font, title, font_size as f32, gap).x as i32;

    let title_x = (rect_x + rect_width / 2) - (text_width / 2);
    let title_y = (rect_y + rect_height / 2 - font_size / 2) - (button_section_y - rect_y) / 2;

    drawing.draw_text_ex(
        custom_font,
        title,
        Vector2::new(title_x as f32, title_y as f32),
        font_size as f32,
        gap,
        Color::GOLD,
    );
}