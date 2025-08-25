use raylib::prelude::*;

#[derive(Debug)]
pub struct RenderMesh {
    pub texture_path: Option<String>, // Store path instead of texture
    pub visible: bool,
    pub scale: Vector2,
    pub rotation: f32,
    pub tint: Color,
}

impl RenderMesh {
    pub fn new() -> Self {
        RenderMesh {
            texture_path: None,
            visible: true,
            scale: Vector2::new(1.0, 1.0),
            rotation: 0.0,
            tint: Color::WHITE,
        }
    }

    pub fn new_with_texture_path(texture_path: String) -> Self {
        RenderMesh {
            texture_path: Some(texture_path),
            visible: true,
            scale: Vector2::new(1.0, 1.0),
            rotation: 0.0,
            tint: Color::WHITE,
        }
    }

    pub fn new_hidden() -> Self {
        RenderMesh {
            texture_path: None,
            visible: false,
            scale: Vector2::new(1.0, 1.0),
            rotation: 0.0,
            tint: Color::WHITE,
        }
    }

    pub fn set_texture_path(&mut self, texture_path: Option<String>) {
        self.texture_path = texture_path;
    }

    pub fn set_visible(&mut self, visible: bool) {
        self.visible = visible;
    }

    pub fn set_scale(&mut self, scale: Vector2) {
        self.scale = scale;
    }

    pub fn set_rotation(&mut self, rotation: f32) {
        self.rotation = rotation;
    }

    pub fn set_tint(&mut self, tint: Color) {
        self.tint = tint;
    }

    pub fn hide(&mut self) {
        self.visible = false;
    }

    pub fn show(&mut self) {
        self.visible = true;
    }

    pub fn has_texture(&self) -> bool {
        self.texture_path.is_some()
    }
}
