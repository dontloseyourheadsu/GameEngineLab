use bevy::ecs::system::Resource;

use crate::maps::map_2d_model::Map2DModel;

#[derive(Resource)]
pub struct Map2DResource {
    pub map: Map2DModel,
}