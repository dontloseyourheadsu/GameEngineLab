use raylib::prelude::*;
use crate::background::ParallaxLayer;

pub struct ParallaxBackground {
    pub layers: Vec<ParallaxLayer>,
    pub is_moving: bool,
}

impl ParallaxBackground {
    pub fn new() -> Self {
        ParallaxBackground {
            layers: Vec::new(),
            is_moving: false,
        }
    }

    pub fn add_layer(&mut self, layer: ParallaxLayer) {
        self.layers.push(layer);
    }

    pub fn add_layers(&mut self, mut layers: Vec<ParallaxLayer>) {
        self.layers.append(&mut layers);
    }

    pub fn update(&mut self) {
        if self.is_moving {
            for layer in &mut self.layers {
                layer.update();
            }
        }
    }

    pub fn draw(&self, d: &mut RaylibDrawHandle) {
        // Draw layers in order (back to front)
        for layer in &self.layers {
            layer.draw(d);
        }
    }

    pub fn start_moving(&mut self) {
        self.is_moving = true;
    }

    pub fn stop_moving(&mut self) {
        self.is_moving = false;
    }

    pub fn set_moving(&mut self, moving: bool) {
        self.is_moving = moving;
    }

    pub fn is_moving(&self) -> bool {
        self.is_moving
    }

    pub fn reset_all_positions(&mut self, screen_width: f32) {
        for layer in &mut self.layers {
            layer.reset_position(screen_width);
        }
    }

    pub fn set_all_speeds(&mut self, speed_multiplier: f32) {
        for (i, layer) in self.layers.iter_mut().enumerate() {
            // Apply different speed multipliers based on layer index to maintain parallax effect
            let base_speed = (i + 1) as f32;
            layer.set_speed(base_speed * speed_multiplier);
        }
    }

    pub fn enable_layer(&mut self, index: usize, enabled: bool) {
        if index < self.layers.len() {
            self.layers[index].set_enabled(enabled);
        }
    }

    pub fn get_layer_count(&self) -> usize {
        self.layers.len()
    }
}
