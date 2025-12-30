use bevy::prelude::*;
use std::collections::HashMap;
use std::fs;

use crate::{
    files::path_resource::PathResource,
    maps::{map_2d_model::Map2DModel, map_2d_resource::Map2DResource, map_json::MapJson},
};

pub fn map_2d_json_serializer_system(
    mut commands: Commands,
    path_resource: Res<PathResource>,
) {
    let content = fs::read_to_string(&path_resource.0).expect("Failed to read map file");
    let map_json: MapJson =
        serde_json::from_str(&content).expect("Failed to parse JSON map file");

    let width = map_json.tile_size.width;
    let height = map_json.tile_size.height;
    let data: Vec<String> = map_json
        .map_data
        .lines()
        .map(|line| line.to_string())
        .collect();

    let symbols: HashMap<String, String> = map_json.symbols; // Parse symbols mapping
    let textures_path = map_json.textures_path; // Parse textures path
    let textures = map_json.textures; // Parse textures mapping

    let map = Map2DModel {
        tile_width: width,
        tile_height: height,
        data,
        symbols,
        textures_path,
        textures,
    };

    commands.insert_resource(Map2DResource { map });
}
