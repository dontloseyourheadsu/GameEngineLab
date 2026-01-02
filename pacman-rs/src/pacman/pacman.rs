use engine_core::character::character_2d::Character2D;
use raylib::math::Vector2;

pub struct Pacman {
    pub character: Character2D,
    pub current_direction: Vector2,
    pub desired_direction: Vector2,
    pub grid_position: (usize, usize),
}
