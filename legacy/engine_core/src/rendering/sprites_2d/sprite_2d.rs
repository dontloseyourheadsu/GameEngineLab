use std::collections::HashMap;
use std::time::{Duration, Instant};
use raylib::prelude::*;

#[derive(Clone)]
pub struct Sprite2D {
    pub animation_state: String,
    pub animation_mapper: HashMap<String, Vec<usize>>,
    pub frame_width: f32,
    pub frame_height: f32,
    pub current_frame_index: usize,
    pub rotation: f32,
    pub flip_horizontal: bool,
    pub flip_vertical: bool,
    timer: Instant,
    animation_speed: Duration,
}

impl Sprite2D {
    pub fn new(
        frame_width: f32,
        frame_height: f32,
        animation_speed: Duration,
        animation_mapper: HashMap<String, Vec<usize>>,
    ) -> Self {
        Self {
            animation_state: "idle".to_string(),
            animation_mapper,
            frame_width,
            frame_height,
            current_frame_index: 0,
            rotation: 0.0,
            flip_horizontal: false,
            flip_vertical: false,
            timer: Instant::now(),
            animation_speed,
        }
    }

    pub fn update(&mut self) {
        if self.timer.elapsed() >= self.animation_speed {
            self.timer = Instant::now();
            if let Some(frames) = self.animation_mapper.get(&self.animation_state) {
                if !frames.is_empty() {
                    self.current_frame_index = (self.current_frame_index + 1) % frames.len();
                }
            }
        }
    }

    pub fn get_current_frame(&self) -> usize {
        if let Some(frames) = self.animation_mapper.get(&self.animation_state) {
            if !frames.is_empty() {
                return frames[self.current_frame_index];
            }
        }
        0
    }

    pub fn draw(
        &self,
        d: &mut RaylibDrawHandle,
        texture: &Texture2D,
        position: Vector2,
        tile_size: f32,
    ) {
        let current_frame = self.get_current_frame();
        let mut source_width = self.frame_width;
        if self.flip_horizontal {
            source_width = -source_width;
        }
        let mut source_height = self.frame_height;
        if self.flip_vertical {
            source_height = -source_height;
        }

        let source_rec = Rectangle::new(
            current_frame as f32 * self.frame_width,
            0.0,
            source_width,
            source_height,
        );
        let dest_rec = Rectangle::new(
            position.x + tile_size / 2.0,
            position.y + tile_size / 2.0,
            tile_size,
            tile_size,
        );
        d.draw_texture_pro(
            texture,
            source_rec,
            dest_rec,
            Vector2::new(tile_size / 2.0, tile_size / 2.0), // origin
            self.rotation,
            Color::WHITE,
        );
    }
}
