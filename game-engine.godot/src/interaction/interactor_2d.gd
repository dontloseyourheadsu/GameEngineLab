class_name Interactor2D
extends Area2D

## Interactor2D
## Area2D-based component attached to the Player/Actor that triggers interactions
## with Interactable2D objects.

@export_category("Input Settings")
## The input map action name configured for interaction (default: 'ui_accept' / Space or Enter)
@export var action_name: String = "ui_accept"

var overlapping_interactables: Array[Interactable2D] = []
var focused_interactable: Interactable2D = null

func _ready() -> void:
	# Connect collision signals to manage tracking
	area_entered.connect(_on_area_entered)
	area_exited.connect(_on_area_exited)


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed(action_name):
		if focused_interactable and focused_interactable.is_active:
			focused_interactable.interact(this_interactor())
			get_viewport().set_input_as_handled()


func _physics_process(_delta: float) -> void:
	_update_focus()


## Scans overlapping areas and focuses the closest active interactable.
func _update_focus() -> void:
	var closest_interactable: Interactable2D = null
	var min_distance := INF
	
	# Clean up any freed nodes and find the closest active interactable
	var active_list: Array[Interactable2D] = []
	for area in overlapping_interactables:
		if is_instance_valid(area) and area.is_active:
			active_list.append(area)
			var dist := global_position.distance_to(area.global_position)
			if dist < min_distance:
				min_distance = dist
				closest_interactable = area
				
	overlapping_interactables = active_list
	
	# Transition focus if the targeted interactable has changed
	if closest_interactable != focused_interactable:
		if focused_interactable:
			focused_interactable.unfocus(this_interactor())
			# Notify EventBus to hide prompt
			if is_inside_tree() and ProjectSettings.has_setting("autoload/EventBus") or has_node("/root/EventBus"):
				get_node("/root/EventBus").interact_prompt_hidden.emit()
				
		focused_interactable = closest_interactable
		
		if focused_interactable:
			focused_interactable.focus(this_interactor())
			# Notify EventBus to show prompt
			if is_inside_tree() and ProjectSettings.has_setting("autoload/EventBus") or has_node("/root/EventBus"):
				get_node("/root/EventBus").interact_prompt_shown.emit(focused_interactable.prompt_message)


func _on_area_entered(area: Area2D) -> void:
	if area is Interactable2D:
		overlapping_interactables.append(area)


func _on_area_exited(area: Area2D) -> void:
	if area is Interactable2D:
		overlapping_interactables.erase(area)


# Helper for type safety inside GDScript self references
func this_interactor() -> Interactor2D:
	return self
