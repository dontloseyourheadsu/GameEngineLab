#[derive(Clone, Debug)]
pub enum ComplexCollisionBody {
    Circle { radius: f32 },
    Box { width: f32, height: f32, angle: f32 },
}

impl From<&crate::physics::collisions_2d::simple_collision_body::SimpleCollisionBody>
    for ComplexCollisionBody
{
    fn from(
        simple: &crate::physics::collisions_2d::simple_collision_body::SimpleCollisionBody,
    ) -> Self {
        use crate::physics::collisions_2d::simple_collision_body::SimpleCollisionBody;
        match simple {
            SimpleCollisionBody::Circle { radius } => {
                ComplexCollisionBody::Circle { radius: *radius }
            }
            SimpleCollisionBody::Box { width, height } => ComplexCollisionBody::Box {
                width: *width,
                height: *height,
                angle: 0.0,
            },
        }
    }
}
