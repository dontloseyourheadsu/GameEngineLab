use crate::bodies::solid_body::SolidBody;
use rapier2d::{
    na::{Vector2, vector},
    prelude::{
        CCDSolver, ColliderBuilder, ColliderSet, DefaultBroadPhase, ImpulseJointSet,
        IntegrationParameters, IslandManager, MultibodyJointSet, NarrowPhase, PhysicsPipeline,
        RigidBodyHandle, RigidBodySet,
    },
};

#[derive(Default)]
pub struct PhysicsWorld {
    pub rigid_body_set: RigidBodySet,
    pub rigid_body_handles: Vec<RigidBodyHandle>,
    pub collider_set: ColliderSet,

    broad_phase: DefaultBroadPhase,
    ccd_solver: CCDSolver,
    event_handler: (),
    gravity: Vector2<f32>,
    impulse_joint_set: ImpulseJointSet,
    integration_parameters: IntegrationParameters,
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

        // Build ground collider at y = 0, spanning the world width
        let ground_collider = ColliderBuilder::cuboid(width as f32 / 2.0, 1.0)
            .translation(vector![width as f32 / 2.0, 1.0])
            .restitution(0.7)
            .build();
        collider_set.insert(ground_collider);

        let integration_parameters = IntegrationParameters::default();
        let physics_pipeline = PhysicsPipeline::new();
        let island_manager = IslandManager::new();
        let broad_phase = DefaultBroadPhase::new();
        let narrow_phase = NarrowPhase::new();
        let impulse_joint_set = ImpulseJointSet::new();
        let multibody_joint_set = MultibodyJointSet::new();
        let ccd_solver = CCDSolver::new();
        let physics_hooks = ();
        let event_handler = ();

        Self {
            rigid_body_set,
            collider_set,
            gravity,
            physics_pipeline,
            island_manager,
            broad_phase,
            narrow_phase,
            impulse_joint_set,
            multibody_joint_set,
            ccd_solver,
            physics_hooks,
            event_handler,
            integration_parameters,
            rigid_body_handles,
        }
    }

    pub fn step(&mut self) {
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

    pub fn add_ball(&mut self, position: Vector2<f32>, radius: f32, restitution: f32) {
        let rigid_body =
            SolidBody::new(rapier2d::prelude::RigidBodyType::Dynamic, position, 0.0).body;

        let collider = ColliderBuilder::ball(radius)
            .restitution(restitution)
            .build();

        let ball_body_handle = self.rigid_body_set.insert(rigid_body);
        self.rigid_body_handles.push(ball_body_handle);

        self.collider_set
            .insert_with_parent(collider, ball_body_handle, &mut self.rigid_body_set);
    }
}
