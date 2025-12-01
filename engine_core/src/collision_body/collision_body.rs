use raylib::prelude::*;

pub trait CollisionBody {
    fn update(&mut self, dt: f32, gravity: Vector2, friction: f32, screen_width: f32, screen_height: f32);
    fn get_position(&self) -> Vector2;
}
