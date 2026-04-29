using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;

namespace GameEngineLab.GolfIt.Features.Physics.Systems;

public sealed class MovementSystem : IGameSystem
{
    public int Order => 10;

    public void Update(World world, FrameContext frameContext)
    {
        foreach (var entityId in world.GetEntitiesWith<TransformComponent, VelocityComponent>())
        {
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);

            transform.Position += velocity.Value * frameContext.DeltaSeconds;
            
            world.SetComponent(entityId, transform);
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
