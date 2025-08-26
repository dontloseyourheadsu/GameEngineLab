use raylib::prelude::*;

/// Represents a single type of tile, storing its visual data (a texture rectangle)
/// and an ID. The ID will be used as a lookup key in the Tilemap grid.
#[derive(Debug, Clone, Copy, PartialEq)]
pub struct Tile {
    /// The unique identifier for this tile type.
    /// This is the value that will be stored in the tilemap grid.
    pub id: u32,

    /// The source rectangle within the tileset texture.
    /// This defines what part of the atlas to draw.
    pub source_rect: Rectangle,

    /// Add other properties here, such as collision flags,
    /// or a flag for whether the tile is animated.
    pub is_solid: bool,
}

impl Tile {
    /// Creates a new tile with the given properties
    pub fn new(id: u32, source_rect: Rectangle, is_solid: bool) -> Self {
        Self {
            id,
            source_rect,
            is_solid,
        }
    }
}
