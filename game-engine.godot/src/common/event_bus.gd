extends Node

## EventBus
## Centralized signal routing singleton (can be configured as an Autoload).
## Decouples systems by allowing them to communicate via global signals.

# --- GAME STATE SIGNALS ---
signal game_started
signal game_paused(paused: bool)
signal game_over

# --- PLAYER SIGNALS ---
signal player_spawned(player_node: Node)
signal player_died
signal player_health_changed(current: float, max_health: float)

# --- LEVEL & PROGRESS SIGNALS ---
signal level_loaded(level_name: String)
signal level_completed
signal score_updated(new_score: int)

# --- INTERACTION SIGNALS ---
signal interact_prompt_shown(text: String)
signal interact_prompt_hidden
