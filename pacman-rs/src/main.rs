use bevy::prelude::*;
use rand::Rng;
use std::collections::HashMap;
use engine_core::maps::{map_2d_loader_plugin::Map2DLoaderPlugin, map_2d_resource::Map2DResource};

const PATH: &str = "data/map.json";
const TILE_SIZE: f32 = 32.0;

#[derive(Component)]
struct MapTile;

fn main() {
    let mut app = App::new();
    app.add_plugins((
        DefaultPlugins,
        Map2DLoaderPlugin { path: PATH.to_string() },
    ))
    .add_systems(Startup, setup_camera)
    // 2. ONLY run render_map if the resource was just added or changed
    .add_systems(
        Update, 
        render_map.run_if(resource_changed::<Map2DResource>),
    );
    app.run();
}

fn setup_camera(mut commands: Commands) {
    commands.spawn(Camera2d);
}

fn render_map(
    mut commands: Commands,
    map_resource: Res<Map2DResource>,
    // query to find old tiles if we need to clean up
    old_tiles: Query<Entity, With<MapTile>>, 
) {
    // 3. Despawn old map entities if the map reloaded
    for entity in old_tiles.iter() {
        commands.entity(entity).despawn();
    }

    let map = &map_resource.map;
    let mut rng = rand::rng();

    // Generate random colors for each symbol (Pre-calculate this!)
    let mut symbol_colors: HashMap<String, Color> = HashMap::new();
    for symbol in map.symbols.keys() {
        // Modern Bevy Color approach
        let color = Color::srgb(
            rng.random_range(0.0..1.0), 
            rng.random_range(0.0..1.0), 
            rng.random_range(0.0..1.0)
        );
        symbol_colors.insert(symbol.clone(), color);
    }

    // 4. Center the map on the screen (Optional, but looks better)
    let map_height = map.data.len() as f32;
    let map_width = map.data.first().map(|r| r.len()).unwrap_or(0) as f32;
    let offset_x = -(map_width * TILE_SIZE) / 2.0;
    let offset_y = (map_height * TILE_SIZE) / 2.0;

    // Render the map
    for (y, row) in map.data.iter().enumerate() {
        for (x, char) in row.chars().enumerate() {
            if let Some(color) = symbol_colors.get(&char.to_string()) {
                
                commands.spawn((
                    // 5. Use Sprite for simple 2D shapes
                    Sprite {
                        color: *color,
                        custom_size: Some(Vec2::splat(TILE_SIZE)),
                        ..default()
                    },
                    Transform::from_xyz(
                        offset_x + (x as f32 * TILE_SIZE), 
                        offset_y - (y as f32 * TILE_SIZE), 
                        0.0
                    ),
                    MapTile, // Add our marker component
                ));
            }
        }
    }
}