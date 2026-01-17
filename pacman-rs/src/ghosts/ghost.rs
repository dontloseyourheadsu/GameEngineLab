use engine_core::character::character_2d::Character2D;
use engine_core::maps::map_2d_model::Map2DModel;
use engine_core::physics::collisions_2d::simple_collision_body::SimpleCollisionBody;
use raylib::math::Vector2;

#[derive(PartialEq, Clone, Copy, Debug)]
pub enum GhostState {
    Chase,
    Scatter,
    Frightened,
}

#[derive(PartialEq, Eq, Hash, Clone, Copy, Debug)]
pub enum GhostBehavior {
    Blinky, // Chases directly
    Pinky,  // Ambushes (ahead of pacman)
    Inky,   // Flanks (uses pivot - simulated)
    Clyde,  // Feigns ignorance (chases if far, scatters if close)
}

pub struct Ghost {
    pub character: Character2D,
    pub target_position: Vector2,
    pub is_active: bool,
    pub grid_position: (usize, usize),
    pub state: GhostState,
    pub frightened_timer: f32,
    pub spawn_position: Vector2,
    pub behavior: GhostBehavior,
    pub current_direction: (i32, i32),
    pub is_moving: bool,
    pub move_progress: f32,
    pub previous_grid_position: (usize, usize),
    pub next_grid_position: (usize, usize),
}

impl Ghost {
    pub fn update(
        &mut self,
        delta_time: f32,
        pacman_pos: Vector2,
        pacman_dir: Vector2,
        tile_size: f32,
        map: &mut Map2DModel,
        move_interval_secs: f32,
    ) {
        if !self.is_active {
            return;
        }

        // Ensure animation updates
        self.character.sprite.update();

        // Update state timers
        if self.state == GhostState::Frightened {
            self.frightened_timer -= delta_time;
            if self.frightened_timer <= 0.0 {
                self.state = GhostState::Chase;
            }
        }

        // Update target position based on state and behavior
        match self.state {
            GhostState::Chase => {
                match self.behavior {
                    GhostBehavior::Blinky => {
                        self.target_position = pacman_pos;
                    }
                    GhostBehavior::Pinky => {
                        // 4 tiles ahead
                        let offset = pacman_dir * (tile_size * 4.0);
                        self.target_position = pacman_pos + offset;
                    }
                    GhostBehavior::Inky => {
                        // Inky Targets "Behind" relative to Pacman's facing to contrast Pinky
                        // vector = (PacmanPos - (PacmanDir * 2))
                        // Note: Real Inky uses Blinky position, but this is a good approximation for distinct behavior without querying Blinky
                        let offset = pacman_dir * (tile_size * 2.0);
                        self.target_position = pacman_pos - offset;
                    }
                    GhostBehavior::Clyde => {
                        // If dist > 8 tiles, chase. Else scatter (to bottom left 0, map_height)
                        let dist_sq = (self.character.position - pacman_pos).length_sqr();
                        let eight_tiles = 8.0 * tile_size;
                        if dist_sq > eight_tiles * eight_tiles {
                            self.target_position = pacman_pos;
                        } else {
                            // Scatter target: Bottom Left (0, Height)
                            self.target_position =
                                Vector2::new(0.0, (map.data.len() as f32) * tile_size);
                        }
                    }
                }
            }
            GhostState::Frightened => {
                // Random target logic handled in move_towards_target for frightened usually,
                // or just set a random target here.
                // For compatibility with existing logic which might rely on behavior inside move_towards_target:
                // Existing logic: if Frightened, target is Zero (0,0).
                // We should improve this to be random corner or random valid tile.
                // For now, let's keep it simple or pick a random valid tile?
                // Let's stick to 0,0 for Frightened to ensure they run away from Pacman generally if we invert logic?
                // No, move_towards_target uses greedy approach to target.
                // If target is 0,0 they all run to top-left.
                // Let's leave it as is for Frightened for now to minimize scope creep,
                // or maybe implement simple random walk in move_towards
                self.target_position = Vector2::zero();
            }
            GhostState::Scatter => {
                // Determine scatter corner based on behavior
                let rows = map.data.len() as f32;
                let cols = map.data[0].len() as f32;

                self.target_position = match self.behavior {
                    GhostBehavior::Blinky => Vector2::new(cols * tile_size, 0.0), // Top Right
                    GhostBehavior::Pinky => Vector2::new(0.0, 0.0),               // Top Left
                    GhostBehavior::Inky => Vector2::new(cols * tile_size, rows * tile_size), // Bottom Right
                    GhostBehavior::Clyde => Vector2::new(0.0, rows * tile_size), // Bottom Left
                };
            }
        }

        // Move towards target
        self.update_visuals(delta_time, move_interval_secs, tile_size);
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
        self.previous_grid_position = self.grid_position;
        self.next_grid_position = self.grid_position;
        self.is_moving = false;
        self.move_progress = 0.0;

        self.state = GhostState::Chase;
        self.frightened_timer = 0.0;
        self.current_direction = (0, 0);
        // is_active remains true
    }

