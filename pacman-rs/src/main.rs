use raylib::prelude::*;

mod asset_editor;
mod clay_filter;
mod game;
mod ghosts;
mod pacman;

fn main() {
    let screen_width = 800;
    let screen_height = 600;

    let (mut rl, thread) = raylib::init()
        .size(screen_width, screen_height)
        .title("Pacman-rs - Main Menu")
        .build();

    rl.set_target_fps(60);

    while !rl.window_should_close() {
        // Ensure window is menu-sized
        if rl.get_screen_width() != screen_width || rl.get_screen_height() != screen_height {
            rl.set_window_size(screen_width, screen_height);
            rl.set_window_title(&thread, "Pacman-rs - Main Menu");
        }

        let mouse_pos = rl.get_mouse_position();
        let play_btn = Rectangle::new(300.0, 200.0, 200.0, 50.0);
        let asset_btn = Rectangle::new(300.0, 280.0, 200.0, 50.0);
        let map_btn = Rectangle::new(300.0, 360.0, 200.0, 50.0);

        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            if play_btn.check_collision_point_rec(mouse_pos) {
                game::run_game(&mut rl, &thread);
                // Skip the rest of the loop to avoid drawing menu frame immediately
                // and allow window resize logic to run at top of loop next time
                continue;
            }
            if asset_btn.check_collision_point_rec(mouse_pos) {
                asset_editor::run_editor(&mut rl, &thread);
                continue;
            }
            // Other buttons do nothing for now
        }

        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::RAYWHITE);

        let title = "PACMAN-RS";
        let title_w = d.measure_text(title, 50);
        d.draw_text(
            title,
            (screen_width - title_w) / 2,
            100,
            50,
            Color::DARKBLUE,
        );

        // Menu Buttons
        draw_button(&mut d, "Play", play_btn, mouse_pos);
        draw_button(&mut d, "Asset Editor", asset_btn, mouse_pos);
        draw_button(&mut d, "Map Editor", map_btn, mouse_pos);
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
