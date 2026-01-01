use crate::pacman::pacman::Pacman;
use engine_core::maps::map_2d_model::Map2DModel;

pub fn move_pacman(pacman: &mut Pacman, map: &Map2DModel) {
    let tile_size = map.tile_size as f32;
    let current_tile_x = (pacman.character.position.x / tile_size).round() as i32;
    let current_tile_y = (pacman.character.position.y / tile_size).round() as i32;

    // Check if desired direction is valid
    let desired_tile_x = current_tile_x + pacman.desired_direction.x as i32;
    let desired_tile_y = current_tile_y + pacman.desired_direction.y as i32;

    if is_tile_walkable(desired_tile_x, desired_tile_y, map) {
        pacman.current_direction = pacman.desired_direction;
    }

    // Move in the current direction if possible
    let next_tile_x = current_tile_x + pacman.current_direction.x as i32;
    let next_tile_y = current_tile_y + pacman.current_direction.y as i32;

    if is_tile_walkable(next_tile_x, next_tile_y, map) {
        pacman.character.position.x = (next_tile_x as f32) * tile_size;
        pacman.character.position.y = (next_tile_y as f32) * tile_size;
    }
}

fn is_tile_walkable(x: i32, y: i32, map: &Map2DModel) -> bool {
    if y >= 0 && (y as usize) < map.data.len() && x >= 0 && (x as usize) < map.data[y as usize].len()
    {
        let tile = map.data[y as usize].chars().nth(x as usize).unwrap_or(' ');
        return tile != '#' && tile != ' ';
    }
    false
}
