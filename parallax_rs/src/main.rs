use raylib::prelude::*;

struct ParallaxLayer {
    texture: Texture2D,
    x1: f32,
    x2: f32,
    y: f32,
    speed: f32,
    width: f32,
    height: f32,
}

impl ParallaxLayer {
    fn new(texture: Texture2D, y: f32, speed: f32, screen_width: f32, height: f32) -> Self {
        Self {
            texture,
            x1: 0.0,
            x2: screen_width,
            y,
            speed,
            width: screen_width,
            height,
        }
    }

    fn update(&mut self) {
        self.x1 -= self.speed;
        self.x2 -= self.speed;

        if self.x1 < -self.width {
            self.x1 = self.width - self.speed;
        }
        if self.x2 < -self.width {
            self.x2 = self.width - self.speed;
        }
    }

    fn draw(&self, d: &mut RaylibDrawHandle) {
        // Draw both instances of the texture with proper scaling
        let dest_rect1 = Rectangle::new(self.x1, self.y, self.width, self.height);

        let dest_rect2 = Rectangle::new(self.x2, self.y, self.width, self.height);

        let src_rect = Rectangle::new(
            0.0,
            0.0,
            self.texture.width as f32,
            self.texture.height as f32,
        );

        d.draw_texture_pro(
            &self.texture,
            src_rect,
            dest_rect1,
            Vector2::zero(),
            0.0,
            Color::WHITE,
        );

        d.draw_texture_pro(
            &self.texture,
            src_rect,
            dest_rect2,
            Vector2::zero(),
            0.0,
            Color::WHITE,
        );
    }
}

struct Player {
    hold_texture: Texture2D,
    running_texture: Texture2D,
    position: Vector2,
    size: Vector2,
    is_running: bool,
}

impl Player {
    fn new(
        hold_texture: Texture2D,
        running_texture: Texture2D,
        position: Vector2,
        size: Vector2,
    ) -> Self {
        Self {
            hold_texture,
            running_texture,
            position,
            size,
            is_running: false,
        }
    }

    fn set_running(&mut self, running: bool) {
        self.is_running = running;
    }

    fn draw(&self, d: &mut RaylibDrawHandle) {
        let texture = if self.is_running {
            &self.running_texture
        } else {
            &self.hold_texture
        };

        let dest_rect = Rectangle::new(self.position.x, self.position.y, self.size.x, self.size.y);

        let src_rect = Rectangle::new(0.0, 0.0, texture.width as f32, texture.height as f32);

        d.draw_texture_pro(
            texture,
            src_rect,
            dest_rect,
            Vector2::zero(),
            0.0,
            Color::WHITE,
        );
    }
}

fn main() {
    const SCREEN_WIDTH: i32 = 650;
    const SCREEN_HEIGHT: i32 = 350;

    let (mut rl, thread) = raylib::init()
        .size(SCREEN_WIDTH, SCREEN_HEIGHT)
        .title("Parallax RS")
        .build();

    rl.set_target_fps(100); // Equivalent to 10ms timer (100 FPS)

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

    // Create parallax layers with different speeds and positions
    // Making all layers fill the screen width to avoid gaps
    let screen_width_f32 = SCREEN_WIDTH as f32;

    let mut layer1 = ParallaxLayer::new(
        layer1_texture,
        0.0,
        1.0,
        screen_width_f32,
        (SCREEN_HEIGHT - 50) as f32, // 300 pixels height
    );

    let mut layer2 = ParallaxLayer::new(
        layer2_texture,
        25.0,
        3.0,
        screen_width_f32,
        (SCREEN_HEIGHT - 200) as f32, // 150 pixels height
    );

    let mut layer3 = ParallaxLayer::new(
        layer3_texture,
        180.0,
        10.0,
        screen_width_f32,
        (SCREEN_HEIGHT - 225) as f32, // 125 pixels height
    );

    // Create player - moved up by 0.75 of her height (60 pixels)
    let mut player = Player::new(
        player_hold_texture,
        player_running_texture,
        Vector2::new(285.0, 140.0), // Moved up 60 pixels (0.75 * 80)
        Vector2::new(50.0, 80.0),
    );

    let mut is_moving = false;
    let mut was_right_pressed = false;

    while !rl.window_should_close() {
        // Handle input
        let right_pressed = rl.is_key_down(KeyboardKey::KEY_RIGHT);

        // Detect key press and release events
        if right_pressed && !was_right_pressed {
            // Key just pressed
            is_moving = true;
            player.set_running(true);
        } else if !right_pressed && was_right_pressed {
            // Key just released
            is_moving = false;
            player.set_running(false);
        }

        was_right_pressed = right_pressed;

        // Update parallax layers if moving
        if is_moving {
            layer1.update();
            layer2.update();
            layer3.update();
        }

        // Draw
        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::BLACK);

        // Draw layers in order (back to front)
        layer1.draw(&mut d);
        layer2.draw(&mut d);
        layer3.draw(&mut d);

        // Draw player
        player.draw(&mut d);
    }
}
