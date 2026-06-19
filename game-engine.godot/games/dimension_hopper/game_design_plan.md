# Game Design Plan: "Dimension Hopper"

**Vibe**: A cynical, hyper-intelligent, multiversal scientific adventure. You play as a brilliant scientist hopping between bizarre dimensions, scanning anomalies, collecting materials, and using a time-slowing **"Thinking Mode"** to craft solutions to puzzles and hazards on the fly.

---

## 1. Core Mechanics

### A. Dimensional Portals (2.5D World)
- The game is played on a **2.5D plane** (3D environment, 2D movement plane).
- The player carries a **Portal Gun** that utilizes the visual effects in `src/vfx/portal/portal_vfx.tscn`.
- Shooting a portal opens a rift to a parallel dimension:
  - **Dimension Alpha (Normal)**: Standard platforms, gravity, and ruins.
  - **Dimension Beta (Acid Swamp)**: Corrosive atmospheres, rising toxic fluids, but has floating organic resource nodes.
  - **Dimension Gamma (High Gravity / Metallic)**: Heavy objects fall faster, jump height is halved, but contains rich metal deposits.
  - **Dimension Delta (Microverse)**: Platforms are giant scale; player can squeeze through narrow gaps, but wind currents are highly volatile.
- Portals don't just teleport you from point A to B; they act as a **lens/window** into the other dimension. Walking through it seamlessly changes your active physical layer.

### B. "Thinking Mode" (Active Time Dilation)
- **Concept**: A genius doesn't panic; they slow down time and calculate.
- When activated, global time slows down to 5-10% speed (`Engine.time_scale = 0.05`).
- The camera zooms in slightly, and a stylized grid overlays the viewport (using our stylized 3D shader's pattern inputs!).
- In this mode, the player's cognitive abilities are represented by a **Building/Crafting Grid**:
  - You can drag-and-drop components from your inventory to construct gadgets directly in the environment (e.g., placing a bridge, building a gravity fan, or setting up a laser refractor).
  - You can wire up connections (e.g., connecting a battery node to a portal anchor to power a gate).
- Exit Thinking Mode to watch your creations react in real-time.

### C. Resource Collection & Crafting
- **Collectibles**: Scientific junk, dark matter, unstable isotopes, microverse cells, scrap metal.
- **Inventory & Crafting Blueprints**:
  - *Tier 1 (Utilities)*: Portable platform, spring pad, gravity-inversion boot charge.
  - *Tier 2 (Portal Mods)*: Stencil lens (allows seeing hidden secrets), Portal magnet (pulls objects through portals).
  - *Tier 3 (Defense/Puzzle Tools)*: Dilation anchor (freezes local objects in time even when Thinking Mode is off).

---

## 2. Procedural Dimension Generation

To create a feeling of discovering the infinite multiverse:
- Levels are procedurally generated using seed-based algorithms.
- Each dimension is made of **modular level chunks** (e.g., entry chunk, hazard chunk, resource chunk, portal gate chunk).
- The chunks generated change depending on the **Dimension Vibe** (e.g., swamp tiles vs. industrial metal tiles).
- Randomly placed portal anomalies spawn in levels, leading to micro-dungeons or treasure rooms.

---

## 3. Project File Structure (Library vs. Game)

To keep code reusable across other games, we maintain a strict boundary between library nodes (generic) and game-specific nodes:

### Reusable Library (`src/`)
- `src/common/raycast_math.gd` (pure math collision queries)
- `src/common/time_manager.gd` (reusable time dilation / slowdown controller)
- `src/shaders/stylized_3d/stylized_3d.gdshader` (stylized 3D rendering)
- `src/vfx/portal/` (portal rendering shaders, controller, and particle setups)
- `src/components/health_component.gd`
- `src/components/velocity_component.gd`
- `src/controllers/state_machine/`

### Game-Specific (`games/dimension_hopper/`)
- `games/dimension_hopper/player/`
  - `scientist_player.gd` (character logic, input)
  - `thinking_mode_controller.gd` (manages the building grid and UI overlays)
- `games/dimension_hopper/dimension_generation/`
  - `dimension_generator.gd` (handles procedural generation of level chunks)
  - `dimension_data.gd` (defines specs for swamp, metal, microverse dimensions)
- `games/dimension_hopper/crafting/`
  - `inventory.gd` (inventory collection)
  - `blueprint_database.gd` (available craftables)
- `games/dimension_hopper/scenes/` (level loops, main menu, dimension nodes)

---

## 4. Implementation Phase Plan

1. **Phase 1: Time Dilation & Portals Verification**
   - Create a reusable `TimeManager` in `src/`.
   - Setup a basic 3D testing scene in `games/dimension_hopper/` containing a player, some test shapes using the stylized shader, and a portal from `src/vfx/portal/portal_vfx.tscn`.
2. **Phase 2: "Thinking Mode" Overlay & Placement**
   - Build the Thinking Mode trigger and UI placement grid.
   - Implement spawn-and-place logic for basic objects (like a bridge or spring).
3. **Phase 3: 2.5D Movement & Portals Layer Swap**
   - Refactor character movement to lock along the Z-axis (2.5D plane).
   - Implement the physics layer swap when traversing portals.
4. **Phase 4: Procedural Generation & Polish**
   - Create chunk loader and generator.
   - Add crafting loop and inventory.
