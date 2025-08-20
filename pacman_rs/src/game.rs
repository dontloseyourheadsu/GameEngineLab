use crate::types::*;
use crate::player::Player;
use crate::ghost::Ghost;
use crate::game_map::GameMap;
use raylib::prelude::*;

pub struct Game {
    pub player: Player,
    pub ghosts: [Ghost; 4],
    pub map: GameMap,
    pub cnt_t_ghost_scared: i32,
    pub death_cnt_t: i32,
    pub cnt_t: i32,
    pub play_dead_sound: bool,
    pub player_points: i32,
    pub font: Option<Font>,
    pub game_over: bool,
    pub game_won: bool,
}

impl Game {
    pub fn new() -> Self {
        let player = Player::new();
        let ghosts = [
            Ghost::new(1, 13, GhostType::Red),
            Ghost::new(2, 13, GhostType::Pink),
            Ghost::new(3, 13, GhostType::Blue),
            Ghost::new(4, 13, GhostType::Orange),
        ];
        
        let map = GameMap::new();
        
        Game {
            player,
            ghosts,
            map,
            cnt_t_ghost_scared: 1,
            death_cnt_t: 1,
            cnt_t: 1,
            play_dead_sound: false,
            player_points: 0,
            font: None,
            game_over: false,
            game_won: false,
        }
    }
    
    pub fn load_resources(&mut self, rl: &mut RaylibHandle, thread: &RaylibThread) -> Result<(), String> {
        // Load font
        let font = rl.load_font_ex(thread, "Resources/PressStart2P-Regular.ttf", 24, None)
            .map_err(|e| format!("Failed to load font: {:?}", e))?;
        self.font = Some(font);
        
        // Load player textures
        self.player.load_textures(rl, thread)?;
        
        // Load ghost textures
        for ghost in &mut self.ghosts {
            ghost.load_textures(rl, thread)?;
        }
        
        // Initialize positions
        self.map.initialize_positions(&mut self.player, &mut self.ghosts);
        
        Ok(())
    }
    
    pub fn update(&mut self) {
        if self.game_over || self.game_won {
            return;
        }
        
        if self.player.is_dead {
            if !self.play_dead_sound {
                self.play_dead_sound = true;
                self.death_cnt_t = 1;
            }
            
            if self.death_cnt_t % self.player.respawn_speed == 0 {
                self.reset_for_next_life();
                self.play_dead_sound = false;
            }
            self.death_cnt_t += 1;
        } else {
            if self.cnt_t % self.player.speed == 0 {
                self.map.update_pacman(&mut self.player, &mut self.ghosts);
            }
            
            for ghost_type in [GhostType::Red, GhostType::Pink, GhostType::Blue, GhostType::Orange] {
                let ghost_index = ghost_type.to_index();
                if self.cnt_t % self.ghosts[ghost_index].speed == 0 {
                    self.map.update_ghost(ghost_type, &mut self.ghosts, &mut self.player);
                }
            }
        }
        
        // Handle scared ghosts
        if self.ghosts.iter().any(|g| g.is_scared) {
            if self.map.consumed_power_pill {
                self.cnt_t_ghost_scared = 1;
                self.map.consumed_power_pill = false;
            }
            
            if self.cnt_t_ghost_scared % self.ghosts[0].scared_time == 0 {
                for ghost in &mut self.ghosts {
                    ghost.is_scared = false;
                }
            }
            self.cnt_t_ghost_scared += 1;
        }
        
        // Handle ghost respawn
        for ghost in &mut self.ghosts {
            if ghost.respawn {
                if ghost.respawn_cnt_t % ghost.respawn_speed == 0 {
                    ghost.reset_for_next_life();
                    self.map.level[ghost.x as usize][ghost.y as usize] = ghost.ghost_type.to_char();
                    ghost.respawn_cnt_t = 1;
                }
                ghost.respawn_cnt_t += 1;
            }
        }
        
        // Update counters
        if self.cnt_t < i32::MAX - 10 {
            self.cnt_t += 1;
        } else {
            self.cnt_t = 0;
        }
        
        // Check game end conditions
        if self.player.lives == 0 {
            self.game_over = true;
        } else if self.player_points != self.player.points {
            if self.map.pacman_wins() {
                self.game_won = true;
            }
            self.player_points = self.player.points;
        }
    }
    
