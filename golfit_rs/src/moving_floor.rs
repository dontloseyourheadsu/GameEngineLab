use raylib::prelude::*;
use crate::vector::Vector2D;
use crate::obstacle::Obstacle;
use crate::map::Map;
use crate::ball::Ball;

#[derive(Debug, Clone, Copy)]
pub enum Direction {
    Up,
    Down,
    Left,
    Right,
}

pub struct MovingFloor {
    pub position: Vector2D,
    pub direction: Direction,
    cell_size: i32,
    floor_force: f32,
    points: [Vector2; 6],
}

impl MovingFloor {
    pub fn new(position: Vector2D, direction: Direction, cell_size: i32) -> Self {
        let mut moving_floor = Self {
            position,
            direction,
            cell_size,
            floor_force: 0.15,
            points: [Vector2::new(0.0, 0.0); 6],
        };
        moving_floor.calculate_arrow_points();
        moving_floor
    }

    pub fn collided(&self, ball: &Ball) -> bool {
        let top = self.is_moving_floor(ball.position.x as i32, (ball.position.y - ball.radius) as i32);
        let bottom = self.is_moving_floor(ball.position.x as i32, (ball.position.y + ball.radius) as i32);
        let left = self.is_moving_floor((ball.position.x - ball.radius) as i32, ball.position.y as i32);
        let right = self.is_moving_floor((ball.position.x + ball.radius) as i32, ball.position.y as i32);
        let top_left = self.is_moving_floor((ball.position.x - ball.radius) as i32, (ball.position.y - ball.radius) as i32);
        let top_right = self.is_moving_floor((ball.position.x + ball.radius) as i32, (ball.position.y - ball.radius) as i32);
        let bottom_left = self.is_moving_floor((ball.position.x - ball.radius) as i32, (ball.position.y + ball.radius) as i32);
        let bottom_right = self.is_moving_floor((ball.position.x + ball.radius) as i32, (ball.position.y + ball.radius) as i32);

        top || bottom || left || right || top_left || top_right || bottom_left || bottom_right
    }

    fn is_moving_floor(&self, x: i32, y: i32) -> bool {
        let grid_x = (x - 8) / self.cell_size; // Account for canvas offset
        let grid_y = (y - 34) / self.cell_size;
        
        // Check if this position matches our moving floor position
        let pos_x = (self.position.x as i32) / self.cell_size;
        let pos_y = (self.position.y as i32) / self.cell_size;
        
        grid_x == pos_x && grid_y == pos_y
    }

    fn calculate_arrow_points(&mut self) {
        let cell_f = self.cell_size as f32;
        let _half_cell = cell_f / 2.0;
        let quarter_cell = cell_f / 4.0;

        match self.direction {
            Direction::Up => {
                self.points = [
                    Vector2::new(self.position.x, self.position.y - quarter_cell),
                    Vector2::new(self.position.x - quarter_cell, self.position.y),
                    Vector2::new(self.position.x - quarter_cell / 2.0, self.position.y),
                    Vector2::new(self.position.x - quarter_cell / 2.0, self.position.y + quarter_cell),
                    Vector2::new(self.position.x + quarter_cell / 2.0, self.position.y + quarter_cell),
                    Vector2::new(self.position.x + quarter_cell / 2.0, self.position.y),
                ];
            }
            Direction::Down => {
                self.points = [
                    Vector2::new(self.position.x, self.position.y + quarter_cell),
                    Vector2::new(self.position.x - quarter_cell, self.position.y),
                    Vector2::new(self.position.x - quarter_cell / 2.0, self.position.y),
                    Vector2::new(self.position.x - quarter_cell / 2.0, self.position.y - quarter_cell),
                    Vector2::new(self.position.x + quarter_cell / 2.0, self.position.y - quarter_cell),
                    Vector2::new(self.position.x + quarter_cell / 2.0, self.position.y),
                ];
            }
            Direction::Left => {
                self.points = [
                    Vector2::new(self.position.x - quarter_cell, self.position.y),
                    Vector2::new(self.position.x, self.position.y - quarter_cell),
                    Vector2::new(self.position.x, self.position.y - quarter_cell / 2.0),
                    Vector2::new(self.position.x + quarter_cell, self.position.y - quarter_cell / 2.0),
                    Vector2::new(self.position.x + quarter_cell, self.position.y + quarter_cell / 2.0),
                    Vector2::new(self.position.x, self.position.y + quarter_cell / 2.0),
                ];
            }
            Direction::Right => {
                self.points = [
                    Vector2::new(self.position.x + quarter_cell, self.position.y),
                    Vector2::new(self.position.x, self.position.y - quarter_cell),
                    Vector2::new(self.position.x, self.position.y - quarter_cell / 2.0),
                    Vector2::new(self.position.x - quarter_cell, self.position.y - quarter_cell / 2.0),
                    Vector2::new(self.position.x - quarter_cell, self.position.y + quarter_cell / 2.0),
                    Vector2::new(self.position.x, self.position.y + quarter_cell / 2.0),
                ];
            }
        }
    }
}

impl Obstacle for MovingFloor {
    fn detect_collision(&self, ball: &Ball) -> Vector2D {
        let mut velocity = Vector2D::zero();

        if !self.collided(ball) {
            return velocity;
        }

        match self.direction {
            Direction::Up => velocity.y = -self.floor_force,
            Direction::Down => velocity.y = self.floor_force,
            Direction::Left => velocity.x = -self.floor_force,
            Direction::Right => velocity.x = self.floor_force,
        }

        velocity
    }

    fn render(&self, d: &mut RaylibDrawHandle) {
        // Draw the arrow shape
        for i in 0..self.points.len() {
            let next_i = (i + 1) % self.points.len();
            d.draw_line_v(self.points[i], self.points[next_i], Color::BLUE);
        }
        
        // Fill the arrow (simple approach)
        let center = Vector2::new(self.position.x, self.position.y);
        d.draw_circle_v(center, (self.cell_size / 4) as f32, Color::BLUE);
    }

    fn update(&mut self, d: &mut RaylibDrawHandle, _map: &Map, _cnt_t: u32) {
        self.render(d);
    }
}
