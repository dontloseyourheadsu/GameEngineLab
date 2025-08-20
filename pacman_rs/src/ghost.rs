use crate::player::Player;
use crate::types::*;
use rand::Rng;
use raylib::prelude::*;
use std::f32::consts::PI;

pub struct Ghost {
    pub speed: i32,
    pub initial_x: i32,
    pub initial_y: i32,
    pub x: i32,
    pub y: i32,
    pub prev_x: f32,
    pub prev_y: f32,
    pub direction: Direction,
    pub animation: AnimationType,
    pub respawn: bool,
    pub respawn_speed: i32,
    pub respawn_cnt_t: i32,
    pub is_scared: bool,
    pub eaten_block: char,
    pub ghost_type: GhostType,
    pub scared_time: i32,

    // Animation continuity tracking
    pub animation_frame_counter: i32,

    // Animation textures by type
    pub move_texture: Option<Texture2D>,
    pub die_texture: Option<Texture2D>,
}

impl Ghost {
    pub fn new(x: i32, y: i32, ghost_type: GhostType) -> Self {
        Ghost {
            speed: 25,
            initial_x: x,
            initial_y: y,
            x,
            y,
            prev_x: x as f32,
            prev_y: y as f32,
            direction: Direction::None,
            animation: AnimationType::None,
            respawn: false,
            respawn_speed: 350,
            respawn_cnt_t: 1,
            is_scared: false,
            eaten_block: 'e',
            ghost_type,
            scared_time: 700,
            animation_frame_counter: 0,
            move_texture: None,
            die_texture: None,
        }
    }

    pub fn load_textures(
        &mut self,
        rl: &mut RaylibHandle,
        thread: &RaylibThread,
    ) -> Result<(), String> {
        let move_texture_path = match self.ghost_type {
            GhostType::Red => "Resources/red-slime-move.gif",
            GhostType::Pink => "Resources/pink-slime-move.gif",
            GhostType::Blue => "Resources/blue-slime-move.gif",
            GhostType::Orange => "Resources/orange-slime-move.gif",
        };

        let move_img = Image::load_image(move_texture_path)
            .map_err(|e| format!("Failed to load ghost move texture: {:?}", e))?;
        let die_img = Image::load_image("Resources/slime-die.gif")
            .map_err(|e| format!("Failed to load ghost die texture: {:?}", e))?;

        self.move_texture = Some(
            rl.load_texture_from_image(thread, &move_img)
                .map_err(|e| format!("Failed to create ghost move texture: {:?}", e))?,
        );
        self.die_texture = Some(
            rl.load_texture_from_image(thread, &die_img)
                .map_err(|e| format!("Failed to create ghost die texture: {:?}", e))?,
        );

        Ok(())
    }

    pub fn sees_pacman(
        &self,
        directions: &[Direction],
        level: &[[char; 15]; 15],
        pacman: &Player,
    ) -> Direction {
        for &dir in directions {
            let mut current_x = self.x;
            let mut current_y = self.y;

            loop {
                let (next_x, next_y) = match dir {
                    Direction::Up => (current_x, current_y - 1),
                    Direction::Down => (current_x, current_y + 1),
                    Direction::Left => (current_x - 1, current_y),
                    Direction::Right => (current_x + 1, current_y),
                    Direction::None => break,
                };

                if next_x < 0 || next_x >= 15 || next_y < 0 || next_y >= 15 {
                    break;
                }

                if level[next_x as usize][next_y as usize] == 'w' {
                    break;
                }

                if next_x == pacman.x && next_y == pacman.y {
                    return dir;
                }

                current_x = next_x;
                current_y = next_y;
            }
        }
        Direction::None
    }

    pub fn will_collide(&self, x: i32, y: i32, level: &[[char; 15]; 15]) -> bool {
        if x < 0 || x >= 15 || y < 0 || y >= 15 {
            return true;
        }

        matches!(
            level[x as usize][y as usize],
            'w' | '1' | '2' | '3' | '4' | '7' | '8' | '9' | '0'
        )
    }

    pub fn kill_pacman(&mut self, pacman: &mut Player, ghosts: &mut [Ghost]) {
        if self.is_scared {
            self.respawn = true;
            self.animation_frame_counter = 0; // Reset animation for respawn effect
            pacman.points += Player::GHOST_POINT;
            return;
        }

        pacman.lives -= 1;
        pacman.is_dead = true;

        for ghost in ghosts.iter_mut() {
            ghost.respawn = true;
            ghost.animation_frame_counter = 0; // Reset animation for respawn effect
        }
    }

    pub fn choose_next_direction(&mut self, level: &[[char; 15]; 15], pacman: &Player) {
        let directions = [
            Direction::Up,
            Direction::Down,
            Direction::Left,
            Direction::Right,
        ];

        let sees_pacman = self.sees_pacman(&directions, level, pacman);
        if sees_pacman != Direction::None && !self.is_scared {
            self.direction = sees_pacman;
            return;
        }

        let opposite_direction = self.direction.opposite();
        let mut possible_directions = Vec::new();

        for &dir in &directions {
            if dir != opposite_direction && self.can_move_in_direction(dir, level) {
                possible_directions.push(dir);
            }
        }

        if possible_directions.is_empty() {
            possible_directions.push(opposite_direction);
        }

        let mut rng = rand::thread_rng();
        if possible_directions.contains(&self.direction) && rng.gen_bool(0.8) {
            return;
        }

        let index = rng.gen_range(0..possible_directions.len());
        self.direction = possible_directions[index];
    }

    fn can_move_in_direction(&self, dir: Direction, level: &[[char; 15]; 15]) -> bool {
        let (check_x, check_y) = match dir {
            Direction::Up => (self.x, self.y - 1),
            Direction::Down => (self.x, self.y + 1),
            Direction::Left => (self.x - 1, self.y),
            Direction::Right => (self.x + 1, self.y),
            Direction::None => return false,
        };

        !self.will_collide(check_x, check_y, level)
    }

    pub fn animate(
        &mut self,
        cell_size: i32,
        _cnt_t: i32,
        d: &mut RaylibDrawHandle,
        pacman: &Player,
    ) {
        // Increment our continuous animation frame counter
        self.animation_frame_counter += 1;

        let animation_step = self.animation_frame_counter % self.speed;
        let step_size = 1.0 / self.speed as f32;
        let adjustment_factor = animation_step as f32 * step_size;

        if self.respawn {
            self.prev_x = self.x as f32;
            self.prev_y = self.y as f32;
        }

        let mut interpolated_x = if self.prev_x != self.x as f32 {
            self.prev_x
        } else {
            self.x as f32
        };
        let mut interpolated_y = if self.prev_y != self.y as f32 {
            self.prev_y
        } else {
            self.y as f32
        };

        if self.prev_x != self.x as f32 || self.prev_y != self.y as f32 {
            match self.direction {
                Direction::Left => interpolated_x -= adjustment_factor,
                Direction::Right => interpolated_x += adjustment_factor,
                Direction::Up => interpolated_y -= adjustment_factor,
                Direction::Down => interpolated_y += adjustment_factor,
                Direction::None => {}
            }
        }

        interpolated_x *= cell_size as f32;
        interpolated_y *= cell_size as f32;

        // Use the continuous animation counter for drawing effects too
        self.draw_ghost(
            d,
            cell_size,
            interpolated_x,
            interpolated_y,
            self.animation_frame_counter as f32,
            pacman,
        );
    }

