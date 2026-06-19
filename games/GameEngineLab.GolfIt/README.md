# GameEngineLab: GolfIt (`GameEngineLab.GolfIt`)

An experimental 2D physics-based mini-golf prototype built using the **C# ECS Core** library.

---

## Technical Implementations

* **Drag-to-Aim Physics**: Applies mouse-drag offsets as instantaneous impulses to the ball entity.
* **Friction & Deceleration**: Employs ground friction systems to slowly dampen linear velocity.
* **Trigger Zones**: Defines pool boundaries and cup target zones using trigger shapes.
* **Soft Body Obstacles**: Demonstrates circular soft-bodies generated using connected spring meshes that deform and bounce upon impact.

---

## How it works (ECS Pipeline)

1. **Entities**:
   - `Ball`: Transform, Velocity, RigidBody (Circle), DragAim.
   - `Hole`: Transform, TriggerZone.
   - `Soft Obstacle`: Circular nodes connected by `DistanceSpring` entities.
2. **Systems**:
   - `PhysicsFrictionSystem`: Dampens speeds over time.
   - `SpringSystem`: Relaxes and contracts distance spring structures.
   - `CollisionSystem`: Resolves ball bounces off walls and soft body obstacles.
   - `ZoneSystem`: Detects when the ball settles inside the hole trigger bounds to win.
