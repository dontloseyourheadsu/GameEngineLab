using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Components;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngineLab.Pacman.Features.Ghosts.Systems;

public sealed class GhostRenderSystem : IGameSystem
{
    public int Order => 95;

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

        if (!world.TryGetResource<MapStateResource>(out var mapState) || mapState is null)
        {
            return;
        }

        var gpAssets = world.GetRequiredResource<GameplayAssetsResource>();
        var sb = frameContext.SpriteBatch;

        var map = mapState.Map;
        var offsetX = (frameContext.Viewport.Width - map.Width * map.TileSize) / 2;
        var offsetY = (frameContext.Viewport.Height - map.Height * map.TileSize) / 2;

        foreach (var entity in world.GetEntitiesWith<GhostComponent, TransformComponent>())
        {
            if (!world.TryGetComponent<GhostComponent>(entity, out var ghost)
                || !world.TryGetComponent<TransformComponent>(entity, out var transform))
            {
                continue;
            }

            var rect = new Rectangle(
                (int)(offsetX + transform.Position.X - ghost.Radius),
                (int)(offsetY + transform.Position.Y - ghost.Radius),
                (int)(ghost.Radius * 2f),
                (int)(ghost.Radius * 2f));

            if (gpAssets.IsInitialized && ghost.State != GhostState.Frightened && ghost.State != GhostState.Returning)
            {
                sb.Draw(gpAssets.Ghost, rect, Color.White);
            }
            else
            {
                var color = ghost.State switch
                {
                    GhostState.Frightened => new Color(86, 136, 255),
                    GhostState.Returning => new Color(190, 190, 190),
                    _ => ghost.Behavior switch
                    {
                        GhostBehavior.Blinky => new Color(255, 64, 64),
                        GhostBehavior.Pinky => new Color(255, 136, 190),
                        GhostBehavior.Inky => new Color(64, 232, 255),
                        GhostBehavior.Clyde => new Color(255, 170, 74),
                        _ => Color.Orange,
                    },
                };
                sb.Draw(frameContext.DebugPixel, rect, color);
            }
        }
    }
}
