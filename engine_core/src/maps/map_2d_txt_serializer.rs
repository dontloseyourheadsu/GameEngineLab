use bevy::prelude::*;
use std::fs;

use crate::maps::map_2d_model::Map2DModel;

#[derive(Resource)]
pub struct MapResource {
    pub map: Map2DModel,
}

#[derive(Resource)]
pub struct PathResource(pub String);

pub fn map_2d_txt_serializer_system(mut commands: Commands, path: Option<Res<PathResource>>) {
    if let Some(path) = path {
        let content = fs::read_to_string(&path.0).expect("Failed to read map file");
        let lines: Vec<String> = content
            .lines()
            .skip_while(|line| !line.starts_with("map="))
            .skip(1)
            .map(|line| line.to_string())
            .collect();

        let width = lines.first().map_or(0, |line| line.len());
        let height = lines.len();

        let map = Map2DModel {
            width,
            height,
            data: lines,
        };

        commands.insert_resource(MapResource { map });
    } else {
        eprintln!("PathResource is missing. Skipping map_2d_txt_serializer_system.");
    }
}
