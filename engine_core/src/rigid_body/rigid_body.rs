use crate::collision_body::collision_body::CollisionBody;
use crate::textures::texture::Texture;
use raylib::prelude::*;

pub struct RigidBody<B: CollisionBody, T: Texture> {
    pub collision_body: B,
    pub texture: T,
}

impl<B: CollisionBody, T: Texture> RigidBody<B, T> {
    pub fn new(collision_body: B, texture: T) -> Self {
        Self {
            collision_body,
            texture,
        }
    }

    pub fn update(&mut self, dt: f32, gravity: Vector2, friction: f32, screen_width: f32, screen_height: f32) {
        self.collision_body.update(dt, gravity, friction, screen_width, screen_height);
    }

    pub fn draw(&self, d: &mut RaylibDrawHandle) {
        self.texture.draw(d, self.collision_body.get_position());
    }
}
