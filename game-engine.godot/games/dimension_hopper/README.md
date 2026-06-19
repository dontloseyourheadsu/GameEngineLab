# Dimension Hopper (`games/dimension_hopper`)

A 2.5D top-down/isometric portal-hopping adventure prototype built inside the **Godot GDScript Lab**. 

The game follows a brilliant, cynical scientist traveling across bizarre parallel dimensions, collecting resources, and using a time-slowing **"Thinking Mode"** to craft solutions to hazards.

---

## Technical Implementations

* **2.5D Ground Navigation**: Characters and assets move freely on the horizontal 3D ground coordinates (X and Z axes) while respecting Y-axis gravity collisions.
* **Hold-Shift Thinking Mode**: Integrates with [TimeManager](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/common/time_manager.gd) to drop global game time scale down to `25%` speed when holding the **Shift** key, reverting instantly on release.
* **Portal Traversal**: Walk through the Blue or Green portals to teleport seamlessly between different locations in the test field, utilizing the runtime physics detection added in [portal_controller.gd](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/vfx/portal/portal_controller.gd).
* **Filmic Ambient Lighting**: Showcases custom environments using ACES tonemapping, Screen-Space Ambient Occlusion (SSAO) shadows, and glow bloom maps to make the portal emissions pop.

---

## Technical Architecture

* **Character Entity**: [scientist_player.gd](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/games/dimension_hopper/player/scientist_player.gd)
* **Test Field Scene**: [test_level.tscn](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/games/dimension_hopper/scenes/test_level.tscn)
* **Reorganized VFX**: [portal_vfx.tscn](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/game-engine.godot/src/vfx/portal/portal_vfx.tscn)

---

## How to Test
1. Open the project inside the Godot Editor.
2. Run the project to launch into the test field.
3. Move the crimson capsule with WASD or Arrow keys.
4. Walk into a portal to teleport.
5. Hold **Shift** to enter slow-motion Thinking Mode.
