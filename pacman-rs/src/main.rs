mod asset_editor;
mod clay_filter;
mod game;
mod ghosts;
mod options_menu;
mod pacman;
mod settings;

use raylib::prelude::*;
use settings::GameSettings;

fn main() {
    let base_width = 800;
    let base_height = 600;

    let (mut rl, thread) = raylib::init()
        .size(base_width, base_height)
        .title("Pacman-rs - Main Menu")
        .build();

    rl.set_target_fps(60);

    // Initialize default settings
    let mut settings = GameSettings::default();

    while !rl.window_should_close() {
        let scale = settings.scale;

        // Ensure window is size correctly according to scale
        let target_width = (base_width as f32 * scale) as i32;
        let target_height = (base_height as f32 * scale) as i32;
        if rl.get_screen_width() != target_width || rl.get_screen_height() != target_height {
            rl.set_window_size(target_width, target_height);
            rl.set_window_title(&thread, "Pacman-rs - Main Menu");
        }

        let mouse_pos_screen = rl.get_mouse_position();
        let mouse_pos = mouse_pos_screen / scale;

        let play_btn = Rectangle::new(300.0, 150.0, 200.0, 50.0);
        let asset_btn = Rectangle::new(300.0, 220.0, 200.0, 50.0);
        let map_btn = Rectangle::new(300.0, 290.0, 200.0, 50.0);
        let options_btn = Rectangle::new(300.0, 360.0, 200.0, 50.0);

        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if play_btn.check_collision_point_rec(mouse_pos) {
                game::run_game(&mut rl, &thread);
                continue;
            }
            if asset_btn.check_collision_point_rec(mouse_pos) {
                asset_editor::run_editor(&mut rl, &thread, &settings);
                continue;
            }
            if options_btn.check_collision_point_rec(mouse_pos) {
                options_menu::run_options(&mut rl, &thread, &mut settings);
                continue;
            }
        }

        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::RAYWHITE);

        {
            let mut d = d.begin_mode2D(Camera2D {
                offset: Vector2::zero(),
                target: Vector2::zero(),
                rotation: 0.0,
                zoom: scale,
            });

            let title = "PACMAN-RS";
            let title_w = d.measure_text(title, 50);
            d.draw_text(title, (base_width - title_w) / 2, 50, 50, Color::DARKBLUE);

            // Menu Buttons
            draw_button(&mut d, "Play", play_btn, mouse_pos);
            draw_button(&mut d, "Asset Editor", asset_btn, mouse_pos);
            draw_button(&mut d, "Map Editor", map_btn, mouse_pos);
            draw_button(&mut d, "Options", options_btn, mouse_pos);
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
