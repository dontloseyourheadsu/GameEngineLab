use crate::physics::collisions_2d::simple_collision_body::SimpleCollisionBody;
use raylib::math::Vector2;

pub fn check_simple_collisions(bodies: &[(Vector2, &SimpleCollisionBody)]) -> Vec<(usize, usize)> {
    let mut collisions = Vec::new();
    for i in 0..bodies.len() {
        for j in (i + 1)..bodies.len() {
            let (pos_a, body_a) = bodies[i];
            let (pos_b, body_b) = bodies[j];
            if check_simple_collision(pos_a, body_a, pos_b, body_b) {
                collisions.push((i, j));
            }
        }
    }
    collisions
}

pub fn check_simple_collision(
    pos_a: Vector2,
    body_a: &SimpleCollisionBody,
    pos_b: Vector2,
    body_b: &SimpleCollisionBody,
) -> bool {
    match (body_a, body_b) {
        (
            SimpleCollisionBody::Circle { radius: r1 },
            SimpleCollisionBody::Circle { radius: r2 },
        ) => {
            let dist_sq = (pos_a - pos_b).length_sqr();
            let r_sum = r1 + r2;
            dist_sq <= r_sum * r_sum
        }
        (
            SimpleCollisionBody::Box {
                width: w1,
                height: h1,
            },
            SimpleCollisionBody::Box {
                width: w2,
                height: h2,
            },
        ) => {
            let a_half_w = w1 / 2.0;
            let a_half_h = h1 / 2.0;
            let b_half_w = w2 / 2.0;
            let b_half_h = h2 / 2.0;

            let a_min_x = pos_a.x - a_half_w;
            let a_max_x = pos_a.x + a_half_w;
            let a_min_y = pos_a.y - a_half_h;
            let a_max_y = pos_a.y + a_half_h;

            let b_min_x = pos_b.x - b_half_w;
            let b_max_x = pos_b.x + b_half_w;
            let b_min_y = pos_b.y - b_half_h;
            let b_max_y = pos_b.y + b_half_h;

            a_min_x < b_max_x && a_max_x > b_min_x && a_min_y < b_max_y && a_max_y > b_min_y
        }
        (SimpleCollisionBody::Box { width, height }, SimpleCollisionBody::Circle { radius }) => {
            check_box_circle(pos_a, *width, *height, pos_b, *radius)
        }
        (SimpleCollisionBody::Circle { radius }, SimpleCollisionBody::Box { width, height }) => {
            check_box_circle(pos_b, *width, *height, pos_a, *radius)
        }
    }
}

fn check_box_circle(
    box_pos: Vector2,
    box_w: f32,
    box_h: f32,
    circle_pos: Vector2,
    radius: f32,
) -> bool {
    // Transform circle center to box local space (simple AABB, so just subtract center)
    // Box center is at box_pos

    let half_w = box_w / 2.0;
    let half_h = box_h / 2.0;

    let rel_x = circle_pos.x - box_pos.x;
    let rel_y = circle_pos.y - box_pos.y;

    // Find closest point on AABB to circle center
    let closest_x = rel_x.clamp(-half_w, half_w);
    let closest_y = rel_y.clamp(-half_h, half_h);

    let distance_x = rel_x - closest_x;
    let distance_y = rel_y - closest_y;

    (distance_x * distance_x + distance_y * distance_y) <= (radius * radius)
}
