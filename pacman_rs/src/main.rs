use raylib::prelude::*;

fn main() -> Result<(), Box<dyn std::error::Error>> {
    const WINDOW_WIDTH: i32 = 1400;
    const WINDOW_HEIGHT: i32 = 1200;
    
    let (mut rl, thread) = raylib::init()
        .size(WINDOW_WIDTH, WINDOW_HEIGHT)
        .title("Pacman RS")
        .build();
    
    rl.set_target_fps(60);
    rl.toggle_fullscreen();
    
    // Game loop
    while !rl.window_should_close() {
        let mut d = rl.begin_drawing(&thread);

        d.clear_background(Color::BLACK);

    }
    
    Ok(())
}