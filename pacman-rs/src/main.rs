use bevy::prelude::*;
use engine_core::maps::map_2d_txt_serializer::{PathResource, map_2d_txt_serializer_system};

const PATH: &str = "src/map.txt";

fn main() {
    App::new()
        .insert_resource(PathResource(PATH.to_string()))
        .add_systems(Startup, map_2d_txt_serializer_system)
        .run();
}
