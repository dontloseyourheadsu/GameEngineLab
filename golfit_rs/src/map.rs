use raylib::prelude::*;
use crate::vector::Vector2D;

#[derive(Debug, Clone)]
pub struct Map {
    pub map: Vec<Vec<char>>,
    pub levels: Vec<Vec<Vec<char>>>,
    pub ball_level_positions: Vec<(usize, usize)>,
    pub goal_level_positions: Vec<(usize, usize)>,
    pub obstacle_positions: Vec<Vec<(usize, usize)>>,
    pub obstacles: Vec<Vec<char>>,
    pub cell_size: i32,
    level: usize,
}

impl Map {
    const LIGHT_TILE: char = 't';
    const DARK_TILE: char = 'T';
    const WALL: char = 'w';
    const SAND: char = 's';
    const TRIANGLE: char = 'r';

    pub fn new(cell_size: i32, level: usize) -> Self {
        let mut map = Self {
            map: vec![],
            levels: vec![],
            ball_level_positions: vec![],
            goal_level_positions: vec![],
            obstacle_positions: vec![],
            obstacles: vec![],
            cell_size,
            level,
        };
        map.create_maps();
        map.set_map(level);
        map
    }

    pub fn set_map(&mut self, index: usize) {
        if index < self.levels.len() {
            self.map = self.levels[index].clone();
        }
    }

    pub fn render(&self, d: &mut RaylibDrawHandle, _cnt_t: u32) {
        // Define colors matching C# version
        let dark_green = Color::new(126, 160, 14, 255);  // #7EA00E
        let light_green = Color::new(220, 217, 100, 255); // #DCD964
        let wall_color = Color::new(33, 53, 2, 255);      // #213502
        let sand_color = Color::new(232, 216, 166, 255);  // #E8D8A6

        for i in 0..self.map.len() {
            for j in 0..self.map[i].len() {
                let x = (i as i32) * self.cell_size + 8; // Add canvas offset
                let y = (j as i32) * self.cell_size + 34; // Add canvas offset
                
                match self.map[i][j] {
                    Self::LIGHT_TILE => {
                        d.draw_rectangle(x, y, self.cell_size, self.cell_size, light_green);
                    }
                    Self::DARK_TILE => {
                        d.draw_rectangle(x, y, self.cell_size, self.cell_size, dark_green);
                    }
                    Self::WALL => {
                        d.draw_rectangle(x, y, self.cell_size, self.cell_size, wall_color);
                    }
                    Self::SAND => {
                        d.draw_rectangle(x, y, self.cell_size, self.cell_size, sand_color);
                    }
                    _ => {}
                }
            }
        }
    }

    pub fn is_wall(&self, x: i32, y: i32) -> bool {
        let grid_x = (x - 8) / self.cell_size; // Account for canvas offset
        let grid_y = (y - 34) / self.cell_size;

        if grid_x < 0 || grid_x >= self.map.len() as i32 || grid_y < 0 || grid_y >= self.map[0].len() as i32 {
            return false;
        }

        self.map[grid_x as usize][grid_y as usize] == Self::WALL
    }

    pub fn is_sand(&self, x: i32, y: i32) -> bool {
        let grid_x = (x - 8) / self.cell_size; // Account for canvas offset
        let grid_y = (y - 34) / self.cell_size;

        if grid_x < 0 || grid_x >= self.map.len() as i32 || grid_y < 0 || grid_y >= self.map[0].len() as i32 {
            return false;
        }

        self.map[grid_x as usize][grid_y as usize] == Self::SAND
    }

    pub fn get_ball_position(&self) -> (i32, i32) {
        if self.level < self.ball_level_positions.len() {
            let pos = self.ball_level_positions[self.level];
            ((pos.0 as i32) * self.cell_size + 8, (pos.1 as i32) * self.cell_size + 34)
        } else {
            (100, 100) // Default position
        }
    }

    pub fn get_goal_position(&self) -> (i32, i32) {
        if self.level < self.goal_level_positions.len() {
            let pos = self.goal_level_positions[self.level];
            ((pos.1 as i32) * self.cell_size + 8, (pos.0 as i32) * self.cell_size + 34) // Note: swapped in C# version
        } else {
            (200, 200) // Default position
        }
    }

    pub fn get_obstacle_positions(&self) -> Vec<(usize, usize)> {
        if self.level < self.obstacle_positions.len() {
            self.obstacle_positions[self.level].clone()
        } else {
            vec![]
        }
    }

    fn create_maps(&mut self) {
        self.obstacle_positions = vec![];
        self.obstacles = vec![];
        self.levels = vec![];
        self.ball_level_positions = vec![];
        self.goal_level_positions = vec![];

        // Level 1
        self.obstacle_positions.push(vec![]);
        self.obstacles.push(vec![]);
        self.levels.push(vec![
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w'],
            vec!['w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w'],
            vec!['w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w'],
            vec!['w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w'],
            vec!['w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w'],
            vec!['w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w'],
            vec!['w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w'],
            vec!['w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w'],
            vec!['w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w'],
            vec!['w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w'],
            vec!['w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
        ]);
        self.ball_level_positions.push((17, 3));
        self.goal_level_positions.push((16, 31));

        // Level 2
        self.obstacle_positions.push(vec![(21, 10)]);
        self.obstacles.push(vec!['r']);
        self.levels.push(vec![
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w'],
            vec!['w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w'],
            vec!['w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w'],
            vec!['w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w'],
            vec!['w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w'],
            vec!['w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'T', 't', 'T', 't', 'T', 't', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 't', 'T', 't', 'T', 't', 'T', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
            vec!['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
        ]);
        self.ball_level_positions.push((14, 13));
        self.goal_level_positions.push((3, 21));
        
        // Add more levels as needed...
    }
}
