use image::{ImageBuffer, Rgb, RgbImage};

pub fn apply_clay_filter(img: &RgbImage) -> RgbImage {
    let width = img.width();
    let height = img.height();
    let mut output = ImageBuffer::new(width, height);

    for y in 1..height - 1 {
        for x in 1..width - 1 {
            let smooth = bilateral_filter(img, x as i32, y as i32, 3, 3.0, 30.0);

            let h_l = luminance(bilateral_filter(img, x as i32 - 1, y as i32, 3, 3.0, 30.0));
            let h_r = luminance(bilateral_filter(img, x as i32 + 1, y as i32, 3, 3.0, 30.0));
            let h_u = luminance(bilateral_filter(img, x as i32, y as i32 - 1, 3, 3.0, 30.0));
            let h_d = luminance(bilateral_filter(img, x as i32, y as i32 + 1, 3, 3.0, 30.0));

            let normal = normal_from_height(h_l, h_r, h_u, h_d, 3.0);
            let lit = light_pixel(smooth, normal);

            let out = Rgb([
                add_noise(lit[0]) as u8,
                add_noise(lit[1]) as u8,
                add_noise(lit[2]) as u8,
            ]);

            output.put_pixel(x, y, out);
        }
    }
    
    // Fill borders with original or black to avoid uninitialized pixels
    // For simplicity, we just leave them black or copy original? 
    // The loop skips 0 and width-1. Let's just leave them as is (black/transparent if initialized to 0)
    // or copy from source. Copying from source is safer visually.
    for x in 0..width {
        output.put_pixel(x, 0, *img.get_pixel(x, 0));
        output.put_pixel(x, height - 1, *img.get_pixel(x, height - 1));
    }
    for y in 0..height {
        output.put_pixel(0, y, *img.get_pixel(0, y));
        output.put_pixel(width - 1, y, *img.get_pixel(width - 1, y));
    }

    output
}

fn bilateral_filter(
    img: &RgbImage,
    x: i32,
    y: i32,
    radius: i32,
    sigma_s: f32,
    sigma_c: f32,
) -> [f32; 3] {
    let mut sum = [0.0; 3];
    let mut wsum = 0.0;

    let center = img.get_pixel(x as u32, y as u32);
    let center_rgb = [
        center[0] as f32,
        center[1] as f32,
        center[2] as f32,
    ];

    for dy in -radius..=radius {
        for dx in -radius..=radius {
            let nx = x + dx;
            let ny = y + dy;
            if nx < 0 || ny < 0 || nx >= img.width() as i32 || ny >= img.height() as i32 {
                continue;
            }

            let p = img.get_pixel(nx as u32, ny as u32);
            let rgb = [p[0] as f32, p[1] as f32, p[2] as f32];

            let ds = (dx * dx + dy * dy) as f32;
            let dc = (rgb[0] - center_rgb[0]).powi(2)
                   + (rgb[1] - center_rgb[1]).powi(2)
                   + (rgb[2] - center_rgb[2]).powi(2);

            let ws = (-ds / (2.0 * sigma_s * sigma_s)).exp();
            let wc = (-dc / (2.0 * sigma_c * sigma_c)).exp();
            let w = ws * wc;

            for c in 0..3 {
                sum[c] += rgb[c] * w;
            }
            wsum += w;
        }
    }

    [sum[0] / wsum, sum[1] / wsum, sum[2] / wsum]
}

fn luminance(rgb: [f32; 3]) -> f32 {
    0.299 * rgb[0] + 0.587 * rgb[1] + 0.114 * rgb[2]
}

fn normal_from_height(
    h_l: f32,
    h_r: f32,
    h_u: f32,
    h_d: f32,
    strength: f32,
) -> [f32; 3] {
    let dx = (h_r - h_l) * strength;
    let dy = (h_d - h_u) * strength;

    let mut nx = -dx;
    let mut ny = -dy;
    let mut nz = 1.0;

    let len = (nx*nx + ny*ny + nz*nz).sqrt();
    nx /= len;
    ny /= len;
    nz /= len;

    [nx, ny, nz]
}

fn light_pixel(
    color: [f32; 3],
    normal: [f32; 3],
) -> [f32; 3] {
    let light_dir = {
        let v: [f32; 3] = [0.4, -0.6, 0.7];
        let l = (v[0]*v[0] + v[1]*v[1] + v[2]*v[2]).sqrt();
        [v[0]/l, v[1]/l, v[2]/l]
    };

    let diffuse = (normal[0]*light_dir[0]
        + normal[1]*light_dir[1]
        + normal[2]*light_dir[2])
        .max(0.0);

    let ambient = 0.4;
    let light = ambient + 0.6 * diffuse;

    [
        color[0] * light,
        color[1] * light,
        color[2] * light,
    ]
}

fn add_noise(c: f32) -> f32 {
    let n: f32 = rand::random::<f32>() - 0.5;
    (c + n * 3.0).clamp(0.0, 255.0)
}
