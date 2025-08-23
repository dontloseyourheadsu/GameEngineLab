use raylib::prelude::*;
mod scenes;
mod constants;
use scenes::home::{
    draw_checkerboard_tiles,
    draw_menu_card_background,
    draw_menu_action_buttons,
    draw_menu_title
};
use constants::game_details::*;

struct Level {
    level: i32,
}

impl Level {
    fn new(level: i32) -> Self {
        Level { level }
    }

    fn set_last_played_level(&mut self) {
        self.level = 1; // For simplicity, always set to level 1
    }
}

enum Scene {
    Home,
    Levels,
    Game,
    Options,
}

fn main() {
    let (mut handler, thread) = raylib::init().size(10, 10).title(GAME_NAME).build();

    let monitor = get_current_monitor();

    if let Ok(monitor_info) = get_monitor_info(monitor) {
        let (window_width, window_height) = (monitor_info.width / 2, monitor_info.height / 2);

        handler.set_window_size(window_width, window_height);

        // Try to load a custom font, fall back to default font if not found
        let data: &[u8] = include_bytes!("./resources/chewy.ttf"); // path is compile-time
        let custom_font = handler.load_font_from_memory(&thread, ".ttf", data, 64, None)
            .expect("Failed to load embedded font");

        let scene = Scene::Home; // The starting scene, during dev it can change, in prod it should be Scene::Home.
        let level = Level::new(1); // Start at level 1

        while !handler.window_should_close() {
            let mouse_pos = if handler.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
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
                    let card_menu = draw_menu_card_background(&mut drawing, window_width as f32, window_height as f32);

                    // Draw the action buttons
                    let button_section = draw_menu_action_buttons(&mut drawing, card_menu, &custom_font);

                    // Draw the title
                    draw_menu_title(&mut drawing, card_menu, button_section, &custom_font);
                }
                Scene::Levels => {}
                Scene::Options => {}
                Scene::Game => {}
            }
        }
    }
}
