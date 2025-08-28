use crate::collision::{CircleCollisionMesh, CollisionMesh, RectangleCollisionMesh};
use crate::physics::{PhysicsObject, VerletPoint};
use raylib::prelude::*;

// Generic collision detection and resolution for any PhysicsObject
pub fn check_collision<T: PhysicsObject, U: PhysicsObject>(obj1: &T, obj2: &U) -> bool {
    if !obj1.has_collision() || !obj2.has_collision() {
        return false;
    }

    let mesh1 = obj1.collision_mesh().unwrap();
    let mesh2 = obj2.collision_mesh().unwrap();

    match (mesh1, mesh2) {
        (CollisionMesh::Circle(c1), CollisionMesh::Circle(c2)) => {
            check_circle_circle_collision(obj1.position(), c1, obj2.position(), c2)
        }
        (CollisionMesh::Circle(c), CollisionMesh::Rectangle(r)) => {
            check_circle_rectangle_collision(obj1.position(), c, obj2.position(), r)
        }
        (CollisionMesh::Rectangle(r), CollisionMesh::Circle(c)) => {
            check_circle_rectangle_collision(obj2.position(), c, obj1.position(), r)
        }
        (CollisionMesh::Rectangle(r1), CollisionMesh::Rectangle(r2)) => {
            check_rectangle_rectangle_collision(obj1.position(), r1, obj2.position(), r2)
        }
    }
}

pub fn resolve_collision<T: PhysicsObject, U: PhysicsObject>(obj1: &mut T, obj2: &mut U) {
    if !check_collision(obj1, obj2) {
        return;
    }

    // Copy collision mesh data before borrowing mutably
    let mesh1_copy = *obj1.collision_mesh().unwrap();
    let mesh2_copy = *obj2.collision_mesh().unwrap();

    match (mesh1_copy, mesh2_copy) {
        (CollisionMesh::Circle(c1), CollisionMesh::Circle(c2)) => {
            resolve_circle_circle_collision(obj1, obj2, &c1, &c2)
        }
        (CollisionMesh::Circle(c), CollisionMesh::Rectangle(r)) => {
            resolve_circle_rectangle_collision(obj1, obj2, &c, &r)
        }
        (CollisionMesh::Rectangle(r), CollisionMesh::Circle(c)) => {
            resolve_rectangle_circle_collision(obj1, obj2, &r, &c)
        }
        (CollisionMesh::Rectangle(r1), CollisionMesh::Rectangle(r2)) => {
            resolve_rectangle_rectangle_collision(obj1, obj2, &r1, &r2)
        }
    }
}

// Circle-Circle collision detection
fn check_circle_circle_collision(
    pos1: Vector2,
    c1: &CircleCollisionMesh,
    pos2: Vector2,
    c2: &CircleCollisionMesh,
) -> bool {
    let distance_squared = (pos1.x - pos2.x).powi(2) + (pos1.y - pos2.y).powi(2);
    let min_distance = c1.radius + c2.radius;
    distance_squared < min_distance.powi(2)
}

// Circle-Rectangle collision detection using closest point method
fn check_circle_rectangle_collision(
    circle_pos: Vector2,
    circle: &CircleCollisionMesh,
    rect_pos: Vector2,
    rect: &RectangleCollisionMesh,
) -> bool {
    // Find the closest point on the rectangle to the circle center
    let closest_point = closest_point_on_rectangle(circle_pos, rect_pos, rect);
    let distance_squared =
        (circle_pos.x - closest_point.x).powi(2) + (circle_pos.y - closest_point.y).powi(2);
    distance_squared <= circle.radius.powi(2)
}

// Rectangle-Rectangle collision detection using SAT (Separated Axis Theorem)
fn check_rectangle_rectangle_collision(
    pos1: Vector2,
    r1: &RectangleCollisionMesh,
    pos2: Vector2,
    r2: &RectangleCollisionMesh,
) -> bool {
    // Get axes from both rectangles
    let axes1 = r1.get_axes();
    let axes2 = r2.get_axes();

    // Get corners for both rectangles
    let corners1 = r1.get_corners(pos1);
    let corners2 = r2.get_corners(pos2);

    // Test all axes
    for axis in axes1.iter().chain(axes2.iter()) {
        if !sat_overlap(&corners1, &corners2, *axis) {
            return false; // Separation found
        }
    }

    true // No separation found, they are colliding
}

// SAT overlap test
fn sat_overlap(corners1: &[Vector2; 4], corners2: &[Vector2; 4], axis: Vector2) -> bool {
    // Project both rectangles onto the axis
    let proj1 = project_onto_axis(corners1, axis);
    let proj2 = project_onto_axis(corners2, axis);

    // Check for overlap
    proj1.0 <= proj2.1 && proj2.0 <= proj1.1
}

// Project corners onto axis and return (min, max)
fn project_onto_axis(corners: &[Vector2; 4], axis: Vector2) -> (f32, f32) {
    let mut min = corners[0].x * axis.x + corners[0].y * axis.y;
    let mut max = min;

    for corner in &corners[1..] {
        let projection = corner.x * axis.x + corner.y * axis.y;
        if projection < min {
            min = projection;
        }
        if projection > max {
            max = projection;
        }
    }

    (min, max)
}

// Find closest point on rectangle to a given point
fn closest_point_on_rectangle(
    point: Vector2,
    rect_pos: Vector2,
    rect: &RectangleCollisionMesh,
) -> Vector2 {
    // Transform point to rectangle's local space
    let cos_r = rect.rotation.cos();
    let sin_r = rect.rotation.sin();

    let local_x = (point.x - rect_pos.x) * cos_r + (point.y - rect_pos.y) * sin_r;
    let local_y = -(point.x - rect_pos.x) * sin_r + (point.y - rect_pos.y) * cos_r;

    // Clamp to rectangle bounds
    let half_width = rect.width / 2.0;
    let half_height = rect.height / 2.0;

    let clamped_x = local_x.clamp(-half_width, half_width);
    let clamped_y = local_y.clamp(-half_height, half_height);

    // Transform back to world space
    Vector2::new(
        rect_pos.x + clamped_x * cos_r - clamped_y * sin_r,
        rect_pos.y + clamped_x * sin_r + clamped_y * cos_r,
    )
}

// Circle-Circle collision resolution
fn resolve_circle_circle_collision<T: PhysicsObject, U: PhysicsObject>(
    obj1: &mut T,
    obj2: &mut U,
    c1: &CircleCollisionMesh,
    c2: &CircleCollisionMesh,
) {
    // Calculate collision normal
    let dx = obj2.position().x - obj1.position().x;
    let dy = obj2.position().y - obj1.position().y;
    let distance = (dx * dx + dy * dy).sqrt();

    if distance == 0.0 {
        return; // Avoid division by zero
    }

    let nx = dx / distance;
    let ny = dy / distance;

    // Calculate overlap
    let overlap = (c1.radius + c2.radius) - distance;

    // Separate the objects based on their masses
    let total_mass = obj1.mass() + obj2.mass();
    let obj1_separation = overlap * (obj2.mass() / total_mass);
    let obj2_separation = overlap * (obj1.mass() / total_mass);

    // Move objects apart
    let mut pos1 = obj1.position();
    let mut pos2 = obj2.position();

    pos1.x -= nx * obj1_separation;
    pos1.y -= ny * obj1_separation;
    pos2.x += nx * obj2_separation;
    pos2.y += ny * obj2_separation;

    obj1.set_position(pos1);
    obj2.set_position(pos2);

    // Calculate relative velocity
    let vel1 = Vector2::new(
        obj1.position().x - obj1.previous_position().x,
        obj1.position().y - obj1.previous_position().y,
    );
    let vel2 = Vector2::new(
        obj2.position().x - obj2.previous_position().x,
        obj2.position().y - obj2.previous_position().y,
    );

    let relative_velocity = Vector2::new(vel2.x - vel1.x, vel2.y - vel1.y);

    // Calculate relative velocity in collision normal direction
    let velocity_along_normal = relative_velocity.x * nx + relative_velocity.y * ny;

    // Do not resolve if velocities are separating
    if velocity_along_normal > 0.0 {
        return;
    }

    // Calculate restitution
    let restitution = (c1.restitution + c2.restitution) * 0.5;

    // Calculate impulse scalar
    let impulse_scalar = -(1.0 + restitution) * velocity_along_normal;
    let impulse_scalar = impulse_scalar / (1.0 / obj1.mass() + 1.0 / obj2.mass());

    // Apply impulse
    let impulse = Vector2::new(impulse_scalar * nx, impulse_scalar * ny);

    // Update previous positions to simulate velocity change
    obj1.apply_impulse_to_previous(Vector2::new(
        impulse.x / obj1.mass(),
        impulse.y / obj1.mass(),
    ));
    obj2.apply_impulse_to_previous(Vector2::new(
        -impulse.x / obj2.mass(),
        -impulse.y / obj2.mass(),
    ));
}

