#[derive(Debug, Clone, Copy, PartialEq)]
pub enum Collision {
    None,
    Top,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}
