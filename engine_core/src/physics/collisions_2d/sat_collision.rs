use raylib::math::Vector2;

pub fn check_box_box_collision(
    pos1: Vector2,
    w1: f32,
    h1: f32,
    a1: f32,
    pos2: Vector2,
    w2: f32,
    h2: f32,
    a2: f32,
) -> bool {
    let rect1 = RotatedRect::new(pos1, w1, h1, a1);
    let rect2 = RotatedRect::new(pos2, w2, h2, a2);

    // SAT: Check axes from both rectangles
    let axes = [rect1.axis_x, rect1.axis_y, rect2.axis_x, rect2.axis_y];

    for axis in axes.iter() {
        if !overlap_on_axis(axis, &rect1, &rect2) {
            return false;
        }
    }

    true
}

struct RotatedRect {
    corners: [Vector2; 4],
    axis_x: Vector2,
    axis_y: Vector2,
}

impl RotatedRect {
    fn new(pos: Vector2, w: f32, h: f32, angle: f32) -> Self {
        let angle_rad = angle.to_radians();
        let cos_a = angle_rad.cos();
        let sin_a = angle_rad.sin();

        let axis_x = Vector2::new(cos_a, sin_a);
        let axis_y = Vector2::new(-sin_a, cos_a);

        // Half dimensions
        let hw = w / 2.0;
        let hh = h / 2.0;

        // corners relative to center (unrotated)
        let corners_local = [
            Vector2::new(hw, hh),
            Vector2::new(-hw, hh),
            Vector2::new(-hw, -hh),
            Vector2::new(hw, -hh),
        ];

        let mut corners = [Vector2::zero(); 4];
        for (i, corner) in corners_local.iter().enumerate() {
            let rotated = Vector2::new(
                corner.x * cos_a - corner.y * sin_a,
                corner.x * sin_a + corner.y * cos_a,
            );
            corners[i] = pos + rotated;
        }

        RotatedRect {
            corners,
            axis_x,
            axis_y,
        }
    }
}

fn overlap_on_axis(axis: &Vector2, rect1: &RotatedRect, rect2: &RotatedRect) -> bool {
    let (min1, max1) = project_rect(axis, rect1);
    let (min2, max2) = project_rect(axis, rect2);

    !(max1 < min2 || max2 < min1)
}

fn project_rect(axis: &Vector2, rect: &RotatedRect) -> (f32, f32) {
    let mut min = axis.dot(rect.corners[0]);
    let mut max = min;

    for i in 1..4 {
        let p = axis.dot(rect.corners[i]);
        if p < min {
            min = p;
        }
        if p > max {
            max = p;
        }
    }

    (min, max)
}
