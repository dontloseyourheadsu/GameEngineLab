use engine_core::{physics_world::PhysicsWorld, world_renderer};
use nalgebra::vector;

fn main() {
    // World size 100x100, gravity downwards
    let mut physics_world = PhysicsWorld::new(100, 100, vector![0.0, -9.81]);

    // Place ball above the floor, in the middle
    physics_world.add_ball(vector![50.0, 50.0], 15.0, 0.7);
    physics_world.add_ball(vector![35.0, 100.0], 15.0, 0.1);

    world_renderer::render("Collision Simulator", &mut physics_world, (1000, 1000), (200.0, 200.0));
}