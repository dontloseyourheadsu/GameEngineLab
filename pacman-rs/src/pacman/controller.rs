use bevy::prelude::*;
use engine_core::maps::map_2d_resource::Map2DResource;
use crate::pacman::{MoveDirection, Pacman};

const TILE_SIZE: f32 = 32.0;

pub fn pacman_direction_control_system(
    keyboard_input: Res<ButtonInput<KeyCode>>,
    mut query: Query<&mut MoveDirection, With<Pacman>>,
) {
    if query.is_empty() {
        return;
    }
    let mut move_direction = query.single_mut();

    if keyboard_input.just_pressed(KeyCode::KeyA) {
        move_direction.0 = Vec2::new(-1.0, 0.0);
    } else if keyboard_input.just_pressed(KeyCode::KeyD) {
        move_direction.0 = Vec2::new(1.0, 0.0);
    } else if keyboard_input.just_pressed(KeyCode::KeyW) {
        move_direction.0 = Vec2::new(0.0, 1.0);
    } else if keyboard_input.just_pressed(KeyCode::KeyS) {
        move_direction.0 = Vec2::new(0.0, -1.0);
    }
}

pub fn pacman_continuous_movement_system(
    mut map_resource: ResMut<Map2DResource>,
    mut query: Query<(&mut Transform, &MoveDirection), With<Pacman>>,
    time: Res<Time>,
    mut timer: ResMut<MoveTimer>,
) {
    if query.is_empty() {
        return;
    }
    let (mut transform, move_direction) = query.single_mut();

    if move_direction.0 == Vec2::ZERO {
        return;
    }

    if timer.0.tick(time.delta()).just_finished() {
        let map = &mut map_resource.map;
        let map_height = map.data.len() as f32;
        let map_width = map.data.first().map(|r| r.len()).unwrap_or(0) as f32;
        let offset_x = -(map_width * TILE_SIZE) / 2.0 + (TILE_SIZE / 2.0);
        let offset_y = (map_height * TILE_SIZE) / 2.0 - (TILE_SIZE / 2.0);

        let current_map_x = ((transform.translation.x - offset_x) / TILE_SIZE).round() as isize;
        let current_map_y = ((offset_y - transform.translation.y) / TILE_SIZE).round() as isize;

        let target_map_x = current_map_x + move_direction.0.x as isize;
        let target_map_y = current_map_y - move_direction.0.y as isize;

        if target_map_y >= 0
            && (target_map_y as usize) < map.data.len()
            && target_map_x >= 0
            && (target_map_x as usize) < map.data[target_map_y as usize].len()
        {
            let tile = map.data[target_map_y as usize]
                .chars()
                .nth(target_map_x as usize)
                .unwrap_or(' ');

            if tile != '#' && tile != ' ' {
                let new_x = offset_x + target_map_x as f32 * TILE_SIZE;
                let new_y = offset_y - target_map_y as f32 * TILE_SIZE;
                transform.translation.x = new_x;
                transform.translation.y = new_y;

                if let Some(row) = map.data.get_mut(current_map_y as usize) {
                    row.replace_range(current_map_x as usize..current_map_x as usize + 1, ".");
                }
            }
        }
    }
}

#[derive(Resource)]
pub struct MoveTimer(pub Timer);