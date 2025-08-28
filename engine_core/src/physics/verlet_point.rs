use crate::collision::CollisionMesh;
use crate::physics::PhysicsObject;
use crate::rendering::RenderMesh;
use raylib::prelude::*;

#[derive(Debug)]
pub struct VerletPoint {
    position: Vector2,
    previous: Vector2,
    acceleration: Vector2,
    mass: f32,
    size: f32,
    color: Color,
    collision_mesh: Option<CollisionMesh>,
    render_mesh: Option<RenderMesh>,
}

impl VerletPoint {
    pub fn new(position: Vector2) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass: 1.0,
            size: 5.0,
            color: Color::WHITE,
            collision_mesh: None,
            render_mesh: None,
        }
    }

    pub fn new_with_properties(position: Vector2, mass: f32, size: f32) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            size,
            color: Color::WHITE,
            collision_mesh: None,
            render_mesh: None,
        }
    }

    pub fn new_full(position: Vector2, mass: f32, size: f32, color: Color) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            size,
            color,
            collision_mesh: None,
            render_mesh: None,
        }
    }

    pub fn new_with_collision(
        position: Vector2,
        mass: f32,
        size: f32,
        color: Color,
        collision_mesh: CollisionMesh,
    ) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            size,
            color,
            collision_mesh: Some(collision_mesh),
            render_mesh: None,
        }
    }

    pub fn new_with_render_mesh(
        position: Vector2,
        mass: f32,
        size: f32,
        color: Color,
        render_mesh: RenderMesh,
    ) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            size,
            color,
            collision_mesh: None,
            render_mesh: Some(render_mesh),
        }
    }

    pub fn new_complete(
        position: Vector2,
        mass: f32,
        size: f32,
        color: Color,
        collision_mesh: Option<CollisionMesh>,
        render_mesh: Option<RenderMesh>,
    ) -> Self {
        VerletPoint {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            size,
            color,
            collision_mesh,
            render_mesh,
        }
    }

    pub fn apply_impulse_to_previous(&mut self, impulse: Vector2) {
        self.previous.x += impulse.x;
        self.previous.y += impulse.y;
    }

    // RenderMesh related methods
    pub fn size(&self) -> f32 {
        self.size
    }

    pub fn color(&self) -> Color {
        self.color
    }

    pub fn set_size(&mut self, size: f32) {
        self.size = size;
    }

    pub fn set_color(&mut self, color: Color) {
        self.color = color;
    }

    pub fn set_collision_mesh(&mut self, collision_mesh: Option<CollisionMesh>) {
        self.collision_mesh = collision_mesh;
    }
    pub fn render_mesh(&self) -> Option<&RenderMesh> {
        self.render_mesh.as_ref()
    }

    pub fn render_mesh_mut(&mut self) -> Option<&mut RenderMesh> {
        self.render_mesh.as_mut()
    }

    pub fn set_render_mesh(&mut self, render_mesh: Option<RenderMesh>) {
        self.render_mesh = render_mesh;
    }

    pub fn has_render_mesh(&self) -> bool {
        self.render_mesh.is_some()
    }
}

impl PhysicsObject for VerletPoint {
    fn position(&self) -> Vector2 {
        self.position
    }

    fn set_position(&mut self, position: Vector2) {
        self.position = position;
    }

    fn previous_position(&self) -> Vector2 {
        self.previous
    }

    fn mass(&self) -> f32 {
        self.mass
    }

    fn set_mass(&mut self, mass: f32) {
        self.mass = mass;
    }

    fn apply_force(&mut self, force: Vector2) {
        // F = ma, so a = F/m
        self.acceleration += force / self.mass;
    }

    fn apply_impulse_to_previous(&mut self, impulse: Vector2) {
        self.previous.x += impulse.x;
        self.previous.y += impulse.y;
    }

    fn update(&mut self, delta_time: f32, world_bounds: Rectangle) {
        let temp = self.position;
        self.position +=
            self.position - self.previous + self.acceleration * delta_time * delta_time;
        self.previous = temp;
        self.acceleration = Vector2::zero();

        // Apply world bounds constraints
        if self.position.x - self.size < world_bounds.x {
            self.position.x = world_bounds.x + self.size;
        } else if self.position.x + self.size > world_bounds.x + world_bounds.width {
            self.position.x = world_bounds.x + world_bounds.width - self.size;
        }

        if self.position.y - self.size < world_bounds.y {
            self.position.y = world_bounds.y + self.size;
        } else if self.position.y + self.size > world_bounds.y + world_bounds.height {
            self.position.y = world_bounds.y + world_bounds.height - self.size;
        }
    }

    fn collision_mesh(&self) -> Option<&CollisionMesh> {
        self.collision_mesh.as_ref()
    }

    fn collision_mesh_mut(&mut self) -> Option<&mut CollisionMesh> {
        self.collision_mesh.as_mut()
    }

    fn has_collision(&self) -> bool {
        self.collision_mesh.is_some() && self.collision_mesh.as_ref().unwrap().is_enabled()
    }

    fn draw(&self, d: &mut RaylibDrawHandle, texture: Option<&Texture2D>) {
        if let Some(render_mesh) = &self.render_mesh {
            if !render_mesh.visible {
                return; // Don't draw if render mesh exists but is not visible
            }

            if let Some(tex) = texture {
                // Render texture using render mesh properties
                let dest_rect = Rectangle::new(
                    self.position.x - (render_mesh.scale.x) / 2.0,
                    self.position.y - (render_mesh.scale.y) / 2.0,
                    render_mesh.scale.x,
                    render_mesh.scale.y,
                );

                let src_rect = Rectangle::new(0.0, 0.0, tex.width as f32, tex.height as f32);

                let origin = Vector2::new((render_mesh.scale.x) / 2.0, (render_mesh.scale.y) / 2.0);

                d.draw_texture_pro(
                    tex,
                    src_rect,
                    dest_rect,
                    origin,
                    render_mesh.rotation,
                    render_mesh.tint,
                );
            } else {
                // Render as colored circle with render mesh tint (fallback)
                d.draw_circle_v(self.position, self.size, render_mesh.tint);
            }
        } else {
            // Default rendering - colored circle using point's color
            d.draw_circle_v(self.position, self.size, self.color);
        }
    }

    fn draw_simple(&self, d: &mut RaylibDrawHandle) {
        self.draw(d, None);
    }
}
