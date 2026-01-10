use crate::pacman::pacman::Pacman;
use engine_core::maps::map_2d_model::Map2DModel;
use raylib::prelude::*;

pub enum PacmanEvent {
    None,
    EatenFood,
    EatenPill,
}

pub fn move_pacman(pacman: &mut Pacman, map: &mut Map2DModel) -> PacmanEvent {
    let tile_size = map.tile_size as f32;
    let (current_x, current_y) = pacman.grid_position;
    let mut event = PacmanEvent::None;

    // Try to update direction
    if let Some(_) = get_next_position(current_x, current_y, pacman.desired_direction, map) {
        pacman.current_direction = pacman.desired_direction;
    }

    // Move in current direction
    if let Some((next_x, next_y)) =
        get_next_position(current_x, current_y, pacman.current_direction, map)
    {
        // Check for food or pills
        if let Some(row) = map.data.get(next_y) {
            if let Some(tile) = row.chars().nth(next_x) {
                if tile == '.' {
                    event = PacmanEvent::EatenFood;
                    // Replace with space
                    if let Some(row_mut) = map.data.get_mut(next_y) {
                        row_mut.replace_range(next_x..next_x + 1, " ");
                    }
                } else if tile == 'o' {
                    event = PacmanEvent::EatenPill;
                    // Replace with space
                    if let Some(row_mut) = map.data.get_mut(next_y) {
                        row_mut.replace_range(next_x..next_x + 1, " ");
                    }
                }
            }
        }

        // Move Pacman
        pacman.grid_position = (next_x, next_y);
        pacman.character.position.x = (next_x as f32) * tile_size;
        pacman.character.position.y = (next_y as f32) * tile_size;
    }
    event
}

fn get_next_position(
    current_x: usize,
    current_y: usize,
    direction: Vector2,
    map: &Map2DModel,
) -> Option<(usize, usize)> {
    let rows = map.data.len() as i32;
    if rows == 0 {
        return None;
    }
    let cols = map.data[0].len() as i32;

    let nx = current_x as i32 + direction.x as i32;
    let ny = current_y as i32 + direction.y as i32;

    let (target_x, target_y) = if nx < 0 {
        (cols - 1, ny)
    } else if nx >= cols {
        (0, ny)
    } else if ny < 0 {
        (nx, rows - 1)
    } else if ny >= rows {
        (nx, 0)
    } else {
        (nx, ny)
    };

    if target_x >= 0 && target_y >= 0 {
        let tx = target_x as usize;
        let ty = target_y as usize;

        if is_tile_walkable(tx, ty, map) {
            return Some((tx, ty));
        }
    }
    None
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
