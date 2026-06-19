extends Node

## TimeManager
## Handles global time scale manipulation (time dilation / bullet time).
## Can be registered as a global Autoload to make time slowdown simple and reusable.

signal time_scale_changed(new_scale: float)
signal time_dilation_started(target_scale: float)
signal time_dilation_ended

# Current active time scale (tracked separately from Engine.time_scale for interpolation)
var current_scale: float = 1.0

# Active Tween for transitioning time scale smoothly
var _time_tween: Tween

func _ready() -> void:
	current_scale = Engine.time_scale


## Smoothly interpolates Engine.time_scale to a target scale.
## [param target_scale] The scale to transition to (e.g. 0.05 for 5% speed).
## [param duration] The transition time in seconds.
func transition_time_scale(target_scale: float, duration: float) -> void:
	if _time_tween:
		_time_tween.kill()
		
	_time_tween = create_tween()
	_time_tween.set_ignore_time_scale(true)
	_time_tween.set_ease(Tween.EASE_OUT)
	_time_tween.set_trans(Tween.TRANS_CUBIC)
	# Target the global Engine time scale
	_time_tween.tween_property(Engine, "time_scale", target_scale, duration)
	_time_tween.parallel().tween_property(self, "current_scale", target_scale, duration)
	
	if target_scale < 1.0:
		time_dilation_started.emit(target_scale)
	else:
		time_dilation_ended.emit()
		
	_time_tween.finished.connect(func(): time_scale_changed.emit(current_scale))


## Instantly sets the Engine time scale.
func set_time_scale_instantly(target_scale: float) -> void:
	if _time_tween:
		_time_tween.kill()
	Engine.time_scale = target_scale
	current_scale = target_scale
	time_scale_changed.emit(current_scale)
	
	if target_scale < 1.0:
		time_dilation_started.emit(target_scale)
	elif target_scale == 1.0:
		time_dilation_ended.emit()


## Reset time scale back to standard speed (1.0).
func reset_time_scale(duration: float = 0.3) -> void:
	transition_time_scale(1.0, duration)


## Returns the multiplier required to keep an actor moving at normal speed.
## Example: if time is at 10% (0.1), speed must be multiplied by 10 to feel normal.
func get_normal_speed_multiplier() -> float:
	if Engine.time_scale > 0.0:
		return 1.0 / Engine.time_scale
	return 1.0


## Calculates adjusted delta time to ignore global time dilation for specific calculations.
func get_unscaled_delta(scaled_delta: float) -> float:
	if Engine.time_scale > 0.0:
		return scaled_delta / Engine.time_scale
	return scaled_delta
