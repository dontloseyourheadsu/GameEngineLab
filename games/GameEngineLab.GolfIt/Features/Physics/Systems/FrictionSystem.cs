using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.GolfIt.Features.Physics.Systems;

public sealed class FrictionSystem : IGameSystem
{
    public int Order => 30;

    private const float Friction = 0.98f;
    private const float StopThreshold = 5f;

    public void Update(World world, FrameContext frameContext)
    {
        foreach (var entityId in world.GetEntitiesWith<VelocityComponent>())
        {
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);

            if (velocity.Value.LengthSquared() > 0)
            {
                velocity.Value *= Friction;

                if (velocity.Value.Length() < StopThreshold)
                {
                    velocity.Value = Vector2.Zero;
                }

                world.SetComponent(entityId, velocity);
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
