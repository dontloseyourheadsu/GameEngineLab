use std::collections::HashMap;

use serde::Deserialize;

use crate::maps::tile_size::TileSize;

#[derive(Deserialize)]
pub struct MapJson {
    pub symbols: HashMap<String, String>,
    pub textures_path: String,
    pub textures: HashMap<String, String>,
    pub tile_size: TileSize,
    pub map_data: String,
}
