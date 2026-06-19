# C# ECS Game Engine Lab Core (`GameEngineLab.Core`)

This directory houses the **C# ECS Game Engine Lab Core**, a custom game engine library built around the **Entity-Component-System (ECS)** architecture paradigm using MonoGame/XNA.

---

## Architectural Features

* **Entity Manager (`World`)**: Manages the life cycle of entities (simple integer IDs) and maps components to entities.
* **Component Storage**: Data is represented as flat, pure structures (`IComponent`) stored contiguously to optimize CPU cache lines.
* **System Scheduler**: Processes game systems (`IGameSystem`) in a deterministic pipeline, query-filtering entities matching specific component profiles.

---

## Subsystem Details

### 1. Physics Engine
- Custom 2D rigid-body collision, impulse resolution, friction, and integration.
- **Spring Physics**: Distance-based springs (`SpringComponent`) to simulate constraints and trusses.
- **Soft Body Simulation**: A soft-body circle factory (`SoftBodyFactory.cs`) that constructs point masses connected by networks of cross-springs.
- **Mathematical Raycasting**: Custom 2D mathematical ray-line, ray-circle, and ray-polygon intersection calculations (`Raycast.cs`).

### 2. Networking & Peer Discovery
- **Discovery Service**: Local network broadcast/multicast scanning to automatically discover hosting local game instances.
- **UDP Transport**: Lightweight, asynchronous UDP networking wrapper (`SimpleUdpTransport.cs`) to sync packets across clients.

### 3. Rendering & UI
- **Shape Rendering**: Custom color palette-based pixel shape drawers (`ShapeRenderer.cs`).
- **UI Toolkit**: Dynamic UI container builder (`UiBuilder.cs`) featuring keyboard focus, interactive buttons, scrolling lists, and layout panels.

---

## Associated Games
This engine core is used by the following game projects located in the root `/games/` directory:
* [Pacman](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/games/GameEngineLab.Pacman/README.md)
* [GolfIt](file:///home/dontloseyourheadsu/Documents/GitHub/GameEngineLab/games/GameEngineLab.GolfIt/README.md)
