use rand::{thread_rng, Rng};
use raylib::prelude::*;
use std::collections::HashMap;

#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ParticleColor {
    Yellow,
    Red,
    Green,
    Blue,
    White,
}

impl ParticleColor {
    fn to_raylib_color(&self) -> Color {
        match self {
            ParticleColor::Yellow => Color::YELLOW,
            ParticleColor::Red => Color::RED,
            ParticleColor::Green => Color::LIME,
            ParticleColor::Blue => Color::BLUE,
            ParticleColor::White => Color::WHITE,
        }
    }
}

#[derive(Debug, Clone, Copy)]
pub enum ParticleType {
    Life,    // For particle life simulation (attraction/repulsion)
    Emitted, // For emitted particles with physics (fire, water, etc.)
}

#[derive(Debug, Clone)]
pub struct Particle {
    pub position: Vector2,
    pub velocity: Vector2,
    pub color: ParticleColor,
    pub particle_type: ParticleType,
    pub size: f32,
    pub mass: f32,
    pub lifetime: f32,
    pub elapsed_time: f32,
    pub alpha: f32,
    pub force: Vector2,
    pub texture_index: Option<usize>,
}

impl Particle {
    pub fn new_life(position: Vector2, color: ParticleColor) -> Self {
        let mut rng = thread_rng();
        Self {
            position,
            velocity: Vector2::new(rng.gen_range(-0.5..0.5), rng.gen_range(-0.5..0.5)),
            color,
            particle_type: ParticleType::Life,
            size: 8.0, // Increased from 5.0
            mass: 1.0,
            lifetime: f32::INFINITY,
            elapsed_time: 0.0,
            alpha: 1.0,
            force: Vector2::zero(),
            texture_index: None,
        }
    }

    pub fn new_emitted(position: Vector2, velocity: Vector2, texture_index: Option<usize>) -> Self {
        let mut rng = thread_rng();
        Self {
            position,
            velocity,
            color: ParticleColor::White,
            particle_type: ParticleType::Emitted,
            size: rng.gen_range(10.0..25.0), // Increased from 5.0..15.0
            mass: rng.gen_range(10.0..25.0), // Increased from 5.0..15.0
            lifetime: rng.gen_range(0.5..2.0),
            elapsed_time: 0.0,
            alpha: rng.gen_range(0.3..0.8),
            force: Vector2::zero(),
            texture_index,
        }
    }

    pub fn update(&mut self, dt: f32, screen_width: f32, screen_height: f32) {
        match self.particle_type {
            ParticleType::Life => {
                // Apply friction for life particles
                let friction = 0.99;
                self.velocity = Vector2::new(
                    (self.velocity.x + self.force.x) * friction,
                    (self.velocity.y + self.force.y) * friction,
                );

                // Boundary collision for life particles
                if self.position.x <= 0.0 || self.position.x >= screen_width - self.size {
                    self.velocity.x *= -1.0;
                }
                if self.position.y <= 0.0 || self.position.y >= screen_height - self.size {
                    self.velocity.y *= -1.0;
                }

                self.position.x += self.velocity.x;
                self.position.y += self.velocity.y;

                // Keep within bounds
                self.position.x = self.position.x.clamp(0.0, screen_width - self.size);
                self.position.y = self.position.y.clamp(0.0, screen_height - self.size);
            }
            ParticleType::Emitted => {
                // Update physics for emitted particles
                self.velocity.x += self.force.x;
                self.velocity.y += self.force.y;

                self.position.x += self.velocity.x * dt;
                self.position.y += self.velocity.y * dt;

                self.elapsed_time += dt;

                // Reset particle if out of bounds or lifetime exceeded
                if self.position.x < 0.0
                    || self.position.x > screen_width
                    || self.position.y < 0.0
                    || self.position.y > screen_height
                    || self.elapsed_time > self.lifetime
                {
                    self.reset_emitted();
                }
            }
        }

        // Reset force for next frame
        self.force = Vector2::zero();
    }

