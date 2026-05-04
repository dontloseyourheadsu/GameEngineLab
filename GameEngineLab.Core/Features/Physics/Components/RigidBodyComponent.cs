using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.Physics.Components;

public enum RigidBodyShape
{
    Circle,
    Rectangle,
    Polygon
}

public struct RigidBodyComponent : IComponent
{
    public RigidBodyShape Shape { get; set; } = RigidBodyShape.Circle;

    /// <summary>
    /// How much energy is kept after a collision (0 to 1).
    /// </summary>
    public float Restitution { get; set; } = 0.8f;

    /// <summary>
    /// Linear friction applied every frame.
    /// </summary>
    public float Friction { get; set; } = 0.98f;

    /// <summary>
    /// Angular damping applied every frame.
    /// </summary>
    public float AngularDamping { get; set; } = 0.95f;

    /// <summary>
    /// The radius for boundary collisions (used if Shape is Circle).
    /// </summary>
    public float BoundingRadius { get; set; } = 10f;

    /// <summary>
    /// The size for boundary collisions (used if Shape is Rectangle).
    /// </summary>
    public Vector2 Size { get; set; } = new Vector2(20, 20);

    /// <summary>
    /// Mass of the body. 0 means infinite mass (static).
    /// </summary>
    public float Mass { get; set; } = 1.0f;

    /// <summary>
    /// Collision filtering group.
    /// </summary>
    public int CollisionGroup { get; set; } = 0;

    /// <summary>
    /// Bitmask of groups this body can collide with. -1 for all.
    /// </summary>
    public int CollisionMask { get; set; } = -1;

    public RigidBodyComponent() { }
}
