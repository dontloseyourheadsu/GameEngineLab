use raylib::prelude::*;
use crate::vector::Vector2D;
use crate::obstacle::Obstacle;
use crate::map::Map;
use crate::ball::Ball;
use std::f32::consts::PI;

pub struct Triangle {
    cell_size: f32,
    position: Vector2D,
    points: [Vector2; 3],
    color: Color,
    width: f32,
    height: f32,
    rotation_speed: f32,
    original_points: [Vector2D; 3],
}

impl Triangle {
    pub fn new(cell_size: i32, position: Vector2D) -> Self {
        let colors = [Color::LIGHTBLUE, Color::PINK, Color::BEIGE];
        let color = colors[fastrand::usize(..colors.len())];
        let width = 5.0 * cell_size as f32;
        let height = 5.0 * cell_size as f32;
        let rotation_speed = 2000.0;

        let original_points = [
            Vector2D::new(0.0, -height / 2.0),
            Vector2D::new(-width / 2.0, height / 2.0),
            Vector2D::new(width / 2.0, height / 2.0),
        ];

        Self {
            cell_size: cell_size as f32,
            position: position * cell_size as f32,
            points: [Vector2::new(0.0, 0.0); 3],
            color,
            width,
            height,
            rotation_speed,
            original_points,
        }
    }

    pub fn set_rotation_speed(&mut self, new_speed: f32) {
        self.rotation_speed = new_speed;
    }

    fn calculate_points(&mut self, cnt_t: u32) {
        let angle_degrees = ((cnt_t as f32) % self.rotation_speed) * (360.0 / self.rotation_speed);
        let angle_radians = angle_degrees * PI / 180.0;

        for i in 0..self.original_points.len() {
            let x = self.original_points[i].x;
            let y = self.original_points[i].y;

            self.points[i] = Vector2::new(
                self.position.x + x * angle_radians.cos() - y * angle_radians.sin(),
                self.position.y + x * angle_radians.sin() + y * angle_radians.cos(),
            );
        }
    }
}

impl Obstacle for Triangle {
    fn detect_collision(&self, ball: &Ball) -> Vector2D {
        let mut collision_normal = Vector2D::zero();
        let vertices = [
            Vector2D::new(self.points[0].x, self.points[0].y),
            Vector2D::new(self.points[1].x, self.points[1].y),
            Vector2D::new(self.points[2].x, self.points[2].y),
        ];

        for i in 0..3 {
            let edge = vertices[(i + 1) % 3] - vertices[i];
            let edge_normal = Vector2D::new(-edge.y, edge.x).normalized();

            let ball_to_vertex = ball.position - vertices[i];
            let distance = ball_to_vertex.dot(&edge_normal);

            if distance > 0.0 {
                let vertex_to_ball = ball.position - vertices[i];
                let dot_product = vertex_to_ball.dot(&edge);
                let edge_length = edge.length();
                let edge_length_squared = edge_length * edge_length;
                let t = 0.0_f32.max(1.0_f32.min(dot_product / edge_length_squared));

                let closest_point = vertices[i] + edge * t;
                let ball_to_closest = ball.position - closest_point;
                let distance_squared = ball_to_closest.mag_sqr();

                if distance_squared < ball.radius * ball.radius {
                    collision_normal = ball_to_closest.normalized();
                    break;
                }
            }
        }

        collision_normal
    }

    fn render(&self, d: &mut RaylibDrawHandle) {
        let points_i32: Vec<Vector2> = self.points.iter().map(|p| Vector2::new(p.x, p.y)).collect();
        
        // Draw filled triangle
        d.draw_triangle(points_i32[0], points_i32[1], points_i32[2], self.color);
        
        // Draw triangle outline
        d.draw_triangle_lines(points_i32[0], points_i32[1], points_i32[2], Color::BLACK);
    }

    fn update(&mut self, d: &mut RaylibDrawHandle, _map: &Map, cnt_t: u32) {
        self.calculate_points(cnt_t);
        self.render(d);
    }
}
