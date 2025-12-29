use crate::{
    files::path_resource::PathResource, maps::map_2d_json_serializer::map_2d_json_serializer_system,
};
use bevy::app::{App, Plugin, Startup};

pub struct Map2DLoaderPlugin {
    pub path: String,
}

impl Plugin for Map2DLoaderPlugin {
    fn build(&self, app: &mut App) {
        app.insert_resource(PathResource(self.path.to_string()))
            .add_systems(Startup, map_2d_json_serializer_system);
    }
}
