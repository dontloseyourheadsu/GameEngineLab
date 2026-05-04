using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.Physics.Systems;

public sealed class PhysicsFrictionSystem : IGameSystem
{
    public int Order => 30;

    private const float StopThreshold = 5f;

    public void Update(World world, FrameContext frameContext)
    {
        foreach (var entityId in world.GetEntitiesWith<RigidBodyComponent, VelocityComponent>())
        {
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);

            if (velocity.Value.LengthSquared() > 0)
            {
                velocity.Value *= body.Friction;

                if (velocity.Value.Length() < StopThreshold)
                {
                    velocity.Value = Vector2.Zero;
                }

                world.SetComponent(entityId, velocity);
            }

            // Apply angular damping
            if (world.TryGetComponent<TransformComponent>(entityId, out var transform))
            {
                // Note: We don't have an explicit AngularVelocityComponent yet,
                // but we can simulate simple rotation decay if it's being updated elsewhere.
                // Or better, we could add AngularVelocityComponent.
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
