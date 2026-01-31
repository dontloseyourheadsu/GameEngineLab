use raylib::math::Vector2;

pub fn check_box_circle_collision(
    box_pos: Vector2,
    box_w: f32,
    box_h: f32,
    box_angle: f32,
    circle_pos: Vector2,
    radius: f32,
) -> bool {
    // Transform circle center to box local space
    let angle_rad = -box_angle.to_radians();
    let local_circle_pos = rotate_point(circle_pos - box_pos, angle_rad);

    // Find closest point on box to circle center
    let half_w = box_w / 2.0;
    let half_h = box_h / 2.0;

    let closest = Vector2::new(
        local_circle_pos.x.clamp(-half_w, half_w),
        local_circle_pos.y.clamp(-half_h, half_h),
    );

    let distance = local_circle_pos.distance_to(closest);
    distance <= radius
}

fn rotate_point(point: Vector2, angle_rad: f32) -> Vector2 {
    let cos_a = angle_rad.cos();
    let sin_a = angle_rad.sin();
    Vector2::new(
        point.x * cos_a - point.y * sin_a,
        point.x * sin_a + point.y * cos_a,
    )
}
