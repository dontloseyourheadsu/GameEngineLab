use engine_core::character::character_2d::Character2D;
use engine_core::maps::map_2d_model::Map2DModel;
use engine_core::physics::collisions_2d::simple_collision_body::SimpleCollisionBody;
use raylib::math::Vector2;

#[derive(PartialEq, Clone, Copy)]
pub enum GhostState {
    Chase,
    Scatter,
    Frightened,
}

pub struct Ghost {
    pub character: Character2D,
    pub target_position: Vector2,
    pub is_active: bool,
    pub grid_position: (usize, usize),
    pub state: GhostState,
    pub frightened_timer: f32,
    pub spawn_position: Vector2,
}

impl Ghost {
    pub fn update(
        &mut self,
        delta_time: f32,
        pacman_pos: Vector2,
        tile_size: f32,
        map: &mut Map2DModel,
    ) {
        if !self.is_active {
            return;
        }

        // Update state timers
        if self.state == GhostState::Frightened {
            self.frightened_timer -= delta_time;
            if self.frightened_timer <= 0.0 {
                self.state = GhostState::Chase;
            }
        }

        // Update target position based on state
        match self.state {
            GhostState::Chase => self.target_position = pacman_pos,
            GhostState::Frightened | GhostState::Scatter => {
                // In a real game, scatter targets are corners.
                // Frightened usually means random movement.
                // For now, keep as zero or implement random logic if I can.
                // But the previous code had zero, so I'll keep it simple for now,
                // but zero target with greedy movement creates weird behavior (always top-left).
                // Let's stick to previous behavior first to avoid breaking changes, then improve.
                self.target_position = Vector2::zero();
            }
        }

        // Move towards target
        self.move_towards_target(tile_size, map);
    }

    pub fn respawn(&mut self) {
        self.character.position = self.spawn_position;

        // We can extract from collision body if it's a Box and set to tile size.
        let tile_size = match self.character.collision_body {
            SimpleCollisionBody::Box { width, .. } => width,
            SimpleCollisionBody::Circle { radius } => radius * 2.0,
        };

        self.grid_position = (
            (self.spawn_position.x / tile_size) as usize,
            (self.spawn_position.y / tile_size) as usize,
        );
        self.state = GhostState::Chase;
        self.frightened_timer = 0.0;
        // Optionally set is_active = true if we manage that
    }

    fn move_towards_target(&mut self, tile_size: f32, map: &mut Map2DModel) {
        let (current_x, current_y) = self.grid_position;
        let target_x = (self.target_position.x / tile_size) as usize;
        let target_y = (self.target_position.y / tile_size) as usize;

        // Directions: Right, Left, Down, Up
        let directions = [(1, 0), (-1, 0), (0, 1), (0, -1)];

        let mut best_move = None;
        let mut min_dist = f32::MAX;

        // Frightened mode: Invert distance check or pick random?
        // Existing code: greedy approach.
        // If Frightened, target is 0,0. It will run to top-left.

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

                    if dist_sq < min_dist {
                        min_dist = dist_sq;
                        best_move = Some((nx, ny));
                    }
                }
            }
        }

        if let Some((next_x, next_y)) = best_move {
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

        // Can move on paths, food, pills, empty spaces
        // Cannot move on Walls '#'.
        // Removed 'P' and 'G' as we stop writing them.
        matches!(tile, '.' | 'o' | '*' | ' ')
    }
}
