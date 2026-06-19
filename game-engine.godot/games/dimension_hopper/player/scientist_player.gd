class_name ScientistPlayer
extends CharacterBody3D

## ScientistPlayer
## 2.5D top-down / isometric movement controller (similar to Don't Starve Together).
## Moves freely on the X and Z ground plane, using Y for height and gravity.
## Integrates with TimeManager to activate Thinking Mode on holding Shift.

@export_category("Movement Tuning")
@export var speed: float = 7.0
@export var acceleration: float = 35.0
@export var friction: float = 45.0

@export_category("Controls")
@export var thinking_mode_time_scale: float = 0.25
@export var thinking_mode_transition_duration: float = 0.15

# Get default gravity from project settings (for drops/ledges)
var gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity", 28.0)

# Flag tracking if player is currently in Thinking Mode
var is_thinking_mode: bool = false


func _physics_process(delta: float) -> void:
	# 1. Update Thinking Mode state (Hold Shift)
	var is_shift_held := Input.is_key_pressed(KEY_SHIFT)
	if is_shift_held != is_thinking_mode:
		_set_thinking_mode(is_shift_held)

	# 2. Apply gravity if in mid-air
	if not is_on_floor():
		velocity.y -= gravity * delta
	else:
		velocity.y = 0.0

	# 3. Get ground movement input (horizontal X and vertical Z axes)
	var input_dir := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	
	# Project input onto the 3D ground plane (Z-axis is up/down on screen, X-axis is left/right)
	var direction := Vector3(input_dir.x, 0.0, input_dir.y)
	if direction.length_squared() > 0.0:
		direction = direction.normalized()

	if direction != Vector3.ZERO:
		# Smooth acceleration on horizontal ground coordinates (X and Z)
		velocity.x = move_toward(velocity.x, direction.x * speed, acceleration * delta)
		velocity.z = move_toward(velocity.z, direction.z * speed, acceleration * delta)
	else:
		# Smooth friction/deceleration on X and Z
		velocity.x = move_toward(velocity.x, 0.0, friction * delta)
		velocity.z = move_toward(velocity.z, 0.0, friction * delta)

	# 4. Apply movement (standard delta, player slows down too)
	move_and_slide()


func _set_thinking_mode(enable: bool) -> void:
	is_thinking_mode = enable
	
	if has_node("/root/TimeManager"):
		var time_manager = get_node("/root/TimeManager")
		if is_thinking_mode:
			time_manager.transition_time_scale(thinking_mode_time_scale, thinking_mode_transition_duration)
		else:
			time_manager.reset_time_scale(thinking_mode_transition_duration)
