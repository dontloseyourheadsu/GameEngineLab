use bevy::prelude::*;

pub mod controller;

#[derive(Component)]
pub struct Pacman;

#[derive(Component)]
pub struct MoveDirection(pub Vec2);
