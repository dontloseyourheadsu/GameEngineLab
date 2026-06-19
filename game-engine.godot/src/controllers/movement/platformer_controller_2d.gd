class_name PlatformerController2D
extends CharacterBody2D

## PlatformerController2D
## An out-of-the-box 2D platformer character controller utilizing VelocityComponent.
## Implements quality-of-life mechanics like Coyote Time and Jump Buffering.

@export_category("Required Components")
## Sibling/Child velocity component node
@export var velocity_component: VelocityComponent

@export_category("Movement Tuning")
@export var speed: float = 250.0
@export var acceleration: float = 1200.0
@export var friction: float = 1400.0

@export_category("Jumping Settings")
@export var jump_force: float = -380.0
## Duration after leaving a platform where the player can still jump (seconds)
@export var coyote_duration: float = 0.15
## Duration before hitting the ground where a jump input is registered (seconds)
@export var jump_buffer_duration: float = 0.1
## Multiplier applied to Y velocity if player releases jump key early
@export var variable_jump_release_multiplier: float = 0.5

# Input tracking state
var _coyote_timer: float = 0.0
var _jump_buffer_timer: float = 0.0
var _was_on_floor: bool = false

func _ready() -> void:
	# Locate velocity component automatically if not assigned
	if not velocity_component:
		for child in get_children():
			if child is VelocityComponent:
				velocity_component = child
				break
				
	if velocity_component:
		velocity_component.max_speed = speed
		velocity_component.acceleration = acceleration
		velocity_component.friction = friction


func _physics_process(delta: float) -> void:
	if not velocity_component:
		return
		
	# 1. Update timers
	_coyote_timer -= delta
	_jump_buffer_timer -= delta
	
	if is_on_floor():
		_coyote_timer = coyote_duration
		_was_on_floor = true
	elif _was_on_floor and not is_on_floor():
		# Just walked off edge, keep coyote timer running
		_was_on_floor = false
		
	# 2. Get horizontal input direction
	var input_dir := Input.get_axis("ui_left", "ui_right")
	var move_dir := Vector2(input_dir, 0.0)
	
	# 3. Apply horizontal movement
	velocity_component.accelerate_in_direction(move_dir, delta)
	
	# 4. Apply gravity
	if not is_on_floor():
		velocity_component.apply_gravity(delta)
		
	# 5. Handle jumping input
	if Input.is_action_just_pressed("ui_up"):
		_jump_buffer_timer = jump_buffer_duration
		
	var can_jump := is_on_floor() or _coyote_timer > 0.0
	if _jump_buffer_timer > 0.0 and can_jump:
		_jump_buffer_timer = 0.0
		_coyote_timer = 0.0
		velocity_component.velocity.y = jump_force
		
	# 6. Variable jump height (releasing jump early cuts upward momentum)
	if Input.is_action_just_released("ui_up") and velocity_component.velocity.y < 0.0:
		velocity_component.velocity.y *= variable_jump_release_multiplier
		
	# 7. Apply movement to CharacterBody2D
	velocity_component.move(self)
