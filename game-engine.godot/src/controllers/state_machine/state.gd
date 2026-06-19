class_name State
extends Node

## State
## Base class for all states in the StateMachine.
## Create specific states (e.g., IdleState, RunState) by extending this class.

# Typed reference to the state machine this state belongs to.
# Assigned automatically by the StateMachine on ready.
var state_machine: StateMachine = null


## Virtual function called when entering the state.
## Pass optional data in the 'msg' dictionary.
func enter(_msg := {}) -> void:
	pass


## Virtual function called when exiting the state before transitioning to another state.
func exit() -> void:
	pass


## Virtual function for _unhandled_input logic.
func handle_input(_event: InputEvent) -> void:
	pass


## Virtual function for _process logic.
func update(_delta: float) -> void:
	pass


## Virtual function for _physics_process logic.
func physics_update(_delta: float) -> void:
	pass
