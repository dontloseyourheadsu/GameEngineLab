use bevy::prelude::*;

#[derive(Component, Clone)]
pub enum SimpleCollisionBody {
    Circle { radius: f32 },
    Box { width: f32, height: f32 },
}
