# GameEngineLab: Pacman (`GameEngineLab.Pacman`)

An experimental 2D grid-based arcade clone built using the **C# ECS Core** library.

---

## Technical Implementations

* **Grid Movement**: Translates pixel inputs to grid-aligned coordinate moves.
* **Map Loading**: Parses 2D tilemaps and checks grid bounds to block movements through walls.
* **AI Pathfinding**: Basic direction-selection routines for enemy ghosts traversing intersection choices.
* **Pellet Collection**: Employs bounding circle overlap detection between Pacman and pellet entities.

---

## How it works (ECS Pipeline)

1. **Entities**:
   - `Player`: Transform, Velocity, Input, Sprite.
   - `Ghost`: Transform, AIBehavior, Sprite.
   - `Pellet`: Transform, CollisionShape, ScoreValue.
2. **Systems**:
   - `InputSystem`: Collects keyboard arrows.
   - `MovementSystem`: Updates coordinates.
   - `CollisionSystem`: Triggers pellet eat events, increments score.
   - `ShapeRenderSystem`: Renders grid meshes.