    fn reset_for_next_life(&mut self) {
        self.player.reset_for_next_life();
        self.map.level[self.player.x as usize][self.player.y as usize] = 'p';
        
        for ghost in &mut self.ghosts {
            self.map.level[ghost.x as usize][ghost.y as usize] = 'e';
            ghost.reset_for_next_life();
            self.map.level[ghost.x as usize][ghost.y as usize] = ghost.ghost_type.to_char();
        }
    }
    
    pub fn handle_input(&mut self, rl: &RaylibHandle) {
        if self.player.x == 0 || self.player.x == self.map.map_size - 1 || 
           self.player.y == 0 || self.player.y == self.map.map_size - 1 {
            return;
        }
        
        if rl.is_key_pressed(KeyboardKey::KEY_LEFT) {
            if self.cnt_t % self.player.speed == 0 {
                self.player.direction = Direction::Left;
            }
            self.player.next_direction = Direction::Left;
        } else if rl.is_key_pressed(KeyboardKey::KEY_RIGHT) {
            if self.cnt_t % self.player.speed == 0 {
                self.player.direction = Direction::Right;
            }
            self.player.next_direction = Direction::Right;
        } else if rl.is_key_pressed(KeyboardKey::KEY_UP) {
            if self.cnt_t % self.player.speed == 0 {
                self.player.direction = Direction::Up;
            }
            self.player.next_direction = Direction::Up;
        } else if rl.is_key_pressed(KeyboardKey::KEY_DOWN) {
            if self.cnt_t % self.player.speed == 0 {
                self.player.direction = Direction::Down;
            }
            self.player.next_direction = Direction::Down;
        }
    }
    
    pub fn draw(&mut self, d: &mut RaylibDrawHandle) {
        d.clear_background(Color::BLACK);
        
        // Draw the map and entities
        self.map.draw_map(self.cnt_t, d, &mut self.player, &mut self.ghosts);
        
        // Draw UI
        self.draw_ui(d);
    }
    
    fn draw_ui(&self, d: &mut RaylibDrawHandle) {
        let font_size = 12;
        let line_height = 20;
        
        if let Some(ref font) = self.font {
            // Score
            let score_text = format!("Score: {}", self.player.points);
            d.draw_text_ex(font, &score_text, Vector2::new(10.0, 10.0), font_size as f32, 1.0, Color::WHITE);
            
            // Lives
            let lives_text = format!("Lives: {}", self.player.lives);
            d.draw_text_ex(font, &lives_text, Vector2::new(10.0, 10.0 + line_height as f32), font_size as f32, 1.0, Color::WHITE);
            
            // Game over/won messages
            if self.game_over {
                let game_over_text = "Game Over!";
                d.draw_text_ex(font, game_over_text, Vector2::new(10.0, 10.0 + line_height as f32 * 2.0), font_size as f32, 1.0, Color::RED);
            } else if self.game_won {
                let you_win_text = "You Win!";
                d.draw_text_ex(font, you_win_text, Vector2::new(10.0, 10.0 + line_height as f32 * 2.0), font_size as f32, 1.0, Color::GREEN);
            }
        } else {
            // Fallback text drawing if font failed to load
            let score_text = format!("Score: {}", self.player.points);
            d.draw_text(&score_text, 10, 10, font_size, Color::WHITE);
            
            let lives_text = format!("Lives: {}", self.player.lives);
            d.draw_text(&lives_text, 10, 30, font_size, Color::WHITE);
            
            if self.game_over {
                d.draw_text("Game Over!", 10, 50, font_size, Color::RED);
            } else if self.game_won {
                d.draw_text("You Win!", 10, 50, font_size, Color::GREEN);
            }
        }
    }
}