use crate::rendering::coloring::color_tile_2d::ColorTile2D;

#[derive(Debug)]
pub enum TextureSource {
    File(String),
    Color(ColorTile2D),
}