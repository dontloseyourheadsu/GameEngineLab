use engine_core::character::character_2d::Character2D;
use raylib::math::Vector2;

pub struct Pacman {
    pub character: Character2D,
    pub current_direction: Vector2,
    pub desired_direction: Vector2,
    pub grid_position: (usize, usize),
    pub is_moving: bool,
    pub move_progress: f32,
    pub previous_grid_position: (usize, usize),
    pub is_teleporting: bool,
}
