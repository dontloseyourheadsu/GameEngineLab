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
    pub collider_set: ColliderSet,
    pub rigid_body_handles: Vec<RigidBodyHandle>,

    pub(crate) broad_phase: DefaultBroadPhase,
    pub(crate) ccd_solver: CCDSolver,
    pub(crate) event_handler: (),
    pub(crate) gravity: Vector2<f32>,
    pub(crate) impulse_joint_set: ImpulseJointSet,
    pub(crate) integration_parameters: IntegrationParameters,
    pub(crate) island_manager: IslandManager,
    pub(crate) multibody_joint_set: MultibodyJointSet,
    pub(crate) narrow_phase: NarrowPhase,
    pub(crate) physics_hooks: (),
    pub(crate) physics_pipeline: PhysicsPipeline,
}

impl PhysicsWorld {
    pub fn new(width: f32, height: f32, gravity: Vector2<f32>) -> Self {
        let mut rigid_body_set = RigidBodySet::new();
        let mut collider_set = ColliderSet::new();
        let mut rigid_body_handles = Vec::new();

        // Build ground collider
        let collider = ColliderBuilder::cuboid(width, height).build();
        collider_set.insert(collider);

        /* Create the bouncing ball. */
        let rigid_body = SolidBody::new(
            rapier2d::prelude::RigidBodyType::Dynamic,
            vector![0.0, 5.0],
            0.0,
        )
        .body;

        let collider = ColliderBuilder::ball(0.5).restitution(0.7).build();

        let ball_body_handle = rigid_body_set.insert(rigid_body);
        rigid_body_handles.push(ball_body_handle);

        collider_set.insert_with_parent(collider, ball_body_handle, &mut rigid_body_set);

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

    fn step(&mut self) {
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

        for ball_handle in &self.rigid_body_handles {
            let ball_body = &self.rigid_body_set[*ball_handle];
            println!("Ball altitude: {}", ball_body.translation().y);
        }
    }
}
