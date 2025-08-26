use raylib::prelude::*;
use std::collections::HashMap;
use super::tile::Tile;

/// This is the "catalog" or "atlas". It holds the single texture
/// containing all the tile images and a map to quickly find a Tile by its ID.
/// The `HashMap` provides an efficient way to look up tile data.
pub struct Tileset {
    /// The single texture containing all the tile graphics.
    /// In raylib-rs, this is a Texture2D.
    pub texture: Texture2D,

    /// A hash map to link a tile's ID to its data (source_rect, etc.).
    pub tiles: HashMap<u32, Tile>,
}

impl Tileset {
    /// A constructor function to create a new Tileset.
    pub fn new(texture: Texture2D) -> Self {
        Tileset {
            texture,
            tiles: HashMap::new(),
        }
    }

    /// A method to add a new tile to the tileset.
    pub fn add_tile(&mut self, id: u32, source_rect: Rectangle, is_solid: bool) {
        let tile = Tile::new(id, source_rect, is_solid);
        self.tiles.insert(id, tile);
    }

    /// Get a tile by its ID
    pub fn get_tile(&self, id: u32) -> Option<&Tile> {
        self.tiles.get(&id)
    }
}