    fn reset_emitted(&mut self) {
        let mut rng = thread_rng();
        self.elapsed_time = 0.0;
        self.lifetime = rng.gen_range(0.5..2.0);
        self.alpha = rng.gen_range(0.3..0.8);
        // Position and velocity will be reset by emitter
    }

    pub fn apply_force(&mut self, force: Vector2) {
        self.force.x += force.x;
        self.force.y += force.y;
    }

    pub fn draw(&self, d: &mut RaylibDrawHandle, textures: &[Texture2D]) {
        match self.particle_type {
            ParticleType::Life => {
                let color = self.color.to_raylib_color();
                d.draw_rectangle(
                    self.position.x as i32,
                    self.position.y as i32,
                    self.size as i32,
                    self.size as i32,
                    color,
                );
            }
            ParticleType::Emitted => {
                if let Some(texture_idx) = self.texture_index {
                    if texture_idx < textures.len() {
                        let mut color = Color::WHITE;
                        color.a = (255.0 * self.alpha) as u8;
                        d.draw_texture_ex(
                            &textures[texture_idx],
                            self.position,
                            0.0,
                            self.size / textures[texture_idx].width as f32,
                            color,
                        );
                    }
                } else {
                    // Fallback to colored rectangle
                    let mut color = self.color.to_raylib_color();
                    color.a = (255.0 * self.alpha) as u8;
                    d.draw_rectangle(
                        self.position.x as i32,
                        self.position.y as i32,
                        self.size as i32,
                        self.size as i32,
                        color,
                    );
                }
            }
        }
    }
}

#[derive(Debug, Clone, Copy)]
pub struct ParticleRule {
    pub receiver: ParticleColor,
    pub sender: ParticleColor,
    pub gravity: f32,
    pub force_distance: f32,
}

pub struct Influencer {
    pub gravity_strength: f32,
    pub wind_force: Vector2,
}

impl Influencer {
    pub fn new(gravity: f32, wind: Vector2) -> Self {
        Self {
            gravity_strength: gravity,
            wind_force: wind,
        }
    }

    pub fn apply_to_particle(&self, particle: &mut Particle, screen_height: f32) {
        match particle.particle_type {
            ParticleType::Emitted => {
                // Apply gravity
                let gravity_force = Vector2::new(0.0, self.gravity_strength * particle.mass);
                particle.apply_force(gravity_force);

                // Apply wind
                let wind_force = Vector2::new(
                    self.wind_force.x * (particle.position.x / screen_height),
                    self.wind_force.y,
                );
                particle.apply_force(wind_force);
            }
            ParticleType::Life => {
                // Life particles don't use external influences
            }
        }
    }
}

pub struct Emitter {
    pub position: Vector2,
    pub velocity_range: (Vector2, Vector2),
    pub particle_count: usize,
    pub texture_type: String, // "fire" or "water"
}

impl Emitter {
    pub fn new(position: Vector2, texture_type: String) -> Self {
        Self {
            position,
            velocity_range: (Vector2::new(-1.0, -2.0), Vector2::new(2.0, -0.5)), // Fixed Y range
            particle_count: 300,
            texture_type,
        }
    }

    pub fn emit_particle(&self, textures: &[Texture2D]) -> Particle {
        let mut rng = thread_rng();

        let velocity = Vector2::new(
            rng.gen_range(self.velocity_range.0.x..self.velocity_range.1.x) * 0.01,
            rng.gen_range(self.velocity_range.0.y..self.velocity_range.1.y) * 0.5,
        );

        let texture_index = if textures.len() > 0 {
            Some(rng.gen_range(0..textures.len()))
        } else {
            None
        };

        let particle = Particle::new_emitted(
            self.position + Vector2::new(rng.gen_range(-1.0..1.0), rng.gen_range(-1.0..1.0)),
            velocity,
            texture_index,
        );

        particle
    }

    pub fn generate_particles(&self, textures: &[Texture2D]) -> Vec<Particle> {
        (0..self.particle_count)
            .map(|_| self.emit_particle(textures))
            .collect()
    }
}

pub struct ParticleSystem {
    pub life_particles: HashMap<ParticleColor, Vec<Particle>>,
    pub emitted_particles: Vec<Particle>,
    pub rules: Vec<ParticleRule>,
    pub influencer: Influencer,
    pub emitters: Vec<Emitter>,
    pub fire_textures: Vec<Texture2D>,
    pub water_textures: Vec<Texture2D>,
    pub current_texture_type: String,
    pub current_mode: SimulationMode,
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum SimulationMode {
    LifeSimulation,
    ParticleEmission,
    Mixed,
}

impl ParticleSystem {
    pub fn new() -> Self {
        Self {
            life_particles: HashMap::new(),
            emitted_particles: Vec::new(),
            rules: Vec::new(),
            influencer: Influencer::new(-0.5, Vector2::new(0.1, 0.0)),
            emitters: Vec::new(),
            fire_textures: Vec::new(),
            water_textures: Vec::new(),
            current_texture_type: "fire".to_string(),
            current_mode: SimulationMode::Mixed,
        }
    }

    pub fn load_textures(&mut self, rl: &mut RaylibHandle, thread: &RaylibThread) {
        // Try to load fire textures
        for i in 1..=3 {
            let filename = match i {
                1 => "Resources/sfirei.png",
                2 => "Resources/sFIREII.png",
                3 => "Resources/sFIREIII.png",
                _ => continue,
            };

            if let Ok(texture) = rl.load_texture(thread, filename) {
                self.fire_textures.push(texture);
            }
        }

        // Try to load water textures
        for filename in &[
            "Resources/WAT I.png",
            "Resources/WAT II.png",
            "Resources/WAT iii.png",
        ] {
            if let Ok(texture) = rl.load_texture(thread, filename) {
                self.water_textures.push(texture);
            }
        }
    }

    pub fn get_current_textures(&self) -> &[Texture2D] {
        match self.current_texture_type.as_str() {
            "fire" => &self.fire_textures,
            "water" => &self.water_textures,
            _ => &self.fire_textures,
        }
    }

    pub fn set_texture_type(&mut self, texture_type: &str) {
        self.current_texture_type = texture_type.to_string();
    }

    pub fn init_life_simulation(&mut self, screen_width: f32, screen_height: f32) {
        let mut rng = thread_rng();

        // Create particles for life simulation
        for &color in &[
            ParticleColor::Yellow,
            ParticleColor::Red,
            ParticleColor::Green,
        ] {
            let mut particles = Vec::new();
            for _ in 0..200 {
                let pos = Vector2::new(
                    rng.gen_range(0.0..screen_width - 8.0), // Updated for new particle size
                    rng.gen_range(0.0..screen_height - 8.0), // Updated for new particle size
                );
                particles.push(Particle::new_life(pos, color));
            }
            self.life_particles.insert(color, particles);
        }

        // Set up particle interaction rules
        self.rules = vec![
            // Green particles
            ParticleRule {
                receiver: ParticleColor::Green,
                sender: ParticleColor::Green,
                gravity: -0.1,
                force_distance: 100.0,
            },
            ParticleRule {
                receiver: ParticleColor::Green,
                sender: ParticleColor::Red,
                gravity: 0.1,
                force_distance: 100.0,
            },
            ParticleRule {
                receiver: ParticleColor::Green,
                sender: ParticleColor::Yellow,
                gravity: 0.1,
                force_distance: 100.0,
            },
            // Red particles
            ParticleRule {
                receiver: ParticleColor::Red,
                sender: ParticleColor::Red,
                gravity: -0.1,
                force_distance: 100.0,
            },
            ParticleRule {
                receiver: ParticleColor::Red,
                sender: ParticleColor::Yellow,
                gravity: 0.1,
                force_distance: 100.0,
            },
            // Yellow particles
            ParticleRule {
                receiver: ParticleColor::Yellow,
                sender: ParticleColor::Yellow,
                gravity: -0.1,
                force_distance: 100.0,
            },
        ];
    }

