use raylib::math::Vector2;

use crate::{
    physics::{
        collisions_2d::simple_collision_body::SimpleCollisionBody, velocity::Velocity,
    },
    rendering::sprites_2d::sprite_2d::Sprite2D,
};

pub struct Character2D {
    pub position: Vector2,
    pub velocity: Velocity,
    pub collision_body: SimpleCollisionBody,
    pub sprite: Sprite2D,
}
