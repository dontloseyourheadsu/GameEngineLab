use raylib::prelude::*;
mod constants;
mod scenes;
mod utilities;
use constants::game_details::*;
use scenes::home::{
    draw_checkerboard_tiles, draw_menu_card_background, draw_menu_title, handle_menu_action_buttons,
};

enum Scene {
    Home,
    Levels,
    Game(i32), // The i32 represents the current level
    Options,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum HomeButtonAction {
    Continue,
    Levels,
    Options,
    None,
}

fn main() {
    let (mut handler, thread) = raylib::init().size(10, 10).title(GAME_NAME).build();

    let monitor = get_current_monitor();

    if let Ok(monitor_info) = get_monitor_info(monitor) {
        let (window_width, window_height) = (monitor_info.width / 2, monitor_info.height / 2);

        handler.set_window_size(window_width, window_height);

        // Try to load a custom font, fall back to default font if not found
        let data: &[u8] = include_bytes!("./resources/chewy.ttf"); // path is compile-time
        let custom_font = handler
            .load_font_from_memory(&thread, ".ttf", data, 64, None)
            .expect("Failed to load embedded font");

        // The starting scene, during dev it can change, in prod it should be Scene::Home.
        let mut scene = Scene::Home;

        while !handler.window_should_close() {
            let mouse_pressed = handler.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT);
            let mouse_pos = if mouse_pressed {
                Some(handler.get_mouse_position())
            } else {
                None
            };

            let mut drawing = handler.begin_drawing(&thread);

            drawing.clear_background(Color::BLACK);

            match scene {
                Scene::Home => {
                    // Draw the background green tiles
                    draw_checkerboard_tiles(&mut drawing, window_width, window_height);

                    // Draw the menu card background
                    let card_menu = draw_menu_card_background(
                        &mut drawing,
                        window_width as f32,
                        window_height as f32,
                    );

                    // Draw the action buttons and get which button was clicked
                    let (button_section, button_action) = handle_menu_action_buttons(
                        &mut drawing,
                        card_menu,
                        &custom_font,
                        mouse_pressed,
                        mouse_pos,
                    );

                    // Draw the title
                    draw_menu_title(&mut drawing, card_menu, button_section, &custom_font);

                    // Handle scene changes based on button clicks
                    match button_action {
                        HomeButtonAction::Continue => {
                            scene = Scene::Game(1); // Start at level 1
                        }
                        HomeButtonAction::Levels => {
                            scene = Scene::Levels;
                        }
                        HomeButtonAction::Options => {
                            scene = Scene::Options;
                        }
                        HomeButtonAction::None => {} // No button was clicked
                    }
                }
                Scene::Levels => {
                    // Add some basic content for the Levels scene
                    drawing.draw_text(
                        "Levels Scene - Press ESC to go back to home",
                        10,
                        10,
                        20,
                        Color::WHITE,
                    );
                }
                Scene::Options => {
                    // Add some basic content for the Options scene
                    drawing.draw_text(
                        "Options Scene - Press ESC to go back to home",
                        10,
                        10,
                        20,
                        Color::WHITE,
                    );
                }
                Scene::Game(level) => {
                    // Add some basic content for the Game scene
                    drawing.draw_text(
                        &format!(
                            "Game Scene - Level {} - Press ESC to go back to home",
                            level
                        ),
                        10,
                        10,
                        20,
                        Color::WHITE,
                    );
                }
            }
        }
    }
}
