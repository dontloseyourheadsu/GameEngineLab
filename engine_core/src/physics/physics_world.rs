// physics_world.rs
use rapier2d::{
    na::{Vector2, vector},
    prelude::{
        CCDSolver, ColliderBuilder, ColliderSet, DefaultBroadPhase, ImpulseJointHandle,
        ImpulseJointSet, IntegrationParameters, IslandManager, MultibodyJointSet, NarrowPhase,
        PhysicsPipeline, RigidBody, RigidBodyHandle, RigidBodySet,
    },
};

use crate::builders::rigid_bodies::spring_builder::SpringBuilder;
use crate::builders::soft_bodies::soft_rectangle_builder::SoftRectangleInstance;
use crate::rigid_bodies::solid_body_build::SolidBodyBuild;

#[derive(Default)]
pub struct PhysicsWorld {
    pub rigid_body_set: RigidBodySet,
    pub rigid_body_handles: Vec<RigidBodyHandle>,
    pub collider_set: ColliderSet,
    pub joint_handles: Vec<ImpulseJointHandle>,

    broad_phase: DefaultBroadPhase,
    ccd_solver: CCDSolver,
    event_handler: (),
    pub gravity: Vector2<f32>,
    pub impulse_joint_set: ImpulseJointSet,
    pub integration_parameters: IntegrationParameters,
    island_manager: IslandManager,
    multibody_joint_set: MultibodyJointSet,
    narrow_phase: NarrowPhase,
    physics_hooks: (),
    physics_pipeline: PhysicsPipeline,
    // Soft body instances for custom rendering
    pub soft_rectangles: Vec<SoftRectangleInstance>,
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
            joint_handles: Vec::new(),
            soft_rectangles: Vec::new(),
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

    pub fn add_solid_body<T: SolidBodyBuild>(&mut self, solid_body_build: T) -> RigidBodyHandle {
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

        ball_body_handle
    }

    /// Add a spring joint between two rigid bodies
    ///
    /// # Arguments
    /// * `spring_builder` - The spring builder containing joint configuration
    /// * `body1_handle` - Handle to the first rigid body
    /// * `body2_handle` - Handle to the second rigid body
    ///
    /// # Returns
    /// The handle of the created joint
    pub fn add_spring(
        &mut self,
        spring_builder: SpringBuilder,
        body1_handle: RigidBodyHandle,
        body2_handle: RigidBodyHandle,
    ) -> ImpulseJointHandle {
        let (joint, _spring_type) = spring_builder.build();
        let joint_handle = self.impulse_joint_set.insert(
            body1_handle,
            body2_handle,
            joint,
            true, // wake up the bodies
        );

        self.joint_handles.push(joint_handle);
        joint_handle
    }

    /// Remove a spring joint from the physics world
    pub fn remove_spring(&mut self, joint_handle: ImpulseJointHandle) -> bool {
        if let Some(pos) = self.joint_handles.iter().position(|&h| h == joint_handle) {
            self.joint_handles.remove(pos);
            self.impulse_joint_set.remove(joint_handle, true).is_some()
        } else {
            false
        }
    }

    /// Get a reference to a rigid body by its handle
    pub fn get_rigid_body(&self, handle: RigidBodyHandle) -> Option<&RigidBody> {
        self.rigid_body_set.get(handle)
    }

    /// Get a mutable reference to a rigid body by its handle
    pub fn get_rigid_body_mut(&mut self, handle: RigidBodyHandle) -> Option<&mut RigidBody> {
        self.rigid_body_set.get_mut(handle)
    }

    /// Register a SoftRectangle instance for rendering/management; returns index handle
    pub fn register_soft_rectangle(&mut self, instance: SoftRectangleInstance) -> usize {
        self.soft_rectangles.push(instance);
        self.soft_rectangles.len() - 1
    }

    /// Convenience: add a distance spring between two bodies with given rest length.
    /// This uses our SpringBuilder to approximate a distance joint.
    pub fn add_distance_spring(
        &mut self,
        a: RigidBodyHandle,
        b: RigidBodyHandle,
        rest_length: f32,
        stiffness: f32,
        damping: f32,
    ) -> ImpulseJointHandle {
        // Use a linear spring with target position = rest length; axis is arbitrary, we rely on solver.
        let spring = SpringBuilder::new_linear(
            vector![1.0, 0.0],
            rapier2d::na::Point2::origin(),
            rapier2d::na::Point2::origin(),
            stiffness,
            damping,
        )
        .with_target_position(rest_length);
        self.add_spring(spring, a, b)
    }
}
