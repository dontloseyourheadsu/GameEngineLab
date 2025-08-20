use crate::types::*;
use raylib::prelude::*;

pub struct Player {
    pub speed: i32,
    pub x: i32,
    pub y: i32,
    pub prev_x: f32,
    pub prev_y: f32,
    pub direction: Direction,
    pub next_direction: Direction,
    pub points: i32,
    pub lives: i32,
    pub respawn_speed: i32,
    pub is_dead: bool,
    pub animation: AnimationType,

    // Animation continuity tracking
    pub animation_frame_counter: i32,

    // Animation textures
    pub running_texture: Option<Texture2D>,
    pub standing_texture: Option<Texture2D>,
}

impl Player {
    pub fn new() -> Self {
        Player {
            speed: 30,
            x: 1,
            y: 1,
            prev_x: 1.0,
            prev_y: 1.0,
            direction: Direction::None,
            next_direction: Direction::None,
            points: 0,
            lives: 3,
            respawn_speed: 500,
            is_dead: false,
            animation: AnimationType::None,
            animation_frame_counter: 0,
            running_texture: None,
            standing_texture: None,
        }
    }

    pub fn load_textures(
        &mut self,
        rl: &mut RaylibHandle,
        thread: &RaylibThread,
    ) -> Result<(), String> {
        let running_img = Image::load_image("Resources/madoka-running.gif")
            .map_err(|e| format!("Failed to load running animation: {:?}", e))?;
        let standing_img = Image::load_image("Resources/madoka-standing.gif")
            .map_err(|e| format!("Failed to load standing animation: {:?}", e))?;

        self.running_texture = Some(
            rl.load_texture_from_image(thread, &running_img)
                .map_err(|e| format!("Failed to create running texture: {:?}", e))?,
        );
        self.standing_texture = Some(
            rl.load_texture_from_image(thread, &standing_img)
                .map_err(|e| format!("Failed to create standing texture: {:?}", e))?,
        );

        Ok(())
    }

    pub fn animate(&mut self, cell_size: i32, cnt_t: i32, d: &mut RaylibDrawHandle) {
        // Increment our continuous animation frame counter
        self.animation_frame_counter += 1;

        let animation_step = self.animation_frame_counter % self.speed;
        let step_size = 1.0 / self.speed as f32;
        let adjustment_factor = animation_step as f32 * step_size;

        if self.is_dead {
            self.prev_x = self.x as f32;
            self.prev_y = self.y as f32;
        }

        let mut interpolated_x = if self.prev_x != self.x as f32 {
            self.prev_x
        } else {
            self.x as f32
        };
        let mut interpolated_y = if self.prev_y != self.y as f32 {
            self.prev_y
        } else {
            self.y as f32
        };

        if self.prev_x != self.x as f32 || self.prev_y != self.y as f32 {
            match self.direction {
                Direction::Left => interpolated_x -= adjustment_factor,
                Direction::Right => interpolated_x += adjustment_factor,
                Direction::Up => interpolated_y -= adjustment_factor,
                Direction::Down => interpolated_y += adjustment_factor,
                Direction::None => {}
            }
        }

        interpolated_x *= cell_size as f32;
        interpolated_y *= cell_size as f32;

        if self.is_dead {
            self.draw_death(
                d,
                interpolated_x as i32,
                interpolated_y as i32,
                cell_size,
                cnt_t,
            );
            return;
        }

        // Update animation based on direction - only when direction changes
        // This preserves animation continuity across cell boundaries
        match self.direction {
            Direction::Left => {
                if self.animation == AnimationType::RightRun
                    || self.animation == AnimationType::None
                {
                    self.animation = AnimationType::LeftRun;
                    // Don't reset animation_frame_counter - let it continue
                }
            }
            Direction::Right => {
                if self.animation == AnimationType::LeftRun || self.animation == AnimationType::None
                {
                    self.animation = AnimationType::RightRun;
                    // Don't reset animation_frame_counter - let it continue
                }
            }
            Direction::None => {
                // Only change to standing if we were running
                if self.animation == AnimationType::LeftRun
                    || self.animation == AnimationType::RightRun
                {
                    self.animation = AnimationType::None;
                }
            }
            _ => {
                // For Up/Down, keep the current animation or set to standing
                if self.animation == AnimationType::None {
                    // If no animation is set, default to standing/none
                }
            }
        }

        self.draw_player(d, interpolated_x as i32, interpolated_y as i32, cell_size);
    }

    fn draw_player(&self, d: &mut RaylibDrawHandle, x: i32, y: i32, cell_size: i32) {
        if let Some(ref texture) = match self.animation {
            AnimationType::LeftRun | AnimationType::RightRun => &self.running_texture,
            _ => &self.standing_texture,
        } {
            let scale = cell_size as f32 / texture.height as f32;

            // For left movement, we flip by using negative width in destination
            let (dest_width, dest_height) = if self.animation == AnimationType::LeftRun {
                (
                    -(texture.width as f32 * scale),
                    texture.height as f32 * scale,
                )
            } else {
                (texture.width as f32 * scale, texture.height as f32 * scale)
            };

            let dest_rect = Rectangle::new(x as f32, y as f32, dest_width, dest_height);

            let source_rect = Rectangle::new(0.0, 0.0, texture.width as f32, texture.height as f32);

            d.draw_texture_pro(
                texture,
                source_rect,
                dest_rect,
                Vector2::zero(),
                0.0,
                Color::WHITE,
            );
        } else {
            // Fallback drawing if textures failed to load
            d.draw_circle(
                x + cell_size / 2,
                y + cell_size / 2,
                cell_size as f32 / 2.5,
                Color::YELLOW,
            );

            // Draw eyes
            let eye_size = cell_size / 8;
            let eye_offset = cell_size / 4;
            d.draw_circle(
                x + cell_size / 2 - eye_offset,
                y + cell_size / 3,
                eye_size as f32,
                Color::BLACK,
            );
            d.draw_circle(
                x + cell_size / 2 + eye_offset,
                y + cell_size / 3,
                eye_size as f32,
                Color::BLACK,
            );
        }
    }

    fn draw_death(&self, d: &mut RaylibDrawHandle, x: i32, y: i32, cell_size: i32, cnt_t: i32) {
        let base_pill_size = 10;
        let pill_size_variation = cnt_t % 4;
        let pill_size = base_pill_size + pill_size_variation;
        let offset_x = (cell_size - pill_size) / 2;
        let offset_y = (cell_size - pill_size) / 2;

        let pill_color = if cnt_t % 2 == 0 {
            Color::new(255, 182, 193, 255)
        } else {
            Color::new(255, 105, 180, 255)
        };

        d.draw_circle(
            x + offset_x + pill_size / 2,
            y + offset_y + pill_size / 2,
            pill_size as f32 / 2.0,
            pill_color,
        );
    }

    pub fn reset_for_next_life(&mut self) {
        self.x = 1;
        self.y = 1;
        self.prev_x = 1.0;
        self.prev_y = 1.0;
        self.direction = Direction::None;
        self.next_direction = Direction::None;
        self.animation = AnimationType::None;
        self.animation_frame_counter = 0; // Reset animation continuity on respawn
        self.is_dead = false;
    }

    // Point constants
    pub const PILL_POINT: i32 = 1;
    pub const POWER_PILL_POINT: i32 = 10;
    pub const GHOST_POINT: i32 = 100;
}
