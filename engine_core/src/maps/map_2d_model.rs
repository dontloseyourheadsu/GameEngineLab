use bevy::ecs::component::Component;
use std::collections::HashMap;

#[derive(Component)]
pub struct Map2DModel {
    pub tile_width: usize,
    pub tile_height: usize,
    pub data: Vec<String>,
    pub symbols: HashMap<String, String>, // Mapping of symbols to their meanings
    pub textures_path: String,            // Path to the textures directory
    pub textures: HashMap<String, String>, // Mapping of symbols to texture file names
}
