use raylib::prelude::*;
use crate::map::Map;

pub trait Verlet {
    fn update(&mut self, d: &mut RaylibDrawHandle, map: &Map);
}
