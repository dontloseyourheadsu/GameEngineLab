use raylib::prelude::*;
use crate::vector::Vector2D;
use crate::verlet::Verlet;
use crate::map::Map;
use crate::ball::Ball;

pub struct Goal {
    cell_size: f32,
    position: Vector2D,
    pub is_ball_in_goal: bool,
}

impl Goal {
    pub fn new(cell_size: i32, position: Vector2D, _ball: &Ball) -> Self {
        Self {
            cell_size: (cell_size as f32) * 1.25,
            position,
            is_ball_in_goal: false,
        }
    }

    pub fn render(&self, d: &mut RaylibDrawHandle) {
        d.draw_circle(
            self.position.x as i32,
            self.position.y as i32,
            self.cell_size / 2.0,
            Color::BLACK,
        );
    }

    pub fn update(&mut self, d: &mut RaylibDrawHandle, _map: &Map, ball: &Ball) {
        let distance = self.position.distance(&ball.position);
        let max_distance = self.cell_size / 2.0 + ball.radius;

        if distance < max_distance {
            if ball.can_enter_goal() {
                self.is_ball_in_goal = true;
            }
        } else {
            self.is_ball_in_goal = false;
        }

        self.render(d);
    }
}
