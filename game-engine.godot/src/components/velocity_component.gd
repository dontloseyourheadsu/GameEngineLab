class_name VelocityComponent
extends Node

## VelocityComponent
## Encapsulates kinematic calculations (acceleration, deceleration, gravity, friction)
## for CharacterBody2D nodes. Simplifies character controller physics.

@export_category("Speeds")
@export var max_speed: float = 200.0
@export var acceleration: float = 800.0
@export var friction: float = 1000.0

@export_category("Gravity")
@export var use_gravity: bool = true
@export var gravity_scale: float = 1.0

# Current calculated velocity
var velocity: Vector2 = Vector2.ZERO

# Reference to the project default gravity
var default_gravity: float = ProjectSettings.get_setting("physics/2d/default_gravity", 980.0)


## Accelerates in the specified input direction using linear interpolation or delta multiplication.
func accelerate_in_direction(direction: Vector2, delta: float) -> void:
	if direction != Vector2.ZERO:
		var target_vel = direction * max_speed
		velocity = velocity.move_toward(target_vel, acceleration * delta)
	else:
		apply_friction(delta)


## Applies deceleration (friction) towards zero velocity.
func apply_friction(delta: float) -> void:
	velocity = velocity.move_toward(Vector2.ZERO, friction * delta)


## Applies gravity to the Y-axis.
func apply_gravity(delta: float) -> void:
	if use_gravity:
		velocity.y += default_gravity * gravity_scale * delta


## Syncs the velocity of the component to the CharacterBody2D, calls move_and_slide,
## and retrieves the actual velocity post-collision back to the component.
func move(body: CharacterBody2D) -> void:
	body.velocity = velocity
	body.move_and_slide()
	velocity = body.velocity
