use engine_core::maps::map_2d_json_serializer::map_2d_json_serializer_system;
use bevy::prelude::*;
use std::collections::HashMap;
use std::time::Duration;
use engine_core::{
    character::character_2d::Character2DBundle,
    maps::{map_2d_loader_plugin::Map2DLoaderPlugin, map_2d_resource::Map2DResource},
    physics::{
        collisions_2d::simple_collision_body::SimpleCollisionBody,
        velocity::Velocity,
    },
    rendering::sprites_2d::sprite_2d::Sprite2D,
};
use pacman::Pacman;
use pacman::controller::{MoveTimer, pacman_direction_control_system, pacman_continuous_movement_system};

pub mod pacman;

const PATH: &str = "data/map.json";
const TILE_SIZE: f32 = 32.0;

#[derive(Component)]
struct MapTile;

fn main() {
    let mut app = App::new();
    app.add_plugins((
        DefaultPlugins.set(ImagePlugin::default_nearest()), // Optional: Makes pixel art crisp
        Map2DLoaderPlugin {
            path: PATH.to_string(),
        },
    ))
    .insert_resource(MoveTimer(Timer::new(Duration::from_millis(200), TimerMode::Repeating)))
    .add_systems(Startup, setup_camera)
    .add_systems(Startup, setup_pacman.after(map_2d_json_serializer_system))
    .add_systems(
        Update,
        (
            render_map.run_if(resource_changed::<Map2DResource>),
            pacman_direction_control_system,
            pacman_continuous_movement_system,
        ),
    );
    app.run();
}

fn setup_camera(mut commands: Commands) {
    commands.spawn(Camera2dBundle::default());
}

fn setup_pacman(
    mut commands: Commands,
    asset_server: Res<AssetServer>,
    mut map_resource: ResMut<Map2DResource>,
) {
    let mut pacman_start_pos = Vec2::ZERO;
    let map_height = map_resource.map.data.len() as f32;
    let map_width = map_resource.map.data.first().map(|r| r.len()).unwrap_or(0) as f32;
    let offset_x = -(map_width * TILE_SIZE) / 2.0 + (TILE_SIZE / 2.0);
    let offset_y = (map_height * TILE_SIZE) / 2.0 - (TILE_SIZE / 2.0);

    let mut pacman_map_pos = IVec2::ZERO;

    for (y, row) in map_resource.map.data.iter().enumerate() {
        if let Some(x) = row.find('P') {
            pacman_start_pos.x = offset_x + (x as f32 * TILE_SIZE);
            pacman_start_pos.y = offset_y - (y as f32 * TILE_SIZE);
            pacman_map_pos.x = x as i32;
            pacman_map_pos.y = y as i32;
            break;
        }
    }

    if let Some(row) = map_resource.map.data.get_mut(pacman_map_pos.y as usize) {
        row.replace_range(pacman_map_pos.x as usize..pacman_map_pos.x as usize + 1, ".");
    }


    let pacman_texture_filename = map_resource.map.textures.get("pacman").expect("no pacman texture in map file");
    let clean_path = map_resource.map.textures_path.trim_start_matches("assets/");
    let base_path = if clean_path.ends_with('/') || clean_path.is_empty() {
        clean_path.to_string()
    } else {
        format!("{}/", clean_path)
    };
    let full_path = format!("{}{}", base_path, pacman_texture_filename);
    let texture = asset_server.load(full_path);

    let mut animation_mapper = HashMap::new();
    animation_mapper.insert("idle".to_string(), vec![0]);

    commands.spawn((
        Character2DBundle {
            velocity: Velocity(Vec2::ZERO),
            collision_body: SimpleCollisionBody::Box {
                width: TILE_SIZE,
                height: TILE_SIZE,
            },
            sprite: Sprite2D {
                animation_state: "idle".to_string(),
                animation_frame: 0,
                animation_mapper,
            },
            sprite_bundle: SpriteBundle {
                texture,
                transform: Transform::from_xyz(pacman_start_pos.x, pacman_start_pos.y, 100.0), // Set a very high Z-index to ensure it's on top
                sprite: Sprite {
                    custom_size: Some(Vec2::splat(TILE_SIZE)),
                    ..default()
                },
                ..default()
            },
        },
        Pacman,
        pacman::MoveDirection(Vec2::ZERO),
    ));
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
            if char == 'P' {
                continue;
            }
            let char_str = char.to_string();

            // Determine Texture and Color
            let (texture_handle, color) = if let Some(handle) = char_to_texture.get(&char_str) {
                // If texture exists: Use it, set color to WHITE (so it doesn't tint the texture)
                (handle.clone(), Color::WHITE)
            } else {
                // If no texture: No image, set color to BLACK
                (Default::default(), Color::BLACK)
            };

            commands.spawn((
                SpriteBundle {
                    texture: texture_handle,
                    sprite: Sprite {
                        color,
                        custom_size: Some(Vec2::splat(TILE_SIZE)),
                        ..default()
                    },
                    transform: Transform::from_xyz(
                        offset_x + (x as f32 * TILE_SIZE),
                        offset_y - (y as f32 * TILE_SIZE),
                        0.0,
                    ),
                    ..default()
                },
                MapTile,
            ));
        }
    }
}