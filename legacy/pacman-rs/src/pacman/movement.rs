use crate::pacman::pacman::Pacman;
use engine_core::maps::map_2d_model::Map2DModel;
use raylib::prelude::*;

pub enum PacmanEvent {
    None,
    EatenFood,
    EatenPill,
}

pub fn move_pacman(pacman: &mut Pacman, map: &mut Map2DModel) -> PacmanEvent {
    let (current_x, current_y) = pacman.grid_position;

    // If already moving, specific logic handles animation updates elsewhere or we return early.
    // However, for this architecture, we split logic (try_move) and visual (update).
    // This function acts as 'try_move'.
    if pacman.is_moving {
        return PacmanEvent::None;
    }

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

        // Move Pacman Logic (Start Move)
        // Check if teleporting (wraparound means distance > 1)
        let dist_x = (next_x as i32 - current_x as i32).abs();
        let dist_y = (next_y as i32 - current_y as i32).abs();
        pacman.is_teleporting = dist_x > 1 || dist_y > 1;

        pacman.previous_grid_position = pacman.grid_position;
        pacman.grid_position = (next_x, next_y);
        // Do NOT set character.position here, it's handled by update_visuals
        pacman.is_moving = true;
        pacman.move_progress = 0.0;
    }
    event
}

pub fn update_pacman_visuals(
    pacman: &mut Pacman,
    delta_time: f32,
    move_interval_secs: f32,
    tile_size: f32,
) {
    if !pacman.is_moving {
        // Ensure position is snapped
        pacman.character.position.x = (pacman.grid_position.0 as f32) * tile_size;
        pacman.character.position.y = (pacman.grid_position.1 as f32) * tile_size;
        return;
    }

    pacman.move_progress += delta_time / move_interval_secs;

    if pacman.move_progress >= 1.0 {
        pacman.move_progress = 0.0;
        pacman.is_moving = false;
        pacman.is_teleporting = false;
        // Snap to final
        pacman.character.position.x = (pacman.grid_position.0 as f32) * tile_size;
        pacman.character.position.y = (pacman.grid_position.1 as f32) * tile_size;
    } else {
        if !pacman.is_teleporting {
            // Normal movement: Interpolate (stepped)
            let start_x = (pacman.previous_grid_position.0 as f32) * tile_size;
            let start_y = (pacman.previous_grid_position.1 as f32) * tile_size;
            let end_x = (pacman.grid_position.0 as f32) * tile_size;
            let end_y = (pacman.grid_position.1 as f32) * tile_size;

            let t = pacman.move_progress;
            let steps = 4.0;
            let stepped_t = (t * steps).floor() / steps;

            pacman.character.position.x = start_x + (end_x - start_x) * stepped_t;
            pacman.character.position.y = start_y + (end_y - start_y) * stepped_t;
        } else {
            // Teleporting logic
            let t = pacman.move_progress;
            let steps = 4.0;
            let stepped_t = (t * steps).floor() / steps; // 0.0, 0.25, 0.5, 0.75

            if stepped_t >= 0.75 {
                // Last frame: Show at target
                let end_x = (pacman.grid_position.0 as f32) * tile_size;
                let end_y = (pacman.grid_position.1 as f32) * tile_size;
                pacman.character.position.x = end_x;
                pacman.character.position.y = end_y;
            } else {
                // First frames: Show at start
                let start_x = (pacman.previous_grid_position.0 as f32) * tile_size;
                let start_y = (pacman.previous_grid_position.1 as f32) * tile_size;
                pacman.character.position.x = start_x;
                pacman.character.position.y = start_y;
            }
        }
    }
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
