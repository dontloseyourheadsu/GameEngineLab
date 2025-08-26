use raylib::prelude::*;
use engine_core::*;

fn main() {
    const SCREEN_WIDTH: i32 = 650;
    const SCREEN_HEIGHT: i32 = 350;

    let (mut rl, thread) = raylib::init()
        .size(SCREEN_WIDTH, SCREEN_HEIGHT)
        .title("Parallax RS - Engine Core Edition")
        .build();

    rl.set_target_fps(100);

    // Load background textures
    let layer1_texture = rl
        .load_texture(&thread, "Resources/B1.jpg")
        .expect("Failed to load B1.jpg");
    let layer2_texture = rl
        .load_texture(&thread, "Resources/B2.png")
        .expect("Failed to load B2.png");
    let layer3_texture = rl
        .load_texture(&thread, "Resources/B3.png")
        .expect("Failed to load B3.png");

    // Load player textures
    let player_hold_texture = rl
        .load_texture(&thread, "Resources/HomuraHold.gif")
        .expect("Failed to load HomuraHold.gif");
    let player_running_texture = rl
        .load_texture(&thread, "Resources/HomuraRunning.gif")
        .expect("Failed to load HomuraRunning.gif");

    // Create parallax background
    let screen_width_f32 = SCREEN_WIDTH as f32;
    let mut background = ParallaxBackground::new();

    // Add layers
    let layer1 = ParallaxLayer::new(
        layer1_texture,
        0.0,
        1.0,
        screen_width_f32,
        (SCREEN_HEIGHT - 50) as f32,
    );

    let layer2 = ParallaxLayer::new(
        layer2_texture,
        25.0,
        3.0,
        screen_width_f32,
        (SCREEN_HEIGHT - 200) as f32,
    );

    let layer3 = ParallaxLayer::new(
        layer3_texture,
        180.0,
        10.0,
        screen_width_f32,
        (SCREEN_HEIGHT - 225) as f32,
    );

    background.add_layer(layer1);
    background.add_layer(layer2);
    background.add_layer(layer3);

    // Create player as a VerletPoint with integrated RenderMesh
    let player_position = Vector2::new(285.0, 230.0);
    let mut player_render_mesh = RenderMesh::new();
    player_render_mesh.set_scale(Vector2::new(50.0, 80.0));
    
    let mut player = VerletPoint::new_with_render_mesh(
        player_position, 
        1.0, 
        50.0, 
        Color::WHITE, 
        player_render_mesh
    );

    // Control state
    let mut was_right_pressed = false;
    let mut is_running = false;

    // World bounds (not really used since player doesn't move, but required for update)
    let world_bounds = Rectangle::new(0.0, 0.0, SCREEN_WIDTH as f32, SCREEN_HEIGHT as f32);

    while !rl.window_should_close() {
        let delta_time = rl.get_frame_time();

        // Handle input
        let right_pressed = rl.is_key_down(KeyboardKey::KEY_RIGHT);

        // Detect key press and release events
        if right_pressed && !was_right_pressed {
            // Key just pressed - start moving background
            background.start_moving();
            is_running = true;
        } else if !right_pressed && was_right_pressed {
            // Key just released - stop moving background
            background.stop_moving();
            is_running = false;
        }

        was_right_pressed = right_pressed;

        // Update systems (player doesn't move, only background moves)
        background.update();
        player.update(delta_time, world_bounds); // No forces applied, so position stays same

        // Draw
        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::BLACK);

        // Draw background
        background.draw(&mut d);

        // Draw player with appropriate texture
        let current_texture = if is_running {
            Some(&player_running_texture)
        } else {
            Some(&player_hold_texture)
        };
        
        player.draw(&mut d, current_texture);
    }
}
