use raylib::prelude::*;

pub trait Texture {
    fn draw(&self, d: &mut RaylibDrawHandle, position: Vector2);
}
