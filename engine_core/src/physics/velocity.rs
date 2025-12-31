use raylib::math::Vector2;

pub struct Velocity(pub Vector2);

impl Default for Velocity {
    fn default() -> Self {
        Self(Vector2::zero())
    }
}
