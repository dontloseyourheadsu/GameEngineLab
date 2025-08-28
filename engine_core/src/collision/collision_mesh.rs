use super::circle_collision_mesh::CircleCollisionMesh;
use super::rectangle_collision_mesh::RectangleCollisionMesh;

#[derive(Debug, Clone, Copy)]
pub enum CollisionMesh {
    Circle(CircleCollisionMesh),
    Rectangle(RectangleCollisionMesh),
}

impl CollisionMesh {
    // Convenience constructors
    pub fn new_circle(radius: f32) -> Self {
        CollisionMesh::Circle(CircleCollisionMesh::new(radius))
    }

    pub fn new_circle_with_restitution(radius: f32, restitution: f32) -> Self {
        CollisionMesh::Circle(CircleCollisionMesh::new_with_restitution(
            radius,
            restitution,
        ))
    }

    pub fn new_rectangle(width: f32, height: f32) -> Self {
        CollisionMesh::Rectangle(RectangleCollisionMesh::new(width, height))
    }

    pub fn new_rectangle_with_restitution(width: f32, height: f32, restitution: f32) -> Self {
        CollisionMesh::Rectangle(RectangleCollisionMesh::new_with_restitution(
            width,
            height,
            restitution,
        ))
    }

    pub fn new_rectangle_rotatable(width: f32, height: f32, can_rotate: bool) -> Self {
        CollisionMesh::Rectangle(RectangleCollisionMesh::new_rotatable(
            width, height, can_rotate,
        ))
    }

    pub fn new_rectangle_full(
        width: f32,
        height: f32,
        rotation: f32,
        restitution: f32,
        can_rotate: bool,
    ) -> Self {
        CollisionMesh::Rectangle(RectangleCollisionMesh::new_full(
            width,
            height,
            rotation,
            restitution,
            can_rotate,
        ))
    }

    // Common methods
    pub fn is_enabled(&self) -> bool {
        match self {
            CollisionMesh::Circle(mesh) => mesh.enabled,
            CollisionMesh::Rectangle(mesh) => mesh.enabled,
        }
    }

    pub fn enable(&mut self) {
        match self {
            CollisionMesh::Circle(mesh) => mesh.enable(),
            CollisionMesh::Rectangle(mesh) => mesh.enable(),
        }
    }

    pub fn disable(&mut self) {
        match self {
            CollisionMesh::Circle(mesh) => mesh.disable(),
            CollisionMesh::Rectangle(mesh) => mesh.disable(),
        }
    }

    pub fn get_restitution(&self) -> f32 {
        match self {
            CollisionMesh::Circle(mesh) => mesh.restitution,
            CollisionMesh::Rectangle(mesh) => mesh.restitution,
        }
    }

    // Pattern matching helpers
    pub fn as_circle(&self) -> Option<&CircleCollisionMesh> {
        match self {
            CollisionMesh::Circle(mesh) => Some(mesh),
            _ => None,
        }
    }

    pub fn as_circle_mut(&mut self) -> Option<&mut CircleCollisionMesh> {
        match self {
            CollisionMesh::Circle(mesh) => Some(mesh),
            _ => None,
        }
    }

    pub fn as_rectangle(&self) -> Option<&RectangleCollisionMesh> {
        match self {
            CollisionMesh::Rectangle(mesh) => Some(mesh),
            _ => None,
        }
    }

    pub fn as_rectangle_mut(&mut self) -> Option<&mut RectangleCollisionMesh> {
        match self {
            CollisionMesh::Rectangle(mesh) => Some(mesh),
            _ => None,
        }
    }
}