    pub fn add_emitter(&mut self, position: Vector2, texture_type: String) {
        let emitter = Emitter::new(position, texture_type);
        let textures = self.get_current_textures();

        // Only generate particles if we have textures or if we're in emitted mode
        if !textures.is_empty() || self.current_mode != SimulationMode::LifeSimulation {
            let particles = emitter.generate_particles(textures);
            self.emitted_particles.extend(particles);
            self.emitters.push(emitter);
        }
    }

    pub fn update(&mut self, dt: f32, screen_width: f32, screen_height: f32) {
        match self.current_mode {
            SimulationMode::LifeSimulation | SimulationMode::Mixed => {
                self.update_life_particles(dt, screen_width, screen_height);
            }
            _ => {}
        }

        match self.current_mode {
            SimulationMode::ParticleEmission | SimulationMode::Mixed => {
                self.update_emitted_particles(dt, screen_width, screen_height);
            }
            _ => {}
        }
    }

    fn update_life_particles(&mut self, dt: f32, screen_width: f32, screen_height: f32) {
        // Apply particle rules
        for rule in &self.rules {
            if let (Some(_receivers), Some(senders)) = (
                self.life_particles.get(&rule.receiver),
                self.life_particles.get(&rule.sender),
            ) {
                let senders_clone = senders.clone();

                if let Some(receivers_mut) = self.life_particles.get_mut(&rule.receiver) {
                    for receiver in receivers_mut.iter_mut() {
                        let mut force = Vector2::zero();

                        for sender in &senders_clone {
                            let dx = receiver.position.x - sender.position.x;
                            let dy = receiver.position.y - sender.position.y;
                            let distance = (dx * dx + dy * dy).sqrt();

                            if distance > 0.0 && distance < rule.force_distance {
                                let force_magnitude = rule.gravity / distance;
                                force.x += force_magnitude * dx / distance;
                                force.y += force_magnitude * dy / distance;
                            }
                        }

                        receiver.apply_force(force);
                    }
                }
            }
        }

        // Update particle positions
        for particles in self.life_particles.values_mut() {
            for particle in particles.iter_mut() {
                particle.update(dt, screen_width, screen_height);
            }
        }
    }

    fn update_emitted_particles(&mut self, dt: f32, screen_width: f32, screen_height: f32) {
        for particle in &mut self.emitted_particles {
            self.influencer.apply_to_particle(particle, screen_height);
            particle.update(dt, screen_width, screen_height);
        }
    }

    pub fn draw(&self, d: &mut RaylibDrawHandle) {
        let current_textures = self.get_current_textures();

        match self.current_mode {
            SimulationMode::LifeSimulation | SimulationMode::Mixed => {
                for particles in self.life_particles.values() {
                    for particle in particles {
                        particle.draw(d, current_textures);
                    }
                }
            }
            _ => {}
        }

        match self.current_mode {
            SimulationMode::ParticleEmission | SimulationMode::Mixed => {
                for particle in &self.emitted_particles {
                    particle.draw(d, current_textures);
                }
            }
            _ => {}
        }
    }

    pub fn reset_life_simulation(&mut self, screen_width: f32, screen_height: f32) {
        self.life_particles.clear();
        self.init_life_simulation(screen_width, screen_height);
    }

    pub fn clear_emitters(&mut self) {
        self.emitters.clear();
        self.emitted_particles.clear();
    }

