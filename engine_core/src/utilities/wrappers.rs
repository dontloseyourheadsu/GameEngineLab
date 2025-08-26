use raylib::ffi;
use raylib::prelude::*;
use std::ffi::CString;

/// Safely measures text with extended font parameters
/// This wraps the raylib FFI function for text measurement
pub fn measure_text_ex_safe(font: &Font, text: &str, font_size: f32, spacing: f32) -> Vector2 {
    let c_text = CString::new(text).unwrap();
    unsafe {
        let ffi_vector2 = ffi::MeasureTextEx(**font, c_text.as_ptr(), font_size, spacing);
        Vector2::new(ffi_vector2.x, ffi_vector2.y)
    }
}
