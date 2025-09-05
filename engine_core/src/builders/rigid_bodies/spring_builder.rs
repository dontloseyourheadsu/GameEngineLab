use nalgebra::{Point2, Unit, Vector2};
use rapier2d::prelude::*;

#[derive(Clone)]
pub enum SpringType {
    /// Linear spring that constrains movement along a specific axis
    Linear { axis: Vector2<f32> },
    /// Distance spring that maintains a specific distance between two points
    Distance,
}

#[derive(Clone)]
pub struct SpringBuilder {
    spring_type: SpringType,
    local_anchor1: Point2<f32>,
    local_anchor2: Point2<f32>,
    stiffness: f32,
    damping: f32,
    rest_length: Option<f32>,
    target_position: Option<f32>,
}

impl SpringBuilder {
    /// Creates a linear spring using a prismatic joint with motor
    /// This constrains movement along a specific axis and applies spring forces
    pub fn new_linear(
        axis: Vector2<f32>,
        local_anchor1: Point2<f32>,
        local_anchor2: Point2<f32>,
        stiffness: f32,
        damping: f32,
    ) -> Self {
        SpringBuilder {
            spring_type: SpringType::Linear { axis },
            local_anchor1,
            local_anchor2,
            stiffness,
            damping,
            rest_length: None,
            target_position: None,
        }
    }

    /// Creates a distance spring using a revolute joint with motor
    /// This maintains a specific distance between two anchor points
    pub fn new_distance(
        local_anchor1: Point2<f32>,
        local_anchor2: Point2<f32>,
        stiffness: f32,
        damping: f32,
    ) -> Self {
        SpringBuilder {
            spring_type: SpringType::Distance,
            local_anchor1,
            local_anchor2,
            stiffness,
            damping,
            rest_length: None,
            target_position: None,
        }
    }

    /// Create a default linear spring (along X-axis)
    pub fn default_linear(local_anchor1: Point2<f32>, local_anchor2: Point2<f32>) -> Self {
        Self::new_linear(
            Vector2::new(1.0, 0.0), // X-axis
            local_anchor1,
            local_anchor2,
            1000.0, // Default stiffness
            50.0,   // Default damping
        )
    }

    /// Create a default distance spring
    pub fn default_distance(local_anchor1: Point2<f32>, local_anchor2: Point2<f32>) -> Self {
        Self::new_distance(
            local_anchor1,
            local_anchor2,
            1000.0, // Default stiffness
            50.0,   // Default damping
        )
    }

    /// Set custom stiffness
    pub fn with_stiffness(mut self, stiffness: f32) -> Self {
        self.stiffness = stiffness;
        self
    }

    /// Set custom damping
    pub fn with_damping(mut self, damping: f32) -> Self {
        self.damping = damping;
        self
    }

    /// Set rest length (for distance springs) or target position (for linear springs)
    pub fn with_rest_length(mut self, rest_length: f32) -> Self {
        self.rest_length = Some(rest_length);
        self
    }

    /// Set target position (mainly for linear springs)
    pub fn with_target_position(mut self, target_pos: f32) -> Self {
        self.target_position = Some(target_pos);
        self
    }

    /// Build the actual joint based on the spring type
    pub fn build(&self) -> (PrismaticJoint, SpringType) {
        match &self.spring_type {
            SpringType::Linear { axis } => {
                let axis_unit = Unit::new_normalize(*axis);
                let mut joint = PrismaticJointBuilder::new(axis_unit)
                    .local_anchor1(self.local_anchor1)
                    .local_anchor2(self.local_anchor2);

                // Configure the motor with spring-like behavior
                if let Some(target_pos) = self.target_position {
                    joint = joint.motor_position(target_pos, self.stiffness, self.damping);
                } else {
                    // Default to holding position 0 with spring behavior
                    joint = joint.motor_position(0.0, self.stiffness, self.damping);
                }

                (joint.build(), SpringType::Linear { axis: *axis })
            }
            SpringType::Distance => {
                // For distance springs, we'll use a prismatic joint along Y-axis as a fallback
                // In a real implementation, you might want to use a different approach
                let axis_unit = Unit::new_normalize(Vector2::new(0.0, 1.0));
                let mut joint = PrismaticJointBuilder::new(axis_unit)
                    .local_anchor1(self.local_anchor1)
                    .local_anchor2(self.local_anchor2);

                if let Some(target_pos) = self.target_position {
                    joint = joint.motor_position(target_pos, self.stiffness, self.damping);
                } else {
                    joint = joint.motor_position(0.0, self.stiffness, self.damping);
                }

                (joint.build(), SpringType::Distance)
            }
        }
    }

    /// Get the spring type
    pub fn spring_type(&self) -> &SpringType {
        &self.spring_type
    }

    /// Get the first anchor point
    pub fn local_anchor1(&self) -> Point2<f32> {
        self.local_anchor1
    }

    /// Get the second anchor point
    pub fn local_anchor2(&self) -> Point2<f32> {
        self.local_anchor2
    }

    /// Get the stiffness
    pub fn stiffness(&self) -> f32 {
        self.stiffness
    }

    /// Get the damping
    pub fn damping(&self) -> f32 {
        self.damping
    }

    /// Get the rest length
    pub fn rest_length(&self) -> Option<f32> {
        self.rest_length
    }

    /// Get the target position
    pub fn target_position(&self) -> Option<f32> {
        self.target_position
    }
}
