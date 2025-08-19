use raylib::prelude::*;
use rand::Rng;

pub struct Pill;

impl Pill {
    pub fn draw_pill(d: &mut RaylibDrawHandle, x: i32, y: i32, cell_size: i32, cnt_t: i32) {
        let mut rng = rand::thread_rng();
        
        if ((cnt_t + rng.gen_range(1..=5)) % 5) == 0 {
            d.draw_circle(
                x * cell_size + 8 + cell_size / 2,
                y * cell_size + 8 + cell_size / 2,
                (cell_size - 16) as f32 / 2.0,
                Color::new(218, 165, 32, 255), // Goldenrod
            );
        } else {
            d.draw_circle(
                x * cell_size + 8 + cell_size / 2,
                y * cell_size + 8 + cell_size / 2,
                (cell_size - 16) as f32 / 2.0,
                Color::GOLD,
            );
            d.draw_circle(
                x * cell_size + 10 + cell_size / 2,
                y * cell_size + 10 + cell_size / 2,
                (cell_size - 20) as f32 / 2.0,
                Color::new(218, 165, 32, 255), // Goldenrod
            );
        }
    }
    
    pub fn draw_power_pill(d: &mut RaylibDrawHandle, x: i32, y: i32, cell_size: i32, cnt_t: i32) {
        let mut rng = rand::thread_rng();
        
        if ((cnt_t + rng.gen_range(1..=10)) % 10) == 0 {
            d.draw_circle(
                x * cell_size + cell_size / 2,
                y * cell_size + cell_size / 2,
                cell_size as f32 / 2.0,
                Color::new(35, 200, 180, 255),
            );
            d.draw_circle(
                x * cell_size + 3 + cell_size / 2,
                y * cell_size + 3 + cell_size / 2,
                (cell_size - 6) as f32 / 2.0,
                Color::YELLOW,
            );
            d.draw_circle(
                x * cell_size + 5 + cell_size / 2,
                y * cell_size + 5 + cell_size / 2,
                (cell_size - 10) as f32 / 2.0,
                Color::GOLD,
            );
        } else {
            d.draw_circle(
                x * cell_size + cell_size / 2,
                y * cell_size + cell_size / 2,
                cell_size as f32 / 2.0,
                Color::new(100, 200, 200, 180),
            );
            d.draw_circle(
                x * cell_size + 2 + cell_size / 2,
                y * cell_size + 2 + cell_size / 2,
                (cell_size - 4) as f32 / 2.0,
                Color::ORANGE,
            );
            d.draw_circle(
                x * cell_size + 5 + cell_size / 2,
                y * cell_size + 5 + cell_size / 2,
                (cell_size - 10) as f32 / 2.0,
                Color::new(250, 240, 230, 255), // Linen
            );
        }
    }
}

pub struct Brick;

impl Brick {
    pub fn draw_brick(d: &mut RaylibDrawHandle, x: i32, y: i32, cell_size: i32) {
        d.draw_rectangle(
            x * cell_size,
            y * cell_size,
            cell_size,
            cell_size,
            Color::new(188, 143, 143, 255), // Rosy brown
        );
        d.draw_rectangle(
            x * cell_size + 4,
            y * cell_size + 4,
            cell_size - 8,
            cell_size - 8,
            Color::new(139, 69, 19, 255), // Saddle brown
        );
    }
}
