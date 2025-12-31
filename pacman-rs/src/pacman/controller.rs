use raylib::prelude::*;
use crate::pacman::pacman::Pacman;

pub fn handle_input(rl: &RaylibHandle, pacman: &mut Pacman) {
    if rl.is_key_down(KeyboardKey::KEY_W) {
        pacman.desired_direction = Vector2::new(0.0, -1.0);
    }
    if rl.is_key_down(KeyboardKey::KEY_S) {
        pacman.desired_direction = Vector2::new(0.0, 1.0);
    }
    if rl.is_key_down(KeyboardKey::KEY_A) {
        pacman.desired_direction = Vector2::new(-1.0, 0.0);
    }
    if rl.is_key_down(KeyboardKey::KEY_D) {
        pacman.desired_direction = Vector2::new(1.0, 0.0);
    }
}
