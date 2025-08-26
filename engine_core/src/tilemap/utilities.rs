/// Converts a character-based map to a tile ID grid using a custom conversion function
/// This is a utility function specifically for tilemap creation from character arrays
pub fn char_map_to_tile_ids(
    char_array: &[&[char]],
    char_to_id_fn: fn(char) -> u32,
) -> Vec<Vec<u32>> {
    char_array
        .iter()
        .map(|row| row.iter().map(|&ch| char_to_id_fn(ch)).collect())
        .collect()
}
