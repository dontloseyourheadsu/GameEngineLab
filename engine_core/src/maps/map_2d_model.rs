use std::collections::HashMap;

use crate::rendering::coloring::texture_source::TextureSource;

#[derive(Debug)]
pub struct Map2DModel {
    pub tile_size: u8,
    pub data: Vec<String>,
    pub symbols: HashMap<String, String>, // Mapping of symbols to their meanings
    pub textures_path: String,            // Path to the textures directory
    pub textures: HashMap<String, TextureSource>, // Mapping of symbols to texture file names or colors
}
