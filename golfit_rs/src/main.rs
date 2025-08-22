use raylib::prelude::*;
mod scenes;
mod constants;
use scenes::home::draw_home_menu;
use constants::game_details::*;

enum Scene {
    Home,
    Levels,
    Game,
}

pub const GAME_NAME: &str = "GolfRs";

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

        while !handler.window_should_close() {
            let smallest_orientation = window_width.min(window_height); // Get the smallest window orientation

            let mut drawing = handler.begin_drawing(&thread);

            drawing.clear_background(Color::BLACK);

            match scene {
                Scene::Home => {
                    draw_home_menu(
                        &mut drawing,
                        window_width,
                        window_height,
                        smallest_orientation,
                        &custom_font,
                    );
                }
                Scene::Levels => {}
                Scene::Game => {}
            }
        }
    }
}
