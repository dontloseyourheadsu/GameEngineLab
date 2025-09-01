// physics_world.rs
use rapier2d::{
    na::{Vector2, vector},
    prelude::{
        CCDSolver, ColliderBuilder, ColliderSet, DefaultBroadPhase, ImpulseJointSet,
        IntegrationParameters, IslandManager, MultibodyJointSet, NarrowPhase, PhysicsPipeline,
        RigidBodyHandle, RigidBodySet,
    },
};

use crate::rigid_bodies::solid_body_build::SolidBodyBuild;

#[derive(Default)]
pub struct PhysicsWorld {
    pub rigid_body_set: RigidBodySet,
    pub rigid_body_handles: Vec<RigidBodyHandle>,
    pub collider_set: ColliderSet,

    broad_phase: DefaultBroadPhase,
    ccd_solver: CCDSolver,
    event_handler: (),
    pub gravity: Vector2<f32>,
    impulse_joint_set: ImpulseJointSet,
    pub integration_parameters: IntegrationParameters,
    island_manager: IslandManager,
    multibody_joint_set: MultibodyJointSet,
    narrow_phase: NarrowPhase,
    physics_hooks: (),
    physics_pipeline: PhysicsPipeline,
}

impl PhysicsWorld {
    pub fn new(width: i32, _height: i32, gravity: Vector2<f32>) -> Self {
        let rigid_body_set = RigidBodySet::new();
        let mut collider_set = ColliderSet::new();
        let rigid_body_handles = Vec::new();

        // Suelo en y=0 a lo largo del ancho del mundo
        let ground_collider = ColliderBuilder::cuboid(width as f32 / 2.0, 1.0)
            .translation(vector![width as f32 / 2.0, -1.0])
            .restitution(0.7)
            .friction(0.8) // Ground friction
            .build();
        collider_set.insert(ground_collider);

        // Importante: dt lo setearás por frame con step_with_dt
        let mut integration_parameters = IntegrationParameters::default();
        integration_parameters.dt = 1.0 / 60.0; // valor por defecto seguro

        Self {
            rigid_body_set,
            collider_set,
            gravity,
            physics_pipeline: PhysicsPipeline::new(),
            island_manager: IslandManager::new(),
            broad_phase: DefaultBroadPhase::new(),
            narrow_phase: NarrowPhase::new(),
            impulse_joint_set: ImpulseJointSet::new(),
            multibody_joint_set: MultibodyJointSet::new(),
            ccd_solver: CCDSolver::new(),
            physics_hooks: (),
            event_handler: (),
            integration_parameters,
            rigid_body_handles,
        }
    }

    /// Step con dt explícito (segundos) para que Rapier integre a tiempo real.
    pub fn step_with_dt(&mut self, dt: f32) {
        self.integration_parameters.dt = dt;
        self.physics_pipeline.step(
            &self.gravity,
            &self.integration_parameters,
            &mut self.island_manager,
            &mut self.broad_phase,
            &mut self.narrow_phase,
            &mut self.rigid_body_set,
            &mut self.collider_set,
            &mut self.impulse_joint_set,
            &mut self.multibody_joint_set,
            &mut self.ccd_solver,
            &mut self.physics_hooks,
            &mut self.event_handler,
        );
    }

    // Conserva el step() simple si lo quieres, usa el dt actual en integration_parameters.
    pub fn step(&mut self) {
        self.step_with_dt(self.integration_parameters.dt);
    }

    pub fn add_solid_body<T: SolidBodyBuild>(&mut self, solid_body_build: T) {
        let rigid_body = solid_body_build.body().clone();
        let collider = Some(solid_body_build.collider().clone());

        // Insert the rigid body into the world
        let ball_body_handle = self.rigid_body_set.insert(rigid_body);

        // Insert handle into the list of rigid body handles to manipulate object
        self.rigid_body_handles.push(ball_body_handle);

        // Insert the collider into the world if it is present
        if let Some(collider) = collider {
            self.collider_set.insert_with_parent(
                collider,
                ball_body_handle,
                &mut self.rigid_body_set,
            );
        }
    }
}
