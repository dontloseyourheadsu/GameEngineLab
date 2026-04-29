using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
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

        foreach (var entityId in world.GetEntitiesWith<BallComponent, TransformComponent>())
        {
            world.TryGetComponent<BallComponent>(entityId, out var ball);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);

            var size = (int)(ball.Radius * 2);
            var rect = new Rectangle(
                (int)(transform.Position.X - ball.Radius),
                (int)(transform.Position.Y - ball.Radius),
                size,
                size);

            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, rect, Color.White);
        }
    }
}
