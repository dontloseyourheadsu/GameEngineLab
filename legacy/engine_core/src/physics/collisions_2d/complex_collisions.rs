use crate::physics::collisions_2d::complex_collision_body::ComplexCollisionBody;
use raylib::math::Vector2;

use super::{circle_box_collision::check_box_circle_collision, sat_collision::check_box_box_collision};

pub fn check_complex_collisions(
    bodies: &[(Vector2, &ComplexCollisionBody)],
) -> Vec<(usize, usize)> {
    let mut collisions = Vec::new();
    for i in 0..bodies.len() {
        for j in (i + 1)..bodies.len() {
            let (pos_a, body_a) = bodies[i];
            let (pos_b, body_b) = bodies[j];
            if check_complex_collision(pos_a, body_a, pos_b, body_b) {
                collisions.push((i, j));
            }
        }
    }
    collisions
}

pub fn check_complex_collision(
    pos_self: Vector2,
    body_self: &ComplexCollisionBody,
    pos_other: Vector2,
    body_other: &ComplexCollisionBody,
) -> bool {
    match (body_self, body_other) {
        (
            ComplexCollisionBody::Circle { radius: r1 },
            ComplexCollisionBody::Circle { radius: r2 },
        ) => {
            let dist_sq = (pos_self - pos_other).length_sqr();
            let r_sum = r1 + r2;
            dist_sq <= r_sum * r_sum
        }
        (
            ComplexCollisionBody::Box {
                width: w,
                height: h,
                angle: a,
            },
            ComplexCollisionBody::Circle { radius: r },
        ) => check_box_circle_collision(pos_self, *w, *h, *a, pos_other, *r),
        (
            ComplexCollisionBody::Circle { radius: r },
            ComplexCollisionBody::Box {
                width: w,
                height: h,
                angle: a,
            },
        ) => check_box_circle_collision(pos_other, *w, *h, *a, pos_self, *r),
        (
            ComplexCollisionBody::Box {
                width: w1,
                height: h1,
                angle: a1,
            },
            ComplexCollisionBody::Box {
                width: w2,
                height: h2,
                angle: a2,
            },
        ) => check_box_box_collision(pos_self, *w1, *h1, *a1, pos_other, *w2, *h2, *a2),
    }
}
