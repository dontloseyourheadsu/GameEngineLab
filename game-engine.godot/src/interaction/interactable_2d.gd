class_name Interactable2D
extends Area2D

## Interactable2D
## Area2D-based component attached to objects in the world that can be interacted with.
## Works in tandem with Interactor2D on the player.

signal focused(interactor: Interactor2D)
signal unfocused(interactor: Interactor2D)
signal interacted(interactor: Interactor2D)

@export_category("Interaction Settings")
## The text prompt shown to the user when looking at this object.
@export var prompt_message: String = "Interact"
## Whether this object is currently available to be interacted with.
@export var is_active: bool = true

func _ready() -> void:
	# Ensure correct collision layers/masks are configured in the editor.
	# Standardizing on layer 3 or similar for interactables is common.
	pass


## Called by the Interactor2D when this object is targeted/highlighted.
func focus(interactor: Interactor2D) -> void:
	if not is_active:
		return
	focused.emit(interactor)


## Called by the Interactor2D when this object is no longer targeted.
func unfocus(interactor: Interactor2D) -> void:
	unfocused.emit(interactor)


## Called by the Interactor2D when the player presses the interact key.
func interact(interactor: Interactor2D) -> void:
	if not is_active:
		return
	interacted.emit(interactor)
