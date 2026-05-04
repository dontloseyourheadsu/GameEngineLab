using GameEngineLab.Core.Features.Ecs.Components;
using GameEngineLab.Core.Features.Ecs.Entities;

namespace GameEngineLab.Core.Features.Physics.Components;

public struct DistanceSpringComponent : IComponent
{
    public EntityId EntityA;
    public EntityId EntityB;
    public float Stiffness;
    public float Damping;
    public float RestLength;

    public DistanceSpringComponent(EntityId a, EntityId b, float stiffness = 50f, float damping = 5f, float restLength = -1f)
    {
        EntityA = a;
        EntityB = b;
        Stiffness = stiffness;
        Damping = damping;
        RestLength = restLength;
    }
}
