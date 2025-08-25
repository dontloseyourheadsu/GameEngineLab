use raylib::prelude::*;

pub struct ParallaxLayer {
    pub texture: Texture2D,
    pub x1: f32,
    pub x2: f32,
    pub y: f32,
    pub speed: f32,
    pub width: f32,
    pub height: f32,
    pub enabled: bool,
}

impl ParallaxLayer {
    pub fn new(texture: Texture2D, y: f32, speed: f32, screen_width: f32, height: f32) -> Self {
        Self {
            texture,
            x1: 0.0,
            x2: screen_width,
            y,
            speed,
            width: screen_width,
            height,
            enabled: true,
        }
    }

    pub fn new_with_position(
        texture: Texture2D,
        x: f32,
        y: f32,
        speed: f32,
        width: f32,
        height: f32,
    ) -> Self {
        Self {
            texture,
            x1: x,
            x2: x + width,
            y,
            speed,
            width,
            height,
            enabled: true,
        }
    }

    pub fn update(&mut self) {
        if !self.enabled {
            return;
        }

        self.x1 -= self.speed;
        self.x2 -= self.speed;

        if self.x1 < -self.width {
            self.x1 = self.width - self.speed;
        }
        if self.x2 < -self.width {
            self.x2 = self.width - self.speed;
        }
    }

    pub fn draw(&self, d: &mut RaylibDrawHandle) {
        if !self.enabled {
            return;
        }

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

    pub fn set_speed(&mut self, speed: f32) {
        self.speed = speed;
    }

    pub fn set_enabled(&mut self, enabled: bool) {
        self.enabled = enabled;
    }

    pub fn reset_position(&mut self, screen_width: f32) {
        self.x1 = 0.0;
        self.x2 = screen_width;
    }
}