    pub fn set_mode(&mut self, mode: SimulationMode) {
        self.current_mode = mode;
    }
}

fn main() {
    const SCREEN_WIDTH: i32 = 1200;
    const SCREEN_HEIGHT: i32 = 900;

    let (mut rl, thread) = raylib::init()
        .size(SCREEN_WIDTH, SCREEN_HEIGHT)
        .title("Particle Simulator RS")
        .build();

    rl.set_target_fps(60);

    let mut particle_system = ParticleSystem::new();
    particle_system.load_textures(&mut rl, &thread);
    particle_system.init_life_simulation(SCREEN_WIDTH as f32, SCREEN_HEIGHT as f32);

    let mut show_ui_info = true;

    while !rl.window_should_close() {
        // Handle input
        if rl.is_key_pressed(KeyboardKey::KEY_ONE) {
            particle_system.set_mode(SimulationMode::LifeSimulation);
        } else if rl.is_key_pressed(KeyboardKey::KEY_TWO) {
            particle_system.set_mode(SimulationMode::ParticleEmission);
        } else if rl.is_key_pressed(KeyboardKey::KEY_THREE) {
            particle_system.set_mode(SimulationMode::Mixed);
        }

        if rl.is_key_pressed(KeyboardKey::KEY_R) {
            particle_system.reset_life_simulation(SCREEN_WIDTH as f32, SCREEN_HEIGHT as f32);
        }

        if rl.is_key_pressed(KeyboardKey::KEY_C) {
            particle_system.clear_emitters();
        }

        if rl.is_key_pressed(KeyboardKey::KEY_F) {
            particle_system.set_texture_type("fire");
        } else if rl.is_key_pressed(KeyboardKey::KEY_W) {
            particle_system.set_texture_type("water");
        }

        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_LEFT) {
            let mouse_pos = rl.get_mouse_position();
            println!(
                "Adding emitter at position: ({}, {})",
                mouse_pos.x, mouse_pos.y
            );
            particle_system.add_emitter(mouse_pos, "fire".to_string());
        }

        // Explicitly ignore right-click to prevent any issues
        if rl.is_mouse_button_pressed(MouseButton::MOUSE_BUTTON_RIGHT) {
            println!("Right-click detected - ignoring");
        }

        if rl.is_key_pressed(KeyboardKey::KEY_H) {
            show_ui_info = !show_ui_info;
        }

        // Update
        let dt = rl.get_frame_time();
        particle_system.update(dt, SCREEN_WIDTH as f32, SCREEN_HEIGHT as f32);

        // Draw
        let mut d = rl.begin_drawing(&thread);
        d.clear_background(Color::BLACK);

        particle_system.draw(&mut d);

        // UI
        if show_ui_info {
            let ui_y = 15;
            d.draw_text("Particle Simulator RS", 15, ui_y, 28, Color::WHITE);
            d.draw_text(
                &format!("Mode: {:?}", particle_system.current_mode),
                15,
                ui_y + 35,
                22,
                Color::WHITE,
            );
            d.draw_text("Controls:", 15, ui_y + 65, 22, Color::YELLOW);
            d.draw_text("1 - Life Simulation", 15, ui_y + 95, 18, Color::WHITE);
            d.draw_text("2 - Particle Emission", 15, ui_y + 120, 18, Color::WHITE);
            d.draw_text("3 - Mixed Mode", 15, ui_y + 145, 18, Color::WHITE);
            d.draw_text(
                "R - Reset Life Simulation",
                15,
                ui_y + 170,
                18,
                Color::WHITE,
            );
            d.draw_text("C - Clear Emitters", 15, ui_y + 195, 18, Color::WHITE);
            d.draw_text("F - Fire Textures", 15, ui_y + 220, 18, Color::WHITE);
            d.draw_text("W - Water Textures", 15, ui_y + 245, 18, Color::WHITE);
            d.draw_text("Click - Add Emitter", 15, ui_y + 270, 18, Color::WHITE);
            d.draw_text("H - Toggle Help", 15, ui_y + 295, 18, Color::WHITE);
        }
    }
}
