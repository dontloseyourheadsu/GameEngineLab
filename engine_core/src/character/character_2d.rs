use raylib::math::Vector2;

use crate::{
    physics::{
        collisions_2d::simple_collision_body::SimpleCollisionBody,
        velocity::Velocity,
    },
    rendering::sprites_2d::sprite_2d::Sprite2D,
};

pub struct Character2D {
    pub position: Vector2,
    pub velocity: Velocity,
    pub collision_body: SimpleCollisionBody,
    pub sprite: Sprite2D,
}

impl Character2D {
    pub fn check_collisions(characters: &[Character2D]) -> Vec<(usize, usize)> {
        let bodies: Vec<(Vector2, &SimpleCollisionBody)> = characters
            .iter()
            .map(|c| (c.position, &c.collision_body))
            .collect();

        crate::physics::collisions_2d::simple_collisions::check_simple_collisions(&bodies)
    }
}
