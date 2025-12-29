use bevy::ecs::component::Component;
use std::collections::HashMap;

#[derive(Component)]
pub struct Map2DModel {
    pub width: usize,
    pub height: usize,
    pub data: Vec<String>,
    pub symbols: HashMap<String, String>, // Mapping of symbols to their meanings
}
