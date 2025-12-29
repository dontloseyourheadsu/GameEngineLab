use serde::Deserialize;

use crate::maps::tile_size::TileSize;

#[derive(Deserialize)]
pub struct MapJson {
    pub symbols: std::collections::HashMap<String, String>,
    pub tile_size: TileSize,
    pub map_data: String,
}
