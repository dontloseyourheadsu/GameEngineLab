use super::tileset::Tileset;
use super::utilities::char_map_to_tile_ids;
use raylib::prelude::*;

/// This is the core data structure representing the level layout.
/// It is a 2D grid that stores the integer IDs of the tiles.
/// The `tileset` field links this grid to the visual data.
pub struct Tilemap {
    /// The width and height of the map, in tiles.
    pub width: usize,
    pub height: usize,

    /// The 2D grid storing the integer IDs of the tiles.
    /// Vec<Vec<u32>> is a common and easy-to-use representation.
    pub grid: Vec<Vec<u32>>,

    /// A reference to the tileset that this tilemap will use for rendering.
    /// This connects the IDs in the grid to the actual visual data.
    pub tileset: Tileset,
}

impl Tilemap {
    /// A constructor for a new Tilemap.
    pub fn new(width: usize, height: usize, tileset: Tileset) -> Self {
        // Initialize the grid with a default tile ID (e.g., 0 for an empty tile).
        let grid = vec![vec![0; width]; height];
        Tilemap {
            width,
            height,
            grid,
            tileset,
        }
    }

    /// Creates a tilemap from a 2D character array with custom conversion function
    pub fn from_char_array_with_converter(
        char_array: &[&[char]],
        tileset: Tileset,
        char_to_id_fn: fn(char) -> u32,
    ) -> Self {
        let height = char_array.len();
        let width = if height > 0 { char_array[0].len() } else { 0 };

        let grid = char_map_to_tile_ids(char_array, char_to_id_fn);

        Tilemap {
            width,
            height,
            grid,
            tileset,
        }
    }

    /// A method to get a tile's ID at a specific grid coordinate.
    pub fn get_tile_id(&self, x: usize, y: usize) -> Option<u32> {
        self.grid.get(y)?.get(x).copied()
    }

    /// A method to set a tile's ID at a specific grid coordinate.
    pub fn set_tile_id(&mut self, x: usize, y: usize, id: u32) {
        if x < self.width && y < self.height {
            self.grid[y][x] = id;
        }
    }

    /// The draw function that renders the tilemap.
    pub fn draw(&self, d: &mut RaylibDrawHandle, tile_size: f32) {
        // We iterate through the grid row by row (y-axis).
        for y in 0..self.height {
            // Then, for each row, we iterate through the columns (x-axis).
            for x in 0..self.width {
                // Get the tile ID at the current grid position.
                if let Some(tile_id) = self.get_tile_id(x, y) {
                    // Look up the actual Tile data from the Tileset using the ID.
                    if let Some(tile) = self.tileset.get_tile(tile_id) {
                        // Define the destination position on the screen where the tile will be drawn.
                        let dest_x = (x as f32) * tile_size;
                        let dest_y = (y as f32) * tile_size;

                        // Draw the tile's texture.
                        // We use `draw_texture_rec` to draw only a specific portion (`source_rect`)
                        // of the entire tileset texture onto the screen.
                        d.draw_texture_rec(
                            &self.tileset.texture,
                            tile.source_rect,
                            rvec2(dest_x, dest_y),
                            Color::WHITE,
                        );
                    }
                }
            }
        }
    }

    /// Draw the tilemap with an offset
    pub fn draw_with_offset(
        &self,
        d: &mut RaylibDrawHandle,
        tile_size: f32,
        offset_x: f32,
        offset_y: f32,
    ) {
        for y in 0..self.height {
            for x in 0..self.width {
                if let Some(tile_id) = self.get_tile_id(x, y) {
                    if let Some(tile) = self.tileset.get_tile(tile_id) {
                        let dest_x = (x as f32) * tile_size + offset_x;
                        let dest_y = (y as f32) * tile_size + offset_y;

                        d.draw_texture_rec(
                            &self.tileset.texture,
                            tile.source_rect,
                            rvec2(dest_x, dest_y),
                            Color::WHITE,
                        );
                    }
                }
            }
        }
    }
}
