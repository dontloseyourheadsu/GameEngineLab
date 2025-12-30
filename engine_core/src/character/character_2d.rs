use crate::{
    physics::{
        collisions_2d::simple_collision_body::SimpleCollisionBody, velocity::Velocity,
    },
    rendering::sprites_2d::sprite_2d::Sprite2D,
};
use bevy::prelude::*;

#[derive(Bundle)]
pub struct Character2DBundle {
    pub velocity: Velocity,
    pub collision_body: SimpleCollisionBody,
    pub sprite: Sprite2D,
    pub sprite_bundle: SpriteBundle,
}
