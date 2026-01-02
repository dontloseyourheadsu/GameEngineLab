use crate::pacman::pacman::Pacman;
use engine_core::maps::map_2d_model::Map2DModel;

pub enum PacmanEvent {
    None,
    EatenFood,
    EatenPill,
}

pub fn move_pacman(pacman: &mut Pacman, map: &mut Map2DModel) -> PacmanEvent {
    let tile_size = map.tile_size as f32;
    let (current_x, current_y) = pacman.grid_position;
    let mut event = PacmanEvent::None;

    // Check if desired direction is valid
    let desired_x = current_x as i32 + pacman.desired_direction.x as i32;
    let desired_y = current_y as i32 + pacman.desired_direction.y as i32;

    if desired_x >= 0 && desired_y >= 0 {
        if is_tile_walkable(desired_x as usize, desired_y as usize, map) {
            pacman.current_direction = pacman.desired_direction;
        }
    }

    // Move in the current direction if possible
    let next_x = current_x as i32 + pacman.current_direction.x as i32;
    let next_y = current_y as i32 + pacman.current_direction.y as i32;

    if next_x >= 0 && next_y >= 0 {
        let next_x_usize = next_x as usize;
        let next_y_usize = next_y as usize;

        if is_tile_walkable(next_x_usize, next_y_usize, map) {
            // Check for food or pills
            if let Some(row) = map.data.get(next_y_usize) {
                if let Some(tile) = row.chars().nth(next_x_usize) {
                    if tile == '.' {
                        event = PacmanEvent::EatenFood;
                    } else if tile == 'o' {
                        event = PacmanEvent::EatenPill;
                    }
                }
            }

            // Update Map: Clear old position
            if let Some(row) = map.data.get_mut(current_y) {
                row.replace_range(current_x..current_x + 1, " ");
            }

            // Update Map: Set new position
            if let Some(row) = map.data.get_mut(next_y_usize) {
                row.replace_range(next_x_usize..next_x_usize + 1, "P");
            }

            pacman.grid_position = (next_x_usize, next_y_usize);
            pacman.character.position.x = (next_x_usize as f32) * tile_size;
            pacman.character.position.y = (next_y_usize as f32) * tile_size;
        }
    }
    event
}

fn is_tile_walkable(x: usize, y: usize, map: &Map2DModel) -> bool {
    if y < map.data.len() {
        let row = &map.data[y];
        if x < row.len() {
            let tile = row.chars().nth(x).unwrap_or('#');
            return tile != '#';
        }
    }
    false
}
