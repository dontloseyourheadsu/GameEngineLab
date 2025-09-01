use rapier2d::prelude::{Collider, RigidBody};

pub trait SolidBodyBuild {
    fn body(&self) -> &RigidBody;
    fn collider(&self) -> &Collider;
}
