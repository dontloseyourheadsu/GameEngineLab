# GameEngineLab: ShadowHell (`GameEngineLab.ShadowHell`)

An experimental 2D rogue-lite action prototype built using the **C# ECS Core** library. Designed with mobile Android experiences in mind and using a desktop runner for quick testing.

---

## Technical & Art Implementations

* **Contrast Art Style (Light vs Shadow)**: Rendered entirely through code with zero external sprite textures:
  - **Player (Angel of Light)**: Built with a 14-bone skeleton drawn in glowing white, angelic light-blue, and a floating golden halo ring.
  - **Enemies (Shadow Blobs)**: Solid black silhouette blobs that squish and stretch procedurally as they crawl, carrying a glowing red core eye.
  - **Environment**: Jagged rocky cave walls, circular stone boulders, and pillar colliders drawn in solid black silhouette.
  - **Background Details**: Parallax scrolling jagged stalactites/stalagmites and translucent orange lava pools glowing on the cavern floor, with floating ember particles.
* **Top-down Roguelike Movement (Isaac Style)**: Players walk freely on the ground plane in all directions, dodging around obstacles and boundaries.
* **Procedural Forward Kinematics (FK)**: Core skeletal animation system that translates parent-child transform trees into real-time joint positions and angles.
* **Procedural Animations**:
  - **Idle**: Gentle breathing bobbing of the torso and slower, organic wing flapping.
  - **Walking**: Alternating leg swings (walking cycle), body bobbing at double the speed, and accelerated wing flaps.
  - **Jumping**: Tucked legs and deep vertical flaps of the inner and outer wing bones, rendering the character raised above the physical ground plane.
  - **Dodge Rolling**: Rapid spin rotation, folding wings and limbs tight to the torso to evade collisions.

---

## How it works (ECS Pipeline)

1. **Entities**:
   - `Player`: Transform, Velocity, RigidBody (Circle), PlayerComponent, SkeletonComponent (defining 14 parent-child bones).
   - `Enemy`: Transform, Velocity, RigidBody (Circle), EnemyComponent (tracking AI target).
   - `Cavern Walls & Pillars`: Static black colliders (Circles or Rectangles) mapping the physical boundary.
2. **Systems**:
   - `AtmosphereSystem` (Order = 2): Updates/renders background parallax spikes, lava pools, and floating embers.
   - `PlayerInputSystem` (Order = 1): Maps WASD keys to directional force, Space to Jump, and Left Shift to Dodge Roll.
   - `PlayerAnimationSystem` (Order = 50): Manipulates bone local variables (angles/offsets) in response to current player states.
   - `SkeletonSystem` (Order = 90): Computes FK translations (from local to world coords) and draws the bones.
   - `EnemySystem` (Order = 101): Tracks player direction, applies squish-and-stretch crawl velocity, and draws glowing eyes.
   - `PhysicsStepperSystem` (Order = 10): Ticks the sub-stepped physics pipeline (Movement, Collision, Boundary, and Friction).
   - `ShapeRenderSystem` (Order = 100): Draws the solid black boundaries and stone pillar obstacles.
