use serde::Deserialize;

#[derive(Deserialize)]
pub struct TileSize {
    pub width: usize,
    pub height: usize,
}