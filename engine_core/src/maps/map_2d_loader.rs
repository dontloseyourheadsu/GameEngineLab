use std::collections::HashMap;
use std::fs;

use crate::maps::{map_2d_model::Map2DModel, map_json::MapJson};

pub fn load_map_from_json(path: &str) -> Map2DModel {
    let content = fs::read_to_string(path).expect("Failed to read map file");
    let map_json: MapJson =
        serde_json::from_str(&content).expect("Failed to parse JSON map file");

    let data: Vec<String> = map_json
        .map_data
        .lines()
        .map(|line| line.to_string())
        .collect();

    let symbols: HashMap<String, String> = map_json.symbols;
    let textures_path = map_json.textures_path;
    let textures = map_json.textures;

    Map2DModel {
        tile_size: map_json.tile_size,
        data,
        symbols,
        textures_path,
        textures,
    }
}
