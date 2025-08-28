use crate::collision::CollisionMesh;
use crate::physics::PhysicsObject;
use crate::rendering::RenderMesh;
use raylib::prelude::*;

#[derive(Debug)]
pub struct BoxObject {
    position: Vector2,
    previous: Vector2,
    acceleration: Vector2,
    mass: f32,
    width: f32,
    height: f32,
    rotation: f32,
    angular_velocity: f32,
    color: Color,
    collision_mesh: Option<CollisionMesh>,
    render_mesh: Option<RenderMesh>,
    can_rotate: bool, // Whether this box can rotate during physics updates
}

impl BoxObject {
    pub fn new(position: Vector2, width: f32, height: f32) -> Self {
        BoxObject {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass: 1.0,
            width,
            height,
            rotation: 0.0,
            angular_velocity: 0.0,
            color: Color::WHITE,
            collision_mesh: None,
            render_mesh: None,
            can_rotate: false,
        }
    }

    pub fn new_with_properties(position: Vector2, width: f32, height: f32, mass: f32) -> Self {
        BoxObject {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            width,
            height,
            rotation: 0.0,
            angular_velocity: 0.0,
            color: Color::WHITE,
            collision_mesh: None,
            render_mesh: None,
            can_rotate: false,
        }
    }

    pub fn new_full(position: Vector2, width: f32, height: f32, mass: f32, color: Color) -> Self {
        BoxObject {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            width,
            height,
            rotation: 0.0,
            angular_velocity: 0.0,
            color,
            collision_mesh: None,
            render_mesh: None,
            can_rotate: false,
        }
    }

    pub fn new_with_collision(
        position: Vector2,
        width: f32,
        height: f32,
        mass: f32,
        color: Color,
        collision_mesh: CollisionMesh,
        can_rotate: bool,
    ) -> Self {
        BoxObject {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            width,
            height,
            rotation: 0.0,
            angular_velocity: 0.0,
            color,
            collision_mesh: Some(collision_mesh),
            render_mesh: None,
            can_rotate,
        }
    }

    pub fn new_complete(
        position: Vector2,
        width: f32,
        height: f32,
        mass: f32,
        color: Color,
        collision_mesh: Option<CollisionMesh>,
        render_mesh: Option<RenderMesh>,
        can_rotate: bool,
    ) -> Self {
        BoxObject {
            position,
            previous: position,
            acceleration: Vector2::zero(),
            mass,
            width,
            height,
            rotation: 0.0,
            angular_velocity: 0.0,
            color,
            collision_mesh,
            render_mesh,
            can_rotate,
        }
    }

    // Box-specific getters and setters
    pub fn width(&self) -> f32 {
        self.width
    }

    pub fn height(&self) -> f32 {
        self.height
    }

    pub fn set_width(&mut self, width: f32) {
        self.width = width;
    }

    pub fn set_height(&mut self, height: f32) {
        self.height = height;
    }

    pub fn rotation(&self) -> f32 {
        self.rotation
    }

    pub fn set_rotation(&mut self, rotation: f32) {
        self.rotation = rotation;
        // Update collision mesh rotation if it's a rectangle
        if let Some(CollisionMesh::Rectangle(rect_mesh)) = &mut self.collision_mesh {
            rect_mesh.set_rotation(rotation);
        }
    }

    pub fn angular_velocity(&self) -> f32 {
        self.angular_velocity
    }

    pub fn set_angular_velocity(&mut self, angular_velocity: f32) {
        self.angular_velocity = angular_velocity;
    }

    pub fn color(&self) -> Color {
        self.color
    }

    pub fn set_color(&mut self, color: Color) {
        self.color = color;
    }

    pub fn can_rotate(&self) -> bool {
        self.can_rotate
    }

    pub fn set_can_rotate(&mut self, can_rotate: bool) {
        self.can_rotate = can_rotate;
    }

