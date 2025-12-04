use crate::collision_body::circle_collision_body::CircleCollisionBody;
use raylib::prelude::*;

pub struct Spring {
    pub rest_length: f32,
    pub stiffness: f32,
    pub damping: f32,
}

impl Spring {
    pub fn new(rest_length: f32, stiffness: f32, damping: f32) -> Self {
        Self {
            rest_length,
            stiffness,
            damping,
        }
    }

    pub fn update(&self, body_a: &mut CircleCollisionBody, body_b: &mut CircleCollisionBody, dt: f32) {
        let delta = body_b.position - body_a.position;
        let dist = delta.length();
        
        if dist == 0.0 { return; }

        let direction = delta / dist;
        let stretch = dist - self.rest_length;
        
        // Calculate relative velocity
        // Velocity = (position - old_position) / dt
        // We use (position - old_position) as "velocity * dt"
        let vel_a_dt = body_a.position - body_a.old_position;
        let vel_b_dt = body_b.position - body_b.old_position;
        
        let rel_vel_dt = vel_b_dt - vel_a_dt;
        let rel_vel_proj_dt = rel_vel_dt.dot(direction);
        
        // Force = Stiffness * Stretch + Damping * RelativeVelocity
        // F = k * x + c * v
        // F = k * stretch + c * (rel_vel_dt / dt)
        
        // Verlet integration: x_new = x + v + a * dt * dt
        // a = F / m (assume m=1)
        // displacement = F * dt * dt
        // displacement = (k * stretch + c * rel_vel_dt / dt) * dt * dt
        // displacement = k * stretch * dt * dt + c * rel_vel_dt * dt
        
        let spring_force_disp = self.stiffness * stretch * dt * dt;
        let damping_force_disp = self.damping * rel_vel_proj_dt * dt;
        
        let total_displacement = direction * (spring_force_disp + damping_force_disp);
        
        // Apply forces (Newton's 3rd law)
        // If stretch is positive (extended), we want to pull them together.
        // Force on A is towards B (positive direction).
        // Force on B is towards A (negative direction).
        
        body_a.position += total_displacement;
        body_b.position -= total_displacement;
    }
    
    pub fn draw(&self, d: &mut RaylibDrawHandle, body_a: &CircleCollisionBody, body_b: &CircleCollisionBody) {
        d.draw_line_v(body_a.position, body_b.position, Color::BLACK);
    }
}
