using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Ball.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.GolfIt.Features.Rendering.Systems;

public sealed class BallRenderSystem : IGameSystem
{
    public int Order => 100;

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        foreach (var entityId in world.GetEntitiesWith<BallComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);

            ShapeRenderer.DrawCircle(
                frameContext.SpriteBatch,
                frameContext.DebugPixel,
                transform.Position,
                body.BoundingRadius,
                Color.White);
        }
    }
}
