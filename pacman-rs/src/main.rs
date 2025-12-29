use bevy::prelude::*;
use std::collections::HashMap;
use engine_core::maps::{map_2d_loader_plugin::Map2DLoaderPlugin, map_2d_resource::Map2DResource};

const PATH: &str = "data/map.json";
const TILE_SIZE: f32 = 32.0;

#[derive(Component)]
struct MapTile;

fn main() {
    let mut app = App::new();
    app.add_plugins((
        DefaultPlugins.set(ImagePlugin::default_nearest()), // Optional: Makes pixel art crisp
        Map2DLoaderPlugin { path: PATH.to_string() },
    ))
    .add_systems(Startup, setup_camera)
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
    old_tiles: Query<Entity, With<MapTile>>, 
    asset_server: Res<AssetServer>, // 1. Access the AssetServer
) {
    // Cleanup old tiles
    for entity in old_tiles.iter() {
        commands.entity(entity).despawn();
    }

    let map = &map_resource.map;

    // 2. Build Lookup: Character String ("#") -> Texture Handle
    let mut char_to_texture: HashMap<String, Handle<Image>> = HashMap::new();

    // Fix Path: Bevy defaults to "assets/", so we strip it if your JSON includes it.
    // "assets/textures/" -> "textures/"
    let clean_path = map.textures_path.trim_start_matches("assets/"); 
    // Ensure it ends with a slash for clean concatenation
    let base_path = if clean_path.ends_with('/') || clean_path.is_empty() {
        clean_path.to_string()
    } else {
        format!("{}/", clean_path)
    };

    // Join 'symbols' and 'textures' to map Char -> Image
    for (name, filename) in &map.textures {
        // Only load if filename is not empty
        if !filename.is_empty() {
            // Find the symbol char for this name (e.g. "wall" -> "#")
            if let Some(symbol_char) = map.symbols.get(name) {
                let full_path = format!("{}{}", base_path, filename);
                let handle = asset_server.load(full_path);
                char_to_texture.insert(symbol_char.clone(), handle);
            }
        }
    }

    // Center the map
    let map_height = map.data.len() as f32;
    let map_width = map.data.first().map(|r| r.len()).unwrap_or(0) as f32;
    
    // Calculate offset so (0,0) is at the center of the screen
    let offset_x = -(map_width * TILE_SIZE) / 2.0 + (TILE_SIZE / 2.0);
    let offset_y = (map_height * TILE_SIZE) / 2.0 - (TILE_SIZE / 2.0);

    // 3. Render Loop
    for (y, row) in map.data.iter().enumerate() {
        for (x, char) in row.chars().enumerate() {
            let char_str = char.to_string();
            
            // Determine Texture and Color
            let (texture_handle, color) = if let Some(handle) = char_to_texture.get(&char_str) {
                // If texture exists: Use it, set color to WHITE (so it doesn't tint the texture)
                (Some(handle.clone()), Color::WHITE)
            } else {
                // If no texture: No image, set color to BLACK
                (None, Color::BLACK)
            };

            commands.spawn((
                Sprite {
                    image: texture_handle.unwrap_or_default(), // Use texture or default empty handle
                    color, 
                    custom_size: Some(Vec2::splat(TILE_SIZE)),
                    ..default()
                },
                Transform::from_xyz(
                    offset_x + (x as f32 * TILE_SIZE), 
                    offset_y - (y as f32 * TILE_SIZE), 
                    0.0
                ),
                MapTile, 
            ));
        }
    }
}