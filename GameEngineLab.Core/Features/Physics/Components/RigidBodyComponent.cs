using GameEngineLab.Core.Features.Ecs.Components;

namespace GameEngineLab.Core.Features.Physics.Components;

public struct RigidBodyComponent : IComponent
{
    /// <summary>
    /// How much energy is kept after a collision (0 to 1).
    /// </summary>
    public float Restitution { get; set; } = 0.8f;

    /// <summary>
    /// Linear friction applied every frame.
    /// </summary>
    public float Friction { get; set; } = 0.98f;

    /// <summary>
    /// The radius for boundary collisions.
    /// </summary>
    public float BoundingRadius { get; set; } = 10f;

    public RigidBodyComponent() { }
}
