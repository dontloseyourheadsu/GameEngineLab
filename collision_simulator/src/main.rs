use raylib::prelude::*;

fn main() {
    let (mut handler, thread) = raylib::init().size(600, 600).title(GAME_NAME).build();

    while !handler.window_should_close() {
        let mut drawing = handler.begin_drawing(&thread);
        drawing.clear_background(Color::BLACK);

        
    }
}
