use crate::map::Map;
use raylib::prelude::*;

pub trait Verlet {
    fn update(&mut self, d: &mut RaylibDrawHandle, map: &Map);
}
