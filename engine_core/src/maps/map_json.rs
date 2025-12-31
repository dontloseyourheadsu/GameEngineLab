use std::collections::HashMap;

use serde::Deserialize;

#[derive(Deserialize)]
pub struct MapJson {
    pub tile_size: u8,
    pub symbols: HashMap<String, String>,
    pub textures_path: String,
    pub textures: HashMap<String, String>,
    pub map_data: String,
}
