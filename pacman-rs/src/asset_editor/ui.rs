use raylib::prelude::*;

use super::models::AssetDef;

pub const PALETTE_COLORS: [Color; 9] = [
    Color::RED,
    Color::ORANGE,
    Color::YELLOW,
    Color::GREEN,
    Color::BLUE,
    Color::PURPLE,
    Color::VIOLET,
    Color::BLACK,
    Color::WHITE,
];

pub struct EditorLayout {
    pub base_width: i32,
    pub base_height: i32,
    pub canvas_rect: Rectangle,
    pub palette_x: f32,
    pub palette_y: f32,
    pub color_box_size: f32,
    pub box_spacing: f32,
    pub back_btn: Rectangle,
    pub ui_base_y: f32,
    pub frame_box_size: f32,
    pub frame_box_spacing: f32,
    pub arrow_size: f32,
    pub arrow_up_rec: Rectangle,
    pub arrow_down_rec: Rectangle,
    pub frames_start_x: f32,
    pub frames_y: f32,
}

impl EditorLayout {
    pub fn new(base_width: i32, base_height: i32) -> Self {
        let canvas_size = 256;
        let canvas_x = (base_width - canvas_size) / 2;
        let canvas_y = (base_height - canvas_size) / 2;
        let canvas_rect = Rectangle::new(
            canvas_x as f32,
            canvas_y as f32,
            canvas_size as f32,
            canvas_size as f32,
        );

        let palette_x = canvas_rect.x + canvas_rect.width + 20.0;
        let palette_y = canvas_rect.y;
        let color_box_size = 30.0;
        let box_spacing = 5.0;

        let back_btn = Rectangle::new(20.0, 20.0, 100.0, 40.0);
        let ui_base_y = canvas_rect.y + canvas_rect.height + 30.0;
        let frame_box_size = 50.0;
        let frame_box_spacing = 10.0;
        let arrow_size = 20.0;

        EditorLayout {
            base_width,
            base_height,
            canvas_rect,
            palette_x,
            palette_y,
            color_box_size,
            box_spacing,
            back_btn,
            ui_base_y,
            frame_box_size,
            frame_box_spacing,
            arrow_size,
            arrow_up_rec: Rectangle::new(0.0, 0.0, 0.0, 0.0), // Calculated dynamically
            arrow_down_rec: Rectangle::new(0.0, 0.0, 0.0, 0.0), // Calculated dynamically
            frames_start_x: 0.0,                              // Calculated dynamically
            frames_y: 0.0,                                    // Calculated dynamically
        }
    }

    pub fn update_dynamic_layout(&mut self, current_frames_count: usize) {
        let total_frames_width = current_frames_count as f32 * self.frame_box_size
            + (current_frames_count as f32 - 1.0).max(0.0) * self.frame_box_spacing;

        self.frames_start_x = (self.base_width as f32 - total_frames_width) / 2.0;
        self.frames_y = self.ui_base_y + 25.0;

        let arrow_x = self.frames_start_x - 40.0;
        self.arrow_up_rec =
            Rectangle::new(arrow_x, self.frames_y, self.arrow_size, self.arrow_size);
        self.arrow_down_rec = Rectangle::new(
            arrow_x,
            self.frames_y + self.arrow_size + 10.0,
            self.arrow_size,
            self.arrow_size,
        );
    }
}