// Circle-Rectangle collision resolution
fn resolve_circle_rectangle_collision<T: PhysicsObject, U: PhysicsObject>(
    circle_obj: &mut T,
    rect_obj: &mut U,
    circle: &CircleCollisionMesh,
    rect: &RectangleCollisionMesh,
) {
    let closest_point =
        closest_point_on_rectangle(circle_obj.position(), rect_obj.position(), rect);
    let collision_normal = Vector2::new(
        circle_obj.position().x - closest_point.x,
        circle_obj.position().y - closest_point.y,
    );

    let distance =
        (collision_normal.x * collision_normal.x + collision_normal.y * collision_normal.y).sqrt();
    if distance == 0.0 {
        return;
    }

    let nx = collision_normal.x / distance;
    let ny = collision_normal.y / distance;

    let overlap = circle.radius - distance;
    if overlap <= 0.0 {
        return;
    }

    // Separate objects
    let total_mass = circle_obj.mass() + rect_obj.mass();
    let circle_separation = overlap * (rect_obj.mass() / total_mass);
    let rect_separation = overlap * (circle_obj.mass() / total_mass);

    let mut circle_pos = circle_obj.position();
    let mut rect_pos = rect_obj.position();

    circle_pos.x += nx * circle_separation;
    circle_pos.y += ny * circle_separation;
    rect_pos.x -= nx * rect_separation;
    rect_pos.y -= ny * rect_separation;

    circle_obj.set_position(circle_pos);
    rect_obj.set_position(rect_pos);

    // Apply impulse (simplified)
    let restitution = (circle.restitution + rect.restitution) * 0.5;
    apply_collision_impulse(circle_obj, rect_obj, Vector2::new(nx, ny), restitution);
}

// Rectangle-Circle collision resolution (wrapper)
fn resolve_rectangle_circle_collision<T: PhysicsObject, U: PhysicsObject>(
    rect_obj: &mut T,
    circle_obj: &mut U,
    rect: &RectangleCollisionMesh,
    circle: &CircleCollisionMesh,
) {
    resolve_circle_rectangle_collision(circle_obj, rect_obj, circle, rect);
}

