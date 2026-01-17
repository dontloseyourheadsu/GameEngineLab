#[derive(Clone)]
pub struct TileDef {
    pub name: &'static str,
    pub symbol: char,
}

pub struct MapGroupSummary {
    pub name: String,
    pub width: usize,
    pub height: usize,
    pub is_done: bool,
}

pub const MAX_MAP_SIZE: usize = 50;

pub const WALL_CHAR: char = '#';
pub const FOOD_CHAR: char = '.';
pub const PILL_CHAR: char = 'o';
pub const GHOST_CHAR: char = 'S';
pub const PACMAN_CHAR: char = 'P';
pub const EMPTY_CHAR: char = ' ';

pub fn get_tile_definitions() -> Vec<TileDef> {
    vec![
        TileDef {
            name: "Wall",
            symbol: WALL_CHAR,
        },
        TileDef {
            name: "Food",
            symbol: FOOD_CHAR,
        },
        TileDef {
            name: "Pill",
            symbol: PILL_CHAR,
        },
        TileDef {
            name: "Ghost",
            symbol: GHOST_CHAR,
        },
        TileDef {
            name: "Pacman",
            symbol: PACMAN_CHAR,
        },
        TileDef {
            name: "Empty",
            symbol: EMPTY_CHAR,
        },
    ]
}
