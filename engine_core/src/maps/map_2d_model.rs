use bevy::ecs::component::Component;

#[derive(Component)]
pub struct Map2DModel {
    pub width: usize,
    pub height: usize,
    pub data: Vec<String>,
}