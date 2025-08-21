use raylib::prelude::*;

mod vector;
mod ball;
mod goal;
mod scene;
mod map;
mod verlet;
mod obstacle;
mod triangle;
mod moving_floor;
mod collision;

use vector::Vector2D;
use ball::Ball;
use goal::Goal;
use scene::Scene;
use map::Map;

const WINDOW_WIDTH: i32 = 576; // 568 + 8 padding
const WINDOW_HEIGHT: i32 = 282; // 240 + 34 top + 8 padding

fn main() {
    let (mut rl, thread) = raylib::init()
        .size(WINDOW_WIDTH, WINDOW_HEIGHT)
        .title("GolfIt - Rust")
        .build();

    rl.set_target_fps(100); // Similar to 10ms timer in C#

    let mut level = 0;
    let mut scene = Scene::new(level);
    let ball_pos = scene.map.get_ball_position();
    let goal_pos = scene.map.get_goal_position();
    
    let mut ball = Ball::new(scene.cell_size, Vector2D::new(ball_pos.0 as f32, ball_pos.1 as f32), Vector2D::new(0.0, 0.0));
    let mut goal = Goal::new(scene.cell_size, Vector2D::new(goal_pos.0 as f32, goal_pos.1 as f32), &ball);
    
    let mut is_dragging = false;
    let mut start_point = Vector2::new(0.0, 0.0);
    let mut end_point = Vector2::new(0.0, 0.0);
    let force_limit = 8.0;
    let mut turn = 0;
    let mut is_menu_active = true;
    let mut cnt_t = 0u32;

    while !rl.window_should_close() {
        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::BLACK);

        // Handle input
        if !is_menu_active {
            handle_input(&mut d, &mut ball, &mut is_dragging, &mut start_point, &mut end_point, force_limit);
            
            // Update game logic
            if goal.is_ball_in_goal {
                // Handle level completion
            }
            
            if !ball.is_moving() {
                if is_dragging {
                    // Draw trajectory line
                    let force = Vector2D::new(end_point.x - start_point.x, end_point.y - start_point.y);
                    let force_magnitude = (force.x * force.x + force.y * force.y).sqrt();
                    if force_magnitude <= force_limit {
                        d.draw_line_v(start_point, end_point, Color::WHITE);
                    }
                }
            } else {
                turn += 1;
            }
            
            // Update objects
            scene.update(&mut d, cnt_t);
            ball.update(&mut d, &scene.map);
            goal.update(&mut d, &scene.map, &ball);
        } else {
            draw_menu(&mut d);
        }
        
        // Update counter
        if cnt_t < u32::MAX {
            cnt_t += 1;
        } else {
            cnt_t = 0;
        }
    }
}

fn handle_input(d: &mut RaylibDrawHandle, ball: &mut Ball, is_dragging: &mut bool, start_point: &mut Vector2, end_point: &mut Vector2, force_limit: f32) {
    if d.is_mouse_button_pressed(MouseButton::MOUSE_LEFT_BUTTON) && !ball.is_moving() {
        let mouse_pos = d.get_mouse_position();
        let ball_bounds = 2.0; // Similar to C# version
        
        if mouse_pos.x < ball.position.x - ball.radius - ball_bounds || 
           mouse_pos.x > ball.position.x + ball.radius + ball_bounds ||
           mouse_pos.y < ball.position.y - ball.radius - ball_bounds || 
           mouse_pos.y > ball.position.y + ball.radius + ball_bounds {
            return;
        }
        
        *is_dragging = true;
        *start_point = mouse_pos;
    }
    
    if d.is_mouse_button_down(MouseButton::MOUSE_LEFT_BUTTON) && *is_dragging && !ball.is_moving() {
        *end_point = d.get_mouse_position();
    }
    
    if d.is_mouse_button_released(MouseButton::MOUSE_LEFT_BUTTON) && *is_dragging {
        let force = Vector2D::new(end_point.x - start_point.x, end_point.y - start_point.y);
        let force_magnitude = (force.x * force.x + force.y * force.y).sqrt();
        
        if force_magnitude <= force_limit {
            ball.push_ball(force);
        }
        
        *is_dragging = false;
    }
}

fn draw_menu(d: &mut RaylibDrawHandle) {
    d.draw_text("GolfIt - Rust Edition", 200, 100, 20, Color::WHITE);
    d.draw_text("Click to start", 240, 150, 16, Color::WHITE);
}
