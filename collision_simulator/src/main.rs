use engine_core::{physics_world::PhysicsWorld, world_renderer};
use nalgebra::vector;

fn main() {
    // World size 100x100, gravity downwards
    let mut physics_world = PhysicsWorld::new(100, 100, vector![0.0, -9.81]);

    // Place ball above the floor, in the middle
    physics_world.add_ball(vector![50.0, 20.0], 5.0, 0.7);

    world_renderer::render(&mut physics_world);
}