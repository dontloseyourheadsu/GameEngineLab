mod types;
mod player;
mod ghost;
mod rendering;
mod game_map;
mod game;

use game::Game;
use raylib::prelude::*;

fn main() -> Result<(), Box<dyn std::error::Error>> {
    const WINDOW_WIDTH: i32 = 800;
    const WINDOW_HEIGHT: i32 = 600;
    
    let (mut rl, thread) = raylib::init()
        .size(WINDOW_WIDTH, WINDOW_HEIGHT)
        .title("Pacman RS")
        .build();
    
    rl.set_target_fps(60);
    rl.toggle_fullscreen();
    
    // Initialize game
    let mut game = Game::new();
    
    // Load resources
    if let Err(e) = game.load_resources(&mut rl, &thread) {
        eprintln!("Failed to load game resources: {}", e);
        // Continue anyway - the game will use fallback rendering
    }
    
    // Game loop
    while !rl.window_should_close() {
        // Handle input
        game.handle_input(&rl);
        
        // Update game state
        game.update();
        
        // Draw everything
        let mut d = rl.begin_drawing(&thread);
        game.draw(&mut d);
    }
    
    Ok(())
}