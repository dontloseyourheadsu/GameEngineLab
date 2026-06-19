class_name StateMachine
extends Node

## StateMachine
## Generic Finite State Machine.
## Child nodes must extend the 'State' class. It routes lifecycle updates
## (input, process, physics_process) to the active state and handles transitions.

signal transitioned(state_name: String)

@export_category("State Settings")
## The state node that will run when the game starts.
@export var initial_state: State

# The active state of the machine
var current_state: State
# Map of state names to State nodes
var states: Dictionary = {}

func _ready() -> void:
	# Wait for parent owner node to be ready to ensure everything is initialized
	await owner.ready
	
	# Register all child states
	for child in get_children():
		if child is State:
			states[child.name.to_lower()] = child
			child.state_machine = this_state_machine()
			
	if initial_state:
		current_state = initial_state
	elif get_child_count() > 0 and get_child(0) is State:
		current_state = get_child(0) as State
		
	if current_state:
		current_state.enter()


func _unhandled_input(event: InputEvent) -> void:
	if current_state:
		current_state.handle_input(event)


func _process(delta: float) -> void:
	if current_state:
		current_state.update(delta)


func _physics_process(delta: float) -> void:
	if current_state:
		current_state.physics_update(delta)


## Transitions from the current state to the specified target state.
## Can pass custom data via the 'msg' dictionary.
func transition_to(target_state_name: String, msg := {}) -> void:
	var target_key = target_state_name.to_lower()
	if not states.has(target_key):
		push_error("StateMachine: State '%s' does not exist in machine '%s'." % [target_state_name, name])
		return
		
	if current_state:
		current_state.exit()
		
	current_state = states[target_key]
	current_state.enter(msg)
	transitioned.emit(target_state_name)


# Workaround helper for reference self inside gdscript static typing in some environments
func this_state_machine() -> StateMachine:
	return self
