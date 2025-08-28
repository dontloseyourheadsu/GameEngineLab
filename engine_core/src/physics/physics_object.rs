use raylib::prelude::*;

pub trait PhysicsObject {
    // Basic physics properties
    fn position(&self) -> Vector2;
    fn set_position(&mut self, position: Vector2);
    fn previous_position(&self) -> Vector2;
    fn mass(&self) -> f32;
    fn set_mass(&mut self, mass: f32);

    // Force application
    fn apply_force(&mut self, force: Vector2);
    fn apply_impulse_to_previous(&mut self, impulse: Vector2);

    // Verlet integration update
    fn update(&mut self, delta_time: f32, world_bounds: Rectangle);

    // Collision mesh access
    fn collision_mesh(&self) -> Option<&crate::collision::CollisionMesh>;
    fn collision_mesh_mut(&mut self) -> Option<&mut crate::collision::CollisionMesh>;
    fn has_collision(&self) -> bool;

    // Rendering
    fn draw(&self, d: &mut RaylibDrawHandle, texture: Option<&Texture2D>);
    fn draw_simple(&self, d: &mut RaylibDrawHandle);
}
