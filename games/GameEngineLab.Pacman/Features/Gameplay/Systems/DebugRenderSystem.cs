using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Components;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Gameplay.Systems;

public sealed class DebugRenderSystem : IGameSystem
{
    public int Order => 100;

    public void Update(World world, FrameContext frameContext)
    {
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch is null || frameContext.DebugPixel is null)
        {
            return;
        }

        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay)
        {
            return;
        }

        foreach (var entity in world.GetEntitiesWith<TransformComponent, PacmanPlayerComponent>())
        {
            if (!world.TryGetComponent<TransformComponent>(entity, out var transform) ||
                !world.TryGetComponent<PacmanPlayerComponent>(entity, out var pacman))
            {
                continue;
            }

            Rectangle rect = new(
                (int)(transform.Position.X - pacman.Radius),
                (int)(transform.Position.Y - pacman.Radius),
                (int)(pacman.Radius * 2f),
                (int)(pacman.Radius * 2f));

            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, rect, Color.Gold);
        }
    }
}
