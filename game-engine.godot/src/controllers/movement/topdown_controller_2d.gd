class_name TopDownController2D
extends CharacterBody2D

## TopDownController2D
## An out-of-the-box 2D top-down and isometric character controller utilizing VelocityComponent.
## Converts standard keyboard inputs into cardinal, ordinal, or isometric vectors.

@export_category("Required Components")
## Sibling/Child velocity component node
@export var velocity_component: VelocityComponent

@export_category("Movement Tuning")
@export var speed: float = 200.0
@export var acceleration: float = 1000.0
@export var friction: float = 1200.0

@export_category("Isometric Options")
## If enabled, input vectors are projected to match an isometric 2:1 ratio projection (45-degree rotation, half Y scale)
@export var use_isometric_projection: bool = false

func _ready() -> void:
	if not velocity_component:
		for child in get_children():
			if child is VelocityComponent:
				velocity_component = child
				break
				
	if velocity_component:
		velocity_component.max_speed = speed
		velocity_component.acceleration = acceleration
		velocity_component.friction = friction
		# Topdown games do not use gravity
		velocity_component.use_gravity = false


func _physics_process(delta: float) -> void:
	if not velocity_component:
		return
		
	# 1. Gather directional input
	var input_x := Input.get_axis("ui_left", "ui_right")
	var input_y := Input.get_axis("ui_up", "ui_down")
	var move_dir := Vector2(input_x, input_y)
	
	if move_dir.length_squared() > 0.0:
		move_dir = move_dir.normalized()
		
		# 2. Convert to isometric space if enabled
		if use_isometric_projection:
			move_dir = cartesian_to_isometric(move_dir).normalized()
			
	# 3. Apply movement calculation
	velocity_component.accelerate_in_direction(move_dir, delta)
	
	# 4. Apply movement to CharacterBody2D
	velocity_component.move(self)


## Helper function to convert Cartesian input vector (e.g. keyboard inputs)
## into standard 2D 2:1 isometric coordinate projection space.
func cartesian_to_isometric(cartesian_vector: Vector2) -> Vector2:
	var iso_vector := Vector2.ZERO
	# X-axis isometric projection
	iso_vector.x = cartesian_vector.x - cartesian_vector.y
	# Y-axis isometric projection (halved for 2:1 isometric aspect ratio)
	iso_vector.y = (cartesian_vector.x + cartesian_vector.y) * 0.5
	return iso_vector
