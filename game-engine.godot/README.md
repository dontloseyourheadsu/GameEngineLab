# Godot Game Engine Lab Core (`game-engine.godot`)

This directory houses the **Godot Game Engine Lab Core**, a collection of reusable GDScript components, shaders, and visual effects built for Godot 4.6.

Rather than forcing an ECS architecture, this library operates around a **Feature-Based (Node Composition)** pattern, composing behaviors natively within Godot's scene tree.

---

## Library Components (`src/`)

### 1. Common Utilities (`src/common/`)
* [time_manager.gd](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/common/time_manager.gd) — Singleton controlling global time scale dilation (bullet time) with real-time Tweens.
* [event_bus.gd](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/common/event_bus.gd) — Centralized event router to dispatch global signals.
* [raycast_math.gd](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/common/raycast_math.gd) — Port of custom 2D raycasting math calculations (lines, polygons, circles).

### 2. Shaders (`src/shaders/`)
* [stylized_3d.gdshader](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/shaders/stylized_3d/stylized_3d.gdshader) — Spatial shader featuring stepped cel-shading, customizable halftone screen-space shadow patterns, and masked fresnel rim lighting.

### 3. Reusable VFX Modules (`src/vfx/`)
* **Portal VFX (`src/vfx/portal/`)**:
  - [portal_vfx.tscn](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/vfx/portal/portal_vfx.tscn) — Fully configured portal particle meshes.
  - [portal_controller.gd](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/vfx/portal/portal_controller.gd) — Automates runtime parameters, setup of detection `Area3D` components, and teleportation logic.

### 4. Actor Components (`src/components/`)
* [health_component.gd](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/components/health_component.gd) — Manages hp, heals, shielding, and invulnerability timer loops.
* [velocity_component.gd](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/components/velocity_component.gd) — Standard physics velocity helper to apply acceleration, friction, and gravity forces.

---

## Associated Games
This engine core is used by the following game projects located inside the local `games/` subdirectory:
* [Dimension Hopper](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/games/dimension_hopper/README.md)
