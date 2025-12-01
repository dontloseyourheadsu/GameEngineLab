use raylib::prelude::*;
use super::texture::Texture;

pub enum Shape {
    Rectangle { width: f32, height: f32 },
    Circle { radius: f32 },
    Nothing,
}

pub struct Sprite2D {
    pub color: Color,
    pub shape: Shape,
}

impl Texture for Sprite2D {
    fn draw(&self, d: &mut RaylibDrawHandle, position: Vector2) {
        match self.shape {
            Shape::Rectangle { width, height } => {
                d.draw_rectangle_v(
                    position - Vector2::new(width / 2.0, height / 2.0),
                    Vector2::new(width, height),
                    self.color
                );
            },
            Shape::Circle { radius } => {
                d.draw_circle_v(position, radius, self.color);
            },
            Shape::Nothing => {}
        }
    }
}
