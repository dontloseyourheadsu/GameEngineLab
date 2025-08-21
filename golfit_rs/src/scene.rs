use raylib::prelude::*;
use crate::map::Map;
use crate::triangle::Triangle;
use crate::obstacle::Obstacle;
use crate::vector::Vector2D;

pub struct Scene {
    pub map: Map,
    pub cell_size: i32,
    pub obstacles: Vec<Box<dyn Obstacle>>,
}

impl Scene {
    pub fn new(level: usize) -> Self {
        let cell_size = 20;
        let map = Map::new(cell_size, level);
        let mut scene = Self {
            map,
            cell_size,
            obstacles: vec![],
        };
        scene.create_obstacles(level);
        scene
    }

    pub fn update(&mut self, d: &mut RaylibDrawHandle, cnt_t: u32) {
        self.render(d, cnt_t);
    }

    pub fn render(&mut self, d: &mut RaylibDrawHandle, cnt_t: u32) {
        self.map.render(d, cnt_t);
        
        // Update and render obstacles
        for obstacle in &mut self.obstacles {
            obstacle.update(d, &self.map, cnt_t);
        }
    }

    fn create_obstacles(&mut self, level: usize) {
        let obstacle_positions = self.map.get_obstacle_positions();
        
        // For level 2, add a triangle at position (21, 10)
        if level == 1 && !obstacle_positions.is_empty() {
            let triangle = Triangle::new(
                self.cell_size,
                Vector2D::new(obstacle_positions[0].0 as f32, obstacle_positions[0].1 as f32)
            );
            self.obstacles.push(Box::new(triangle));
        }
        
        // Add more obstacles for different levels as needed
    }
}