pub fn draw_ui(
    d: &mut RaylibMode2D<RaylibDrawHandle>,
    layout: &EditorLayout,
    mouse_pos: Vector2,
    assets: &[AssetDef],
    asset_storage: &[Vec<RenderTexture2D>],
    current_asset_idx: usize,
    current_frame_idx: usize,
    selected_color: Color,
) {
    // Header
    d.draw_text(
        "Asset Editor",
        layout.base_width / 2 - 60,
        20,
        20,
        Color::DARKGRAY,
    );

    // Back Button
    let btn_color = if layout.back_btn.check_collision_point_rec(mouse_pos) {
        Color::SKYBLUE
    } else {
        Color::LIGHTGRAY
    };
    d.draw_rectangle_rec(layout.back_btn, btn_color);
    d.draw_rectangle_lines_ex(layout.back_btn, 2.0, Color::DARKGRAY);
    d.draw_text(
        "BACK",
        layout.back_btn.x as i32 + 25,
        layout.back_btn.y as i32 + 10,
        20,
        Color::BLACK,
    );

    // Palette
    for (i, &color) in PALETTE_COLORS.iter().enumerate() {
        let box_rec = Rectangle::new(
            layout.palette_x,
            layout.palette_y + (i as f32 * (layout.color_box_size + layout.box_spacing)),
            layout.color_box_size,
            layout.color_box_size,
        );

        d.draw_rectangle_rec(box_rec, color);
        d.draw_rectangle_lines_ex(box_rec, 1.0, Color::BLACK);

        // Highlight selected
        let is_selected = color.r == selected_color.r
            && color.g == selected_color.g
            && color.b == selected_color.b
            && color.a == selected_color.a;

        if is_selected {
            d.draw_rectangle_lines_ex(
                Rectangle::new(
                    box_rec.x - 3.0,
                    box_rec.y - 3.0,
                    box_rec.width + 6.0,
                    box_rec.height + 6.0,
                ),
                3.0,
                Color::GOLD,
            );
        }
    }

    // Selected Color Indicator
    let indicator_y = layout.palette_y
        + (PALETTE_COLORS.len() as f32 * (layout.color_box_size + layout.box_spacing));
    d.draw_text(
        "Color:",
        layout.palette_x as i32,
        (indicator_y + 10.0) as i32,
        10,
        Color::BLACK,
    );
    d.draw_rectangle(
        layout.palette_x as i32,
        (indicator_y + 25.0) as i32,
        layout.color_box_size as i32,
        layout.color_box_size as i32,
        selected_color,
    );
    d.draw_rectangle_lines(
        layout.palette_x as i32,
        (indicator_y + 25.0) as i32,
        layout.color_box_size as i32,
        layout.color_box_size as i32,
        Color::BLACK,
    );

    // Asset Selection UI
    let asset_name = assets[current_asset_idx].name;
    let name_width = d.measure_text(asset_name, 20);
    d.draw_text(
        asset_name,
        (layout.base_width / 2) - (name_width / 2),
        layout.ui_base_y as i32,
        20,
        Color::BLACK,
    );

    // Arrows
    d.draw_rectangle_rec(layout.arrow_up_rec, Color::LIGHTGRAY);
    d.draw_rectangle_lines_ex(layout.arrow_up_rec, 1.0, Color::DARKGRAY);
    d.draw_triangle(
        Vector2::new(
            layout.arrow_up_rec.x + layout.arrow_size / 2.0,
            layout.arrow_up_rec.y + 3.0,
        ),
        Vector2::new(
            layout.arrow_up_rec.x + 3.0,
            layout.arrow_up_rec.y + layout.arrow_size - 3.0,
        ),
        Vector2::new(
            layout.arrow_up_rec.x + layout.arrow_size - 3.0,
            layout.arrow_up_rec.y + layout.arrow_size - 3.0,
        ),
        Color::BLACK,
    );

    d.draw_rectangle_rec(layout.arrow_down_rec, Color::LIGHTGRAY);
    d.draw_rectangle_lines_ex(layout.arrow_down_rec, 1.0, Color::DARKGRAY);
    d.draw_triangle(
        Vector2::new(layout.arrow_down_rec.x + 3.0, layout.arrow_down_rec.y + 3.0),
        Vector2::new(
            layout.arrow_down_rec.x + layout.arrow_size / 2.0,
            layout.arrow_down_rec.y + layout.arrow_size - 3.0,
        ),
        Vector2::new(
            layout.arrow_down_rec.x + layout.arrow_size - 3.0,
            layout.arrow_down_rec.y + 3.0,
        ),
        Color::BLACK,
    );

    // Frames
    for i in 0..assets[current_asset_idx].frames {
        let rec = Rectangle::new(
            layout.frames_start_x + i as f32 * (layout.frame_box_size + layout.frame_box_spacing),
            layout.frames_y,
            layout.frame_box_size,
            layout.frame_box_size,
        );

        d.draw_rectangle_rec(rec, Color::RAYWHITE);

        // Thumbnail
        let thumb_tex = asset_storage[current_asset_idx][i].texture();
        let thumb_src = Rectangle::new(
            0.0,
            0.0,
            thumb_tex.width() as f32,
            -thumb_tex.height() as f32,
        );
        d.draw_texture_pro(
            thumb_tex,
            thumb_src,
            rec,
            Vector2::zero(),
            0.0,
            Color::WHITE,
        );

        // Border
        if i == current_frame_idx {
            d.draw_rectangle_lines_ex(rec, 3.0, Color::BLUE);
        } else {
            d.draw_rectangle_lines_ex(rec, 1.0, Color::BLACK);
        }

        // Frame Number
        let num_str = format!("{}", i + 1);
        let num_width = d.measure_text(&num_str, 10);
        d.draw_text(
            &num_str,
            (rec.x + rec.width / 2.0) as i32 - num_width / 2,
            (rec.y + rec.height + 2.0) as i32,
            10,
            Color::BLACK,
        );
    }
}
