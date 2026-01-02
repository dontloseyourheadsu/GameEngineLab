use engine_core::character::character_2d::Character2D;
use engine_core::maps::map_2d_model::Map2DModel;
use raylib::math::Vector2;

pub struct Ghost {
    pub character: Character2D,
    pub target_position: Vector2,
    pub is_active: bool,
    pub grid_position: (usize, usize),
    pub stored_tile: char,
}

impl Ghost {
    pub fn update(&mut self, pacman_pos: Vector2, tile_size: f32, map: &mut Map2DModel) {
        if !self.is_active {
            return;
        }

        // Update target position
        self.target_position = pacman_pos;

        // Move towards target
        self.move_towards_target(tile_size, map);
    }

    fn move_towards_target(&mut self, tile_size: f32, map: &mut Map2DModel) {
        let (current_x, current_y) = self.grid_position;
        let target_x = (self.target_position.x / tile_size) as usize;
        let target_y = (self.target_position.y / tile_size) as usize;

        // Directions: Right, Left, Down, Up
        let directions = [(1, 0), (-1, 0), (0, 1), (0, -1)];

        let mut best_move = None;
        let mut min_dist = f32::MAX;

        // Simple greedy approach: pick the valid neighbor closest to target
        for (dir_x, dir_y) in directions.iter() {
            let next_x = current_x as i32 + dir_x;
            let next_y = current_y as i32 + dir_y;

            if next_x >= 0 && next_y >= 0 {
                let nx = next_x as usize;
                let ny = next_y as usize;

                if self.can_move_to(nx, ny, map) {
                    // Calculate distance squared to target
                    let dist_sq = ((nx as i32 - target_x as i32).pow(2)
                        + (ny as i32 - target_y as i32).pow(2))
                        as f32;

                    // Add a small penalty for reversing direction? (Not implemented yet)

                    if dist_sq < min_dist {
                        min_dist = dist_sq;
                        best_move = Some((nx, ny));
                    }
                }
            }
        }

        if let Some((next_x, next_y)) = best_move {
            // Restore old tile
            if let Some(row) = map.data.get_mut(current_y) {
                row.replace_range(current_x..current_x + 1, &self.stored_tile.to_string());
            }

            // Store new tile
            if let Some(row) = map.data.get(next_y) {
                self.stored_tile = row.chars().nth(next_x).unwrap_or(' ');
            }

            // Place Ghost on map
            if let Some(row) = map.data.get_mut(next_y) {
                row.replace_range(next_x..next_x + 1, "G");
            }

            // Update position
            self.grid_position = (next_x, next_y);
            self.character.position.x = (next_x as f32) * tile_size;
            self.character.position.y = (next_y as f32) * tile_size;
        }
    }

    fn can_move_to(&self, x: usize, y: usize, map: &Map2DModel) -> bool {
        if y >= map.data.len() {
            return false;
        }
        let row = &map.data[y];
        if x >= row.len() {
            return false;
        }

        let tile = row.chars().nth(x).unwrap_or('#');

        // Can move on paths, food, pills, empty spaces, Pacman
        // Cannot move on Walls '#', other Ghosts 'G'
        matches!(tile, '.' | 'o' | '*' | ' ' | 'P')
    }
}