// Rectangle-Rectangle collision resolution (basic implementation)
fn resolve_rectangle_rectangle_collision<T: PhysicsObject, U: PhysicsObject>(
    obj1: &mut T,
    obj2: &mut U,
    r1: &RectangleCollisionMesh,
    r2: &RectangleCollisionMesh,
) {
    // Find minimum translation vector using SAT
    let mut min_overlap = f32::INFINITY;
    let mut mtv = Vector2::zero();

    let axes1 = r1.get_axes();
    let axes2 = r2.get_axes();
    let corners1 = r1.get_corners(obj1.position());
    let corners2 = r2.get_corners(obj2.position());

    // Test all axes and find minimum overlap
    for axis in axes1.iter().chain(axes2.iter()) {
        let proj1 = project_onto_axis(&corners1, *axis);
        let proj2 = project_onto_axis(&corners2, *axis);

        // Check for separation on this axis
        if proj1.1 < proj2.0 || proj2.1 < proj1.0 {
            return; // No collision - objects are separated on this axis
        }

        // Calculate overlap
        let overlap = (proj1.1 - proj2.0).min(proj2.1 - proj1.0);
        if overlap < min_overlap {
            min_overlap = overlap;
            mtv = *axis * overlap;
        }
    }

    // If we get here, there is a collision
    if min_overlap <= 0.0 || min_overlap == f32::INFINITY {
        return; // Invalid overlap
    }

    // Ensure MTV points from obj1 to obj2
    let center_diff = Vector2::new(
        obj2.position().x - obj1.position().x,
        obj2.position().y - obj1.position().y,
    );
    if mtv.x * center_diff.x + mtv.y * center_diff.y < 0.0 {
        mtv.x = -mtv.x;
        mtv.y = -mtv.y;
    }

    // Separate objects (with bounds checking)
    let total_mass = obj1.mass() + obj2.mass();
    let separation1 = (min_overlap * (obj2.mass() / total_mass)).min(50.0); // Cap separation
    let separation2 = (min_overlap * (obj1.mass() / total_mass)).min(50.0); // Cap separation

    let mut pos1 = obj1.position();
    let mut pos2 = obj2.position();

    let normal = Vector2::new(mtv.x / min_overlap, mtv.y / min_overlap);

    pos1.x -= normal.x * separation1;
    pos1.y -= normal.y * separation1;
    pos2.x += normal.x * separation2;
    pos2.y += normal.y * separation2;

    obj1.set_position(pos1);
    obj2.set_position(pos2);

    // Apply gentler impulse
    let restitution = (r1.restitution + r2.restitution) * 0.3; // Reduced restitution
    apply_collision_impulse(obj1, obj2, normal, restitution);
}

// Generic impulse application
fn apply_collision_impulse<T: PhysicsObject, U: PhysicsObject>(
    obj1: &mut T,
    obj2: &mut U,
    normal: Vector2,
    restitution: f32,
) {
    // Calculate relative velocity
    let vel1 = Vector2::new(
        obj1.position().x - obj1.previous_position().x,
        obj1.position().y - obj1.previous_position().y,
    );
    let vel2 = Vector2::new(
        obj2.position().x - obj2.previous_position().x,
        obj2.position().y - obj2.previous_position().y,
    );

    let relative_velocity = Vector2::new(vel2.x - vel1.x, vel2.y - vel1.y);
    let velocity_along_normal = relative_velocity.x * normal.x + relative_velocity.y * normal.y;

    if velocity_along_normal > 0.0 {
        return; // Objects separating
    }

    let impulse_scalar = -(1.0 + restitution) * velocity_along_normal;
    let impulse_scalar = impulse_scalar / (1.0 / obj1.mass() + 1.0 / obj2.mass());

    let impulse = Vector2::new(impulse_scalar * normal.x, impulse_scalar * normal.y);

    obj1.apply_impulse_to_previous(Vector2::new(
        impulse.x / obj1.mass(),
        impulse.y / obj1.mass(),
    ));
    obj2.apply_impulse_to_previous(Vector2::new(
        -impulse.x / obj2.mass(),
        -impulse.y / obj2.mass(),
    ));
}

// Legacy implementations for VerletPoint (for backwards compatibility)
impl VerletPoint {
    // Collision detection between two points
    pub fn check_collision(&self, other: &VerletPoint) -> bool {
        check_collision(self, other)
    }

    // Resolve collision between two points
    pub fn resolve_collision(&mut self, other: &mut VerletPoint) {
        resolve_collision(self, other)
    }
}

// Helper function to resolve collisions between all physics objects in a collection
pub fn resolve_all_collisions<T: PhysicsObject>(objects: &mut [T]) {
    for i in 0..objects.len() {
        for j in (i + 1)..objects.len() {
            if objects[i].has_collision() && objects[j].has_collision() {
                let (left, right) = objects.split_at_mut(j);
                resolve_collision(&mut left[i], &mut right[0]);
            }
        }
    }
}

// Specialized version for VerletPoints (backwards compatibility)
pub fn resolve_all_verlet_collisions(points: &mut [VerletPoint]) {
    resolve_all_collisions(points);
}
