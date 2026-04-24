use image::{ImageBuffer, Rgba, RgbaImage, Pixel};

pub fn apply_grayscale_filter(img: &RgbaImage) -> RgbaImage {
    let width = img.width();
    let height = img.height();
    let mut output = ImageBuffer::new(width, height);

    for y in 0..height {
        for x in 0..width {
            let pixel = img.get_pixel(x, y);
            if pixel[3] == 0 {
                output.put_pixel(x, y, *pixel);
                continue;
            }
            let luminance = (0.299 * pixel[0] as f32 + 0.587 * pixel[1] as f32 + 0.114 * pixel[2] as f32) as u8;
            output.put_pixel(x, y, Rgba([luminance, luminance, luminance, pixel[3]]));
        }
    }

    output
}
