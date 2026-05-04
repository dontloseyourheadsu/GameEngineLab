using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.GolfIt.Features.Physics.Systems;

public sealed class DemoRotationSystem : IGameSystem
{
    public int Order => 0;

    public void Update(World world, FrameContext frameContext)
    {
        foreach (var entityId in world.GetEntitiesWith<PolygonComponent, TransformComponent>())
        {
            // Only rotate entities that are intended to rotate (maybe tag them or check mass)
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            if (body.Mass == 0) // Static objects that rotate
            {
                world.TryGetComponent<TransformComponent>(entityId, out var transform);
                transform.Rotation += 1.0f * frameContext.DeltaSeconds;
                world.SetComponent(entityId, transform);
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
