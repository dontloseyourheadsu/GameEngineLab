use bevy::prelude::*;
use std::collections::HashMap;
use std::fs;

use crate::{
    files::path_resource::PathResource,
    maps::{map_2d_model::Map2DModel, map_2d_resource::Map2DResource, map_json::MapJson},
};

pub fn map_2d_json_serializer_system(mut commands: Commands, path: Option<Res<PathResource>>) {
    if let Some(path) = path {
        let content = fs::read_to_string(&path.0).expect("Failed to read map file");
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

        let map = Map2DModel {
            width,
            height,
            data,
            symbols,
        };

        commands.insert_resource(Map2DResource { map });
    } else {
        eprintln!("PathResource is missing. Skipping map_2d_json_serializer_system.");
    }
}
