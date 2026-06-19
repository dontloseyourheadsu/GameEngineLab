class_name CameraController2D
extends Camera2D

## CameraController2D
## Custom Camera node that adds target tracking with smoothing, smooth zoom,
## look-ahead offset, and robust screen shake effects.

@export_category("Target Tracking")
## Node that the camera should follow (typically the Player)
@export var target: Node2D
## How smoothly the camera catches up to the target position
@export var follow_smoothness: float = 0.1
## Lead offset based on target's velocity or facing direction
@export var look_ahead_distance: float = 50.0
@export var look_ahead_smoothness: float = 0.05

@export_category("Zoom Settings")
@export var zoom_smoothness: float = 0.1
var target_zoom: Vector2 = Vector2.ONE

# Screen Shake Variables
var _shake_amount: float = 0.0
var _shake_decay: float = 0.0
var _shake_noise: FastNoiseLite = FastNoiseLite.new()
var _noise_y: float = 0.0

# Interpolated look-ahead position offset
var _look_ahead_offset: Vector2 = Vector2.ZERO

func _ready() -> void:
	target_zoom = zoom
	# Configure noise generator for organic-feeling screen shake
	_shake_noise.seed = randi()
	_shake_noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	_shake_noise.frequency = 0.5


func _physics_process(delta: float) -> void:
	if not target:
		return
		
	# 1. Calculate base target position
	var target_pos := target.global_position
	
	# 2. Add look-ahead offset if target has a velocity or direction
	var target_vel := Vector2.ZERO
	if "velocity" in target:
		target_vel = target.velocity
	elif "velocity_component" in target and target.velocity_component:
		target_vel = target.velocity_component.velocity
		
	if target_vel.length_squared() > 10.0:
		var desired_offset = target_vel.normalized() * look_ahead_distance
		_look_ahead_offset = _look_ahead_offset.lerp(desired_offset, look_ahead_smoothness)
	else:
		_look_ahead_offset = _look_ahead_offset.lerp(Vector2.ZERO, look_ahead_smoothness)
		
	# Combine target and look-ahead
	var destination := target_pos + _look_ahead_offset
	
	# 3. Smoothly move camera towards destination
	global_position = global_position.lerp(destination, follow_smoothness)
	
	# 4. Smoothly interpolate zoom
	zoom = zoom.lerp(target_zoom, zoom_smoothness)
	
	# 5. Process screen shake
	if _shake_amount > 0.0:
		_shake_amount = maxf(_shake_amount - _shake_decay * delta, 0.0)
		_apply_shake(delta)
	else:
		offset = Vector2.ZERO


## Triggers a screen shake event.
## Intensity is the peak displacement in pixels.
## Duration is how long the shake lasts before completely decaying.
func shake(intensity: float, duration: float) -> void:
	if intensity > _shake_amount:
		_shake_amount = intensity
		# Decay speed = intensity / duration so it ends exactly on time
		_shake_decay = intensity / duration


## Internal method to apply displacement to the camera offset using Noise.
func _apply_shake(delta: float) -> void:
	_noise_y += delta * 100.0 # Speed of shake oscillation
	var shake_offset := Vector2(
		_shake_noise.get_noise_2d(_noise_y, 0.0) * _shake_amount,
		_shake_noise.get_noise_2d(0.0, _noise_y) * _shake_amount
	)
	offset = shake_offset


## Sets a new target zoom multiplier.
func set_target_zoom(zoom_factor: float) -> void:
	target_zoom = Vector2(zoom_factor, zoom_factor)