    fn draw_ghost(
        &self,
        d: &mut RaylibDrawHandle,
        cell_size: i32,
        interpolated_x: f32,
        interpolated_y: f32,
        cnt_t: f32,
        pacman: &Player,
    ) {
        let (outer_color, inner_color) = self.get_ghost_colors();
        let eye_color = Color::WHITE;
        let scared_color = Color::new(0, 0, 139, 255); // Dark blue

        let final_outer_color = if self.is_scared {
            scared_color
        } else {
            outer_color
        };
        let final_inner_color = if self.is_scared {
            Color::BLUE
        } else {
            inner_color
        };

        if self.respawn {
            // Draw respawn animation (pie-like effect)
            let cycle_position = (cnt_t % self.respawn_speed as f32) / self.respawn_speed as f32;
            let sweep_angle = 360.0 * (1.0 - cycle_position);

            let start_angle = 270.0 - (sweep_angle / 2.0);

            d.draw_circle_sector(
                Vector2::new(
                    interpolated_x + cell_size as f32 / 2.0,
                    interpolated_y + cell_size as f32 / 2.0,
                ),
                cell_size as f32 / 2.0,
                start_angle,
                start_angle + sweep_angle,
                16,
                final_inner_color,
            );
        } else {
            // Draw normal ghost with pulsing effect
            let min_scale = 0.8;
            let max_scale = 1.2;
            let scale =
                min_scale + ((cnt_t * 2.0 * PI).cos() * 0.5 + 0.5) * (max_scale - min_scale);

            let scaled_cell_size = cell_size as f32 * scale;
            let x_position = interpolated_x - (scaled_cell_size - cell_size as f32) / 2.0;
            let y_position = interpolated_y - (scaled_cell_size - cell_size as f32) / 2.0;

            // Draw ghost body
            if let Some(ref texture) = if self.respawn {
                &self.die_texture
            } else {
                &self.move_texture
            } {
                let dest_rect =
                    Rectangle::new(x_position, y_position, scaled_cell_size, scaled_cell_size);

                d.draw_texture_pro(
                    texture,
                    Rectangle::new(0.0, 0.0, texture.width as f32, texture.height as f32),
                    dest_rect,
                    Vector2::zero(),
                    0.0,
                    final_outer_color,
                );
            } else {
                // Fallback drawing
                d.draw_circle(
                    (x_position + scaled_cell_size / 2.0) as i32,
                    (y_position + scaled_cell_size / 2.0) as i32,
                    scaled_cell_size / 2.0,
                    final_outer_color,
                );
            }

            // Draw eyes that look toward pacman
            let eye_offset = cell_size as f32 * 0.25;
            let mut eye_position = Vector2::new(
                interpolated_x + (cell_size as f32 * 0.5) - (cell_size as f32 * 0.05),
                interpolated_y + (cell_size as f32 * 0.3),
            );
            let eye_size = cell_size as f32 * 0.2;

            // Calculate direction to pacman for eye movement
            let dx = pacman.x as f32 - self.x as f32;
            let dy = pacman.y as f32 - self.y as f32;
            let distance = (dx * dx + dy * dy).sqrt();

            if distance > 0.1 {
                let normalized_dx = dx / distance;
                let normalized_dy = dy / distance;
                eye_position.x += normalized_dx * eye_offset * 0.5;
                eye_position.y += normalized_dy * eye_offset * 0.5;
            }

            // Draw eyes
            d.draw_circle(
                eye_position.x as i32,
                eye_position.y as i32,
                eye_size / 2.0,
                eye_color,
            );
            d.draw_circle(
                (eye_position.x + eye_size * 1.5) as i32,
                eye_position.y as i32,
                eye_size / 2.0,
                eye_color,
            );
        }
    }

    fn get_ghost_colors(&self) -> (Color, Color) {
        match self.ghost_type {
            GhostType::Red => (Color::new(229, 115, 115, 255), Color::new(255, 0, 0, 255)),
            GhostType::Pink => (
                Color::new(255, 200, 200, 255),
                Color::new(255, 192, 203, 255),
            ),
            GhostType::Blue => (Color::new(173, 216, 230, 255), Color::new(0, 0, 255, 255)),
            GhostType::Orange => (Color::new(255, 183, 77, 255), Color::new(255, 165, 0, 255)),
        }
    }

    pub fn reset_for_next_life(&mut self) {
        self.x = self.initial_x;
        self.y = self.initial_y;
        self.prev_x = self.x as f32;
        self.prev_y = self.y as f32;
        self.direction = Direction::None;
        self.animation = AnimationType::None;
        self.respawn = false;
        self.is_scared = false;
        self.animation_frame_counter = 0; // Reset animation continuity on respawn
    }
}
