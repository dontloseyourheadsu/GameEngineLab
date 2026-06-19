# Game Engine Laboratory (GameEngineLab)

Welcome to the **Game Engine Laboratory** workspace. This repository acts as a pair of experimental spaces for exploring game engine architectures, custom graphics shaders, physics integrations, and game loops.

The workspace is divided into two distinct engine paradigms:

1. **The C# ECS Lab**: A custom Entity-Component-System (ECS) engine built in C# (MonoGame/XNA).
2. **The Godot GDScript Lab**: A feature-based (Node Composition) engine built in Godot 4.6 (GDScript).

---

## Workspace Structure

```text
GameEngineLab/
├── GameEngineLab.Core/       # C# ECS Library Core
├── games/                    # Games built with the C# ECS Core
│   ├── GameEngineLab.Pacman/ # Custom 2D grid Pacman clone
│   └── GameEngineLab.GolfIt/ # 2D physics-based mini-golf prototype
│
├── game-engine.godot/        # Godot 4.6 Engine Library & Projects
│   ├── src/                  # Godot GDScript Reusable Library Core
│   └── games/                # Games built with the Godot Engine Library
│       └── dimension_hopper/ # 2.5D dimension-hopping crafting prototype
```

---

## Parading Engine Approaches

### 1. C# Custom ECS Lab
- **Architecture**: Strict Entity-Component-System (ECS). Data is stored in flat structs (Components) and logic is executed linearly in updates loops (Systems).
- **Core Library**: [GameEngineLab.Core](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/GameEngineLab.Core/README.md)
- **Included Games**:
  - [Pacman](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/games/GameEngineLab.Pacman/README.md): Demonstrates custom 2D grid maps and coordinate pathfinding.
  - [GolfIt](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/games/GameEngineLab.GolfIt/README.md): Showcases custom 2D physics engines, soft-bodies using spring meshes, and boundary systems.

### 2. Godot GDScript Lab
- **Architecture**: Feature-Based Node Composition. Functionality is encapsulated in reusable component nodes attached directly to entities inside Godot's scene tree (e.g. `HealthComponent`, `VelocityComponent`), integrating with Godot's built-in physics and rendering pipelines.
- **Core Library**: [game-engine.godot/src/](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/README.md)
- **Included Games**:
  - [Dimension Hopper](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/games/dimension_hopper/README.md): A 2.5D top-down / isometric adventure featuring multiversal portals and time dilation controls.