    fn update_visuals(&mut self, delta_time: f32, move_interval_secs: f32, tile_size: f32) {
        if !self.is_moving {
            self.character.position.x = (self.grid_position.0 as f32) * tile_size;
            self.character.position.y = (self.grid_position.1 as f32) * tile_size;
            return;
        }

        self.move_progress += delta_time / move_interval_secs;

        // Physical update: for 1/3 (3/4 actually, but let's say > 0.5) and 4/4 then it will be at target cell
        if self.move_progress >= 0.5 && self.grid_position != self.next_grid_position {
            self.grid_position = self.next_grid_position;
        }

        if self.move_progress >= 1.0 {
            self.move_progress = 0.0;
            self.is_moving = false;
            // Snap to next
            self.grid_position = self.next_grid_position;
            self.character.position.x = (self.grid_position.0 as f32) * tile_size;
            self.character.position.y = (self.grid_position.1 as f32) * tile_size;
        } else {
            // Interpolate
            let start_x = (self.previous_grid_position.0 as f32) * tile_size;
            let start_y = (self.previous_grid_position.1 as f32) * tile_size;
            let end_x = (self.next_grid_position.0 as f32) * tile_size;
            let end_y = (self.next_grid_position.1 as f32) * tile_size;

            let t = self.move_progress;
            self.character.position.x = start_x + (end_x - start_x) * t;
            self.character.position.y = start_y + (end_y - start_y) * t;
        }
    }

    fn move_towards_target(&mut self, tile_size: f32, map: &mut Map2DModel) {
        if self.is_moving {
            return;
        }
        let (current_x, current_y) = self.grid_position;
        let target_x = (self.target_position.x / tile_size) as usize;
        let target_y = (self.target_position.y / tile_size) as usize;

        // Directions: Right, Left, Down, Up
        let directions = [(1, 0), (-1, 0), (0, 1), (0, -1)];

        let mut best_move = None;
        let mut min_dist = f32::MAX;

        let reverse_dir = (-self.current_direction.0, -self.current_direction.1);

        // Collect valid moves
        let mut valid_moves = Vec::new();
        for (dir_x, dir_y) in directions.iter() {
            let next_x = current_x as i32 + dir_x;
            let next_y = current_y as i32 + dir_y;

            if next_x >= 0 && next_y >= 0 {
                let nx = next_x as usize;
                let ny = next_y as usize;

                if self.can_move_to(nx, ny, map) {
                    valid_moves.push(((nx, ny), (*dir_x, *dir_y)));
                }
            }
        }

        // Filter reverse direction unless it's a dead end (only 1 move available)
        let candidates: Vec<_> = if valid_moves.len() > 1 && self.current_direction != (0, 0) {
            valid_moves
                .into_iter()
                .filter(|(_, dir)| *dir != reverse_dir)
                .collect()
        } else {
            valid_moves
        };

        // If Frightened, pick random
        if self.state == GhostState::Frightened {
            use rand::seq::SliceRandom;
            let mut rng = rand::thread_rng();
            if let Some(((next_x, next_y), dir)) = candidates.choose(&mut rng) {
                best_move = Some((*next_x, *next_y));
                self.current_direction = *dir;
            }
        } else {
            // Pick closest to target
            for ((nx, ny), dir) in candidates {
                // Calculate distance squared to target
                let dist_sq = ((nx as i32 - target_x as i32).pow(2)
                    + (ny as i32 - target_y as i32).pow(2)) as f32;

                // Priority: Up (-1 y) > Left (-1 x) > Down (+1 y) > Right (+1 x) ?
                // Standard Pacman uses priority logic for ties.
                // Simple strict less than handles ties by order of visiting.
                // Our directions array is Right, Left, Down, Up.
                // If we want standard priority (Up > Left > Down > Right), iterate in that order?
                // For now, simple min_dist is fine, but helps to be consistent.

                if dist_sq < min_dist {
                    min_dist = dist_sq;
                    best_move = Some((nx, ny));
                    self.current_direction = dir;
                }
            }
        }

        if let Some((next_x, next_y)) = best_move {
            // Initiate move
            self.previous_grid_position = self.grid_position;
            self.next_grid_position = (next_x, next_y);
            self.is_moving = true;
            self.move_progress = 0.0;
            // grid_position remains current until t >= 0.5
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

        // Can move on paths, food, pills, empty spaces, and spawners
        // Cannot move on Walls '#'.
        matches!(tile, '.' | 'o' | '*' | ' ' | 'S')
    }
}
