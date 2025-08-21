use raylib::prelude::*;
use crate::vector::Vector2D;
use crate::map::Map;
use crate::ball::Ball;

pub trait Obstacle {
    fn detect_collision(&self, ball: &Ball) -> Vector2D;
    fn render(&self, d: &mut RaylibDrawHandle);
    fn update(&mut self, d: &mut RaylibDrawHandle, map: &Map, cnt_t: u32);
}