    pub fn apply_torque(&mut self, torque: f32) {
        if self.can_rotate {
            // Simple angular acceleration = torque / moment_of_inertia
            // For a rectangle: I = (m * (w² + h²)) / 12
            let moment_of_inertia =
                self.mass * (self.width * self.width + self.height * self.height) / 12.0;
            let angular_acceleration = torque / moment_of_inertia;
            self.angular_velocity += angular_acceleration;
        }
    }

    // RenderMesh related methods
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

impl PhysicsObject for BoxObject {
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

        // Update rotation if can rotate
        if self.can_rotate {
            self.rotation += self.angular_velocity * delta_time;
            // Keep rotation between 0 and 2π
            while self.rotation < 0.0 {
                self.rotation += 2.0 * std::f32::consts::PI;
            }
            while self.rotation >= 2.0 * std::f32::consts::PI {
                self.rotation -= 2.0 * std::f32::consts::PI;
            }

            // Apply angular damping
            self.angular_velocity *= 0.99;
        }

        // Always update collision mesh rotation to match visual rotation
        if let Some(CollisionMesh::Rectangle(rect_mesh)) = &mut self.collision_mesh {
            rect_mesh.set_rotation(self.rotation);
        }

        // Apply world bounds constraints (position is center of the box)
        let half_width = self.width / 2.0;
        let half_height = self.height / 2.0;

        if self.position.x - half_width < world_bounds.x {
            self.position.x = world_bounds.x + half_width;
        } else if self.position.x + half_width > world_bounds.x + world_bounds.width {
            self.position.x = world_bounds.x + world_bounds.width - half_width;
        }

        if self.position.y - half_height < world_bounds.y {
            self.position.y = world_bounds.y + half_height;
        } else if self.position.y + half_height > world_bounds.y + world_bounds.height {
            self.position.y = world_bounds.y + world_bounds.height - half_height;
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
                // Render texture so its top-left matches collision mesh top-left
                let dest_rect = Rectangle::new(
                    self.position.x - (render_mesh.scale.x * self.width) / 2.0, // Convert center to top-left
                    self.position.y - (render_mesh.scale.y * self.height) / 2.0, // Convert center to top-left
                    render_mesh.scale.x * self.width,
                    render_mesh.scale.y * self.height,
                );

                let src_rect = Rectangle::new(0.0, 0.0, tex.width as f32, tex.height as f32);

                let origin = Vector2::new(0.0, 0.0); // Use top-left as origin

                d.draw_texture_pro(
                    tex,
                    src_rect,
                    dest_rect,
                    origin,
                    self.rotation.to_degrees() + render_mesh.rotation,
                    render_mesh.tint,
                );
            } else {
                // Render rectangle so its top-left matches collision mesh top-left
                let rect = Rectangle::new(
                    self.position.x - (render_mesh.scale.x * self.width) / 2.0, // Convert center to top-left
                    self.position.y - (render_mesh.scale.y * self.height) / 2.0, // Convert center to top-left
                    render_mesh.scale.x * self.width,
                    render_mesh.scale.y * self.height,
                );

                let origin = Vector2::new(0.0, 0.0); // Use top-left as origin

                d.draw_rectangle_pro(
                    rect,
                    origin,
                    self.rotation.to_degrees() + render_mesh.rotation,
                    render_mesh.tint,
                );
            }
        } else {
            // Default rendering - draw rectangle so its top-left matches collision mesh top-left
            let rect = Rectangle::new(
                self.position.x - self.width / 2.0,  // Convert center to top-left
                self.position.y - self.height / 2.0, // Convert center to top-left
                self.width,
                self.height,
            );

            let origin = Vector2::new(0.0, 0.0); // Use top-left as origin

            d.draw_rectangle_pro(rect, origin, self.rotation.to_degrees(), self.color);
        }
    }

    fn draw_simple(&self, d: &mut RaylibDrawHandle) {
        self.draw(d, None);
    }
}
