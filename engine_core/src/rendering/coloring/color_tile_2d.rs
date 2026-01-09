use raylib::prelude::Color;

#[derive(Debug, Clone, Copy)]
pub struct ColorTile2D {
    pub color: Color,
}

impl ColorTile2D {
    pub fn from_string(value: &str) -> Option<Self> {
        let value = value.trim();
        if value.starts_with('#') {
            return Self::from_hex(value);
        } else if value.starts_with("rgb") {
            return Self::from_rgb_usage(value);
        } else if value.starts_with("hsv") {
            return Self::from_hsv_usage(value);
        }
        None
    }

    fn from_hex(hex: &str) -> Option<Self> {
        let hex = hex.trim_start_matches('#');
        if hex.len() == 6 {
            let r = u8::from_str_radix(&hex[0..2], 16).ok()?;
            let g = u8::from_str_radix(&hex[2..4], 16).ok()?;
            let b = u8::from_str_radix(&hex[4..6], 16).ok()?;
            Some(Self {
                color: Color::new(r, g, b, 255),
            })
        } else if hex.len() == 8 {
            let r = u8::from_str_radix(&hex[0..2], 16).ok()?;
            let g = u8::from_str_radix(&hex[2..4], 16).ok()?;
            let b = u8::from_str_radix(&hex[4..6], 16).ok()?;
            let a = u8::from_str_radix(&hex[6..8], 16).ok()?;
            Some(Self {
                color: Color::new(r, g, b, a),
            })
        } else {
            None
        }
    }

    fn from_rgb_usage(value: &str) -> Option<Self> {
        // expected format: rgb(r, g, b) or rgba(r, g, b, a)
        let custom_val = value
            .trim_start_matches("rgba")
            .trim_start_matches("rgb")
            .trim_matches(|c| c == '(' || c == ')');

        let parts: Vec<&str> = custom_val.split(',').collect();
        if parts.len() == 3 {
            let r = parts[0].trim().parse::<u8>().ok()?;
            let g = parts[1].trim().parse::<u8>().ok()?;
            let b = parts[2].trim().parse::<u8>().ok()?;
            Some(Self {
                color: Color::new(r, g, b, 255),
            })
        } else if parts.len() == 4 {
            let r = parts[0].trim().parse::<u8>().ok()?;
            let g = parts[1].trim().parse::<u8>().ok()?;
            let b = parts[2].trim().parse::<u8>().ok()?;
            let a_str = parts[3].trim();
            let a = if let Ok(val) = a_str.parse::<u8>() {
                val
            } else if let Ok(val) = a_str.parse::<f32>() {
                (val.clamp(0.0, 1.0) * 255.0) as u8
            } else {
                return None;
            };
            Some(Self {
                color: Color::new(r, g, b, a),
            })
        } else {
            None
        }
    }

    fn from_hsv_usage(value: &str) -> Option<Self> {
        // expected format: hsv(h, s, v)
        // raylib Color::color_from_hsv takes (hue, saturation, value) as f32
        // hue [0..360], saturation [0..1], value [0..1]

        let custom_val = value
            .trim_start_matches("hsv")
            .trim_matches(|c| c == '(' || c == ')');

        let parts: Vec<&str> = custom_val.split(',').collect();
        if parts.len() == 3 {
            let h = parts[0].trim().parse::<f32>().ok()?;
            let s = parts[1].trim().parse::<f32>().ok()?;
            let v = parts[2].trim().parse::<f32>().ok()?;
            Some(Self {
                color: Color::color_from_hsv(h, s, v),
            })
        } else {
            None
        }
    }
}
