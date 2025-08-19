#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Direction {
    Up,
    Down,
    Left,
    Right,
    None,
}

impl Direction {
    pub fn opposite(self) -> Direction {
        match self {
            Direction::Up => Direction::Down,
            Direction::Down => Direction::Up,
            Direction::Left => Direction::Right,
            Direction::Right => Direction::Left,
            Direction::None => Direction::None,
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum AnimationType {
    RightRun,
    LeftRun,
    Death,
    None,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum GhostType {
    Red = 0,
    Pink = 1,
    Blue = 2,
    Orange = 3,
}

impl GhostType {
    pub fn to_char(self) -> char {
        match self {
            GhostType::Red => '1',
            GhostType::Pink => '2',
            GhostType::Blue => '3',
            GhostType::Orange => '4',
        }
    }

    pub fn from_char(c: char) -> Option<GhostType> {
        match c {
            '1' => Some(GhostType::Red),
            '2' => Some(GhostType::Pink),
            '3' => Some(GhostType::Blue),
            '4' => Some(GhostType::Orange),
            _ => None,
        }
    }

    pub fn to_index(self) -> usize {
        self as usize
    }
}

// Cell types in the map
#[derive(Debug, Clone, Copy, PartialEq)]
pub enum CellType {
    Wall,             // 'w'
    Pill,             // '#'
    PowerPill,        // '"'
    Player,           // 'p'
    Ghost(GhostType), // '1', '2', '3', '4'
    Empty,            // 'e' or ' '
    Tunnel,           // For teleportation points
}

impl CellType {
    pub fn from_char(c: char) -> CellType {
        match c {
            'w' => CellType::Wall,
            '#' => CellType::Pill,
            '"' => CellType::PowerPill,
            'p' => CellType::Player,
            '1' => CellType::Ghost(GhostType::Red),
            '2' => CellType::Ghost(GhostType::Pink),
            '3' => CellType::Ghost(GhostType::Blue),
            '4' => CellType::Ghost(GhostType::Orange),
            'e' | ' ' => CellType::Empty,
            _ => CellType::Empty,
        }
    }

    pub fn to_char(self) -> char {
        match self {
            CellType::Wall => 'w',
            CellType::Pill => '#',
            CellType::PowerPill => '"',
            CellType::Player => 'p',
            CellType::Ghost(ghost_type) => ghost_type.to_char(),
            CellType::Empty => 'e',
            CellType::Tunnel => '#', // Tunnels act like pills
        }
    }
}
