use crate::types::*;
use crate::player::Player;
use crate::ghost::Ghost;
use crate::rendering::{Pill, Brick};
use raylib::prelude::*;

pub struct GameMap {
    pub level: [[char; 15]; 15],
    pub cell_size: i32,
    pub map_size: i32,
    pub consumed_power_pill: bool,
}

impl GameMap {
    pub fn new() -> Self {
        let level = [
            ['w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            ['w', 'e', '#', '#', '#', '"', '#', '#', 'w', '"', '#', 'e', 'e', '1', 'w'],
            ['w', '#', '#', '#', '#', '#', 'w', '#', 'w', '#', '#', 'w', 'e', '2', 'w'],
            ['w', '#', 'w', 'w', 'w', '#', 'w', '#', '#', '#', '#', 'w', 'e', '3', 'w'],
            ['w', '#', '#', 'w', '#', '#', 'w', '#', 'w', '#', '#', 'w', 'e', '4', 'w'],
            ['w', '#', '#', '#', '#', '#', '#', '#', 'w', '#', '"', 'w', 'e', 'e', 'w'],
            ['w', 'w', 'w', 'w', '#', '#', '#', '#', 'w', 'w', '#', 'w', 'w', 'w', 'w'],
            ['#', '#', '#', '#', '"', 'w', '#', '#', '#', '#', '#', 'w', '#', '#', '#'],
            ['w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', '#', '#', 'w', '#', 'w'],
            ['w', 'w', '#', 'w', 'w', '#', '#', '#', 'w', '#', '#', 'w', '#', '#', 'w'],
            ['w', 'w', '#', 'w', '#', '#', 'w', '#', '#', '#', '#', '#', '#', '#', 'w'],
            ['w', '#', '#', '#', '#', '#', 'w', '#', 'w', 'w', '#', 'w', '#', '#', 'w'],
            ['w', '#', 'w', 'w', 'w', '#', 'w', '#', 'w', 'w', '#', 'w', '#', '#', 'w'],
            ['w', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', '#', 'w'],
            ['w', 'w', 'w', 'w', 'w', 'w', 'w', '#', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
        ];
        
        GameMap {
            level,
            cell_size: 75,
            map_size: 15,
            consumed_power_pill: false,
        }
    }
    
    pub fn initialize_positions(&mut self, player: &mut Player, ghosts: &mut [Ghost; 4]) {
        // Set player position
        self.level[player.x as usize][player.y as usize] = 'p';
        
        // Set ghost positions
        for ghost in ghosts.iter() {
            self.level[ghost.x as usize][ghost.y as usize] = ghost.ghost_type.to_char();
        }
    }
    
    pub fn pacman_wins(&self) -> bool {
        for y in 0..self.map_size {
            for x in 0..self.map_size {
                if self.level[x as usize][y as usize] == '#' || self.level[x as usize][y as usize] == '"' {
                    return false;
                }
            }
        }
        true
    }
    
    fn update_pacman_points(&mut self, player: &mut Player, x: i32, y: i32, ghosts: &mut [Ghost; 4]) {
        match self.level[x as usize][y as usize] {
            '#' => {
                player.points += Player::PILL_POINT;
            }
            '"' => {
                player.points += Player::POWER_PILL_POINT;
                self.consumed_power_pill = true;
                for ghost in ghosts.iter_mut() {
                    ghost.is_scared = true;
                }
            }
            '1' | '2' | '3' | '4' => {
                if let Some(ghost_type) = GhostType::from_char(self.level[x as usize][y as usize]) {
                    let ghost_index = ghost_type.to_index();
                    // Handle killing pacman differently to avoid borrowing issues
                    if ghosts[ghost_index].is_scared {
                        ghosts[ghost_index].respawn = true;
                        player.points += Player::GHOST_POINT;
                    } else {
                        player.lives -= 1;
                        player.is_dead = true;
                        // Set all ghosts to respawn
                        for ghost in ghosts.iter_mut() {
                            ghost.respawn = true;
                        }
                    }
                }
            }
            _ => {}
        }
    }
    
    pub fn update_pacman(&mut self, player: &mut Player, ghosts: &mut [Ghost; 4]) {
        if player.y != 0 && player.y != self.map_size - 1 && player.x != 0 && player.x != self.map_size - 1 {
            if (self.level[player.x as usize][(player.y - 1) as usize] != 'w' && player.next_direction == Direction::Up) ||
               (self.level[player.x as usize][(player.y + 1) as usize] != 'w' && player.next_direction == Direction::Down) ||
               (self.level[(player.x - 1) as usize][player.y as usize] != 'w' && player.next_direction == Direction::Left) ||
               (self.level[(player.x + 1) as usize][player.y as usize] != 'w' && player.next_direction == Direction::Right) {
                player.direction = player.next_direction;
            }
        }
        
        player.prev_x = player.x as f32;
        player.prev_y = player.y as f32;
        
        match player.direction {
            Direction::Up => {
                if player.y == 0 {
                    self.level[player.x as usize][player.y as usize] = 'e';
                    self.update_pacman_points(player, player.x, self.map_size - 1, ghosts);
                    player.y = self.map_size - 1;
                    self.level[player.x as usize][player.y as usize] = 'p';
                } else if self.level[player.x as usize][(player.y - 1) as usize] != 'w' {
                    self.update_pacman_points(player, player.x, player.y - 1, ghosts);
                    self.level[player.x as usize][player.y as usize] = 'e';
                    player.y -= 1;
                    self.level[player.x as usize][player.y as usize] = 'p';
                }
            }
            Direction::Down => {
                if player.y == self.map_size - 1 {
                    self.level[player.x as usize][player.y as usize] = 'e';
                    self.update_pacman_points(player, player.x, 0, ghosts);
                    player.y = 0;
                    self.level[player.x as usize][player.y as usize] = 'p';
                } else if self.level[player.x as usize][(player.y + 1) as usize] != 'w' {
                    self.update_pacman_points(player, player.x, player.y + 1, ghosts);
                    self.level[player.x as usize][player.y as usize] = 'e';
                    player.y += 1;
                    self.level[player.x as usize][player.y as usize] = 'p';
                }
            }
            Direction::Left => {
                if player.x == 0 {
                    self.level[player.x as usize][player.y as usize] = 'e';
                    self.update_pacman_points(player, self.map_size - 1, player.y, ghosts);
                    player.x = self.map_size - 1;
                    self.level[player.x as usize][player.y as usize] = 'p';
                } else if self.level[(player.x - 1) as usize][player.y as usize] != 'w' {
                    self.update_pacman_points(player, player.x - 1, player.y, ghosts);
                    self.level[player.x as usize][player.y as usize] = 'e';
                    player.x -= 1;
                    self.level[player.x as usize][player.y as usize] = 'p';
                }
            }
            Direction::Right => {
                if player.x == self.map_size - 1 {
                    self.level[player.x as usize][player.y as usize] = 'e';
                    self.update_pacman_points(player, 0, player.y, ghosts);
                    player.x = 0;
                    self.level[player.x as usize][player.y as usize] = 'p';
                } else if self.level[(player.x + 1) as usize][player.y as usize] != 'w' {
                    self.update_pacman_points(player, player.x + 1, player.y, ghosts);
                    self.level[player.x as usize][player.y as usize] = 'e';
                    player.x += 1;
                    self.level[player.x as usize][player.y as usize] = 'p';
                }
            }
            Direction::None => {}
        }
    }
    
    pub fn update_ghost(&mut self, ghost_type: GhostType, ghosts: &mut [Ghost; 4], player: &mut Player) {
        let i = ghost_type.to_index();
        
        if ghosts[i].respawn {
            return;
        }
        
        ghosts[i].choose_next_direction(&self.level, player);
        
        ghosts[i].prev_y = ghosts[i].y as f32;
        ghosts[i].prev_x = ghosts[i].x as f32;
        
        match ghosts[i].direction {
            Direction::Up => {
                if ghosts[i].y == 0 {
                    return;
                } else if self.level[ghosts[i].x as usize][(ghosts[i].y - 1) as usize] == 'p' {
                    // Handle killing pacman inline to avoid borrowing issues
                    if ghosts[i].is_scared {
                        ghosts[i].respawn = true;
                        player.points += Player::GHOST_POINT;
                    } else {
                        player.lives -= 1;
                        player.is_dead = true;
                        for ghost in ghosts.iter_mut() {
                            ghost.respawn = true;
                        }
                    }
                } else if !ghosts[i].will_collide(ghosts[i].x, ghosts[i].y - 1, &self.level) {
                    self.level[ghosts[i].x as usize][ghosts[i].y as usize] = ghosts[i].eaten_block;
                    ghosts[i].y -= 1;
                    ghosts[i].eaten_block = self.level[ghosts[i].x as usize][ghosts[i].y as usize];
                    self.level[ghosts[i].x as usize][ghosts[i].y as usize] = ghost_type.to_char();
                }
            }
            Direction::Down => {
                if ghosts[i].y == self.map_size - 1 {
                    return;
                } else if self.level[ghosts[i].x as usize][(ghosts[i].y + 1) as usize] == 'p' {
                    // Handle killing pacman inline to avoid borrowing issues
                    if ghosts[i].is_scared {
                        ghosts[i].respawn = true;
                        player.points += Player::GHOST_POINT;
                    } else {
                        player.lives -= 1;
                        player.is_dead = true;
                        for ghost in ghosts.iter_mut() {
                            ghost.respawn = true;
                        }
                    }
                } else if !ghosts[i].will_collide(ghosts[i].x, ghosts[i].y + 1, &self.level) {
                    self.level[ghosts[i].x as usize][ghosts[i].y as usize] = ghosts[i].eaten_block;
                    ghosts[i].y += 1;
                    ghosts[i].eaten_block = self.level[ghosts[i].x as usize][ghosts[i].y as usize];
                    self.level[ghosts[i].x as usize][ghosts[i].y as usize] = ghost_type.to_char();
                }
            }
            Direction::Left => {
                if ghosts[i].x == 0 {
                    return;
                } else if self.level[(ghosts[i].x - 1) as usize][ghosts[i].y as usize] == 'p' {
                    // Handle killing pacman inline to avoid borrowing issues
                    if ghosts[i].is_scared {
                        ghosts[i].respawn = true;
                        player.points += Player::GHOST_POINT;
                    } else {
                        player.lives -= 1;
                        player.is_dead = true;
                        for ghost in ghosts.iter_mut() {
                            ghost.respawn = true;
                        }
                    }
                } else if !ghosts[i].will_collide(ghosts[i].x - 1, ghosts[i].y, &self.level) {
                    self.level[ghosts[i].x as usize][ghosts[i].y as usize] = ghosts[i].eaten_block;
                    ghosts[i].x -= 1;
                    ghosts[i].eaten_block = self.level[ghosts[i].x as usize][ghosts[i].y as usize];
                    self.level[ghosts[i].x as usize][ghosts[i].y as usize] = ghost_type.to_char();
                }
            }
            Direction::Right => {
                if ghosts[i].x == self.map_size - 1 {
                    return;
                } else if self.level[(ghosts[i].x + 1) as usize][ghosts[i].y as usize] == 'p' {
                    // Handle killing pacman inline to avoid borrowing issues
                    if ghosts[i].is_scared {
                        ghosts[i].respawn = true;
                        player.points += Player::GHOST_POINT;
                    } else {
                        player.lives -= 1;
                        player.is_dead = true;
                        for ghost in ghosts.iter_mut() {
                            ghost.respawn = true;
                        }
                    }
                } else if !ghosts[i].will_collide(ghosts[i].x + 1, ghosts[i].y, &self.level) {
                    self.level[ghosts[i].x as usize][ghosts[i].y as usize] = ghosts[i].eaten_block;
                    ghosts[i].x += 1;
                    ghosts[i].eaten_block = self.level[ghosts[i].x as usize][ghosts[i].y as usize];
                    self.level[ghosts[i].x as usize][ghosts[i].y as usize] = ghost_type.to_char();
                }
            }
            Direction::None => {}
        }
    }
    
    pub fn draw_map(&self, cnt_t: i32, d: &mut RaylibDrawHandle, player: &mut Player, ghosts: &mut [Ghost; 4]) {
        for y in 0..self.map_size {
            for x in 0..self.map_size {
                match self.level[x as usize][y as usize] {
                    '#' => Pill::draw_pill(d, x, y, self.cell_size, cnt_t),
                    'w' => Brick::draw_brick(d, x, y, self.cell_size),
                    'p' => player.animate(self.cell_size, cnt_t, d),
                    '"' => Pill::draw_power_pill(d, x, y, self.cell_size, cnt_t),
                    '1' => ghosts[0].animate(self.cell_size, cnt_t, d, player),
                    '2' => ghosts[1].animate(self.cell_size, cnt_t, d, player),
                    '3' => ghosts[2].animate(self.cell_size, cnt_t, d, player),
                    '4' => ghosts[3].animate(self.cell_size, cnt_t, d, player),
                    _ => {
                        // Draw transparent background for empty cells
                    }
                }
            }
        }
    }
}
