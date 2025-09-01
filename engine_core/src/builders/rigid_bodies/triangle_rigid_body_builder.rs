use crate::rigid_bodies::solid_body_build::SolidBodyBuild;
use rapier2d::na::{Point2, Vector2};
pub use rapier2d::prelude::*;

#[derive(Clone)]
pub struct TriangleRigidBodyBuilder {
    pub(crate) body: RigidBody,
    pub(crate) collider: Collider,
}

impl SolidBodyBuild for TriangleRigidBodyBuilder {
    fn body(&self) -> &RigidBody {
        &self.body
    }

    fn collider(&self) -> &Collider {
        &self.collider
    }
}

impl TriangleRigidBodyBuilder {
    pub fn new(
        body_type: RigidBodyType,
        position: Vector2<f32>,
        point1: Vector2<f32>,
        point2: Vector2<f32>,
        point3: Vector2<f32>,
    ) -> Self {
        // Calculate the centroid of the triangle to position the body
        let centroid = Vector2::new(
            (point1.x + point2.x + point3.x) / 3.0,
            (point1.y + point2.y + point3.y) / 3.0,
        );

        let solid_body = RigidBodyBuilder::new(body_type)
            .translation(position)
            .build();

        // Create triangle vertices relative to the centroid (local coordinates)
        let p1 = Point2::new(point1.x - centroid.x, point1.y - centroid.y);
        let p2 = Point2::new(point2.x - centroid.x, point2.y - centroid.y);
        let p3 = Point2::new(point3.x - centroid.x, point3.y - centroid.y);

        let collider = ColliderBuilder::triangle(p1, p2, p3)
            .friction(0.7) // Default friction coefficient
            .build();

        TriangleRigidBodyBuilder {
            body: solid_body,
            collider: collider,
        }
    }

    pub fn with_rotation(mut self, rotation: f32) -> Self {
        self.body = RigidBodyBuilder::new(self.body.body_type())
            .translation(self.body.position().translation.vector)
            .rotation(rotation)
            .build();
        self
    }

    pub fn with_friction(mut self, friction: f32) -> Self {
        // Rebuild the collider with new friction, preserving the triangle shape
        if let Some(triangle) = self.collider.shape().as_triangle() {
            let vertices = triangle.vertices();
            let p1 = vertices[0];
            let p2 = vertices[1];
            let p3 = vertices[2];
            self.collider = ColliderBuilder::triangle(p1, p2, p3)
                .friction(friction)
                .build();
        }
        self
    }
}
