class_name HealthComponent
extends Node

## HealthComponent
## Manages health values, damage mitigation, healing, and invulnerability.
## Attach this as a child to any entity that requires health tracking.

signal health_changed(current: float, max_value: float)
signal damage_received(amount: float)
signal healed(amount: float)
signal died

@export_category("Health Settings")
@export var max_health: float = 100.0
@export var starting_health: float = 100.0

@export_category("Invulnerability Settings")
@export var has_i_frames: bool = false
@export var i_frame_duration: float = 0.5

var current_health: float
var is_invulnerable: bool = false

func _ready() -> void:
	current_health = clampf(starting_health, 0.0, max_health)
	health_changed.emit(current_health, max_health)


## Applies damage to the health pool. Emits relevant signals.
func damage(amount: float) -> void:
	if amount <= 0.0 or is_invulnerable:
		return
		
	current_health -= amount
	damage_received.emit(amount)
	
	if current_health <= 0.0:
		current_health = 0.0
		health_changed.emit(current_health, max_health)
		died.emit()
	else:
		health_changed.emit(current_health, max_health)
		if has_i_frames:
			trigger_i_frames()


## Restores health to the health pool up to max_health.
func heal(amount: float) -> void:
	if amount <= 0.0 or current_health <= 0.0:
		return
		
	current_health = minf(current_health + amount, max_health)
	healed.emit(amount)
	health_changed.emit(current_health, max_health)


## Triggers invulnerability frames using a scene tree timer.
func trigger_i_frames() -> void:
	is_invulnerable = true
	await get_tree().create_timer(i_frame_duration).timeout
	is_invulnerable = false


## Helper to check if the entity is dead.
func is_dead() -> bool:
	return current_health <= 0.0
