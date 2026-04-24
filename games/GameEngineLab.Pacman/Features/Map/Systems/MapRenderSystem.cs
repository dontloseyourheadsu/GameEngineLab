using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.Pacman.Features.Map.Systems;

public sealed class MapRenderSystem : IGameSystem
{
    public int Order => 90;

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
        var tileSize = map.TileSize;

        var offsetX = (frameContext.Viewport.Width - map.Width * tileSize) / 2;
        var offsetY = (frameContext.Viewport.Height - map.Height * tileSize) / 2;

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.GetTile(x, y);
                var rect = new Rectangle(offsetX + x * tileSize, offsetY + y * tileSize, tileSize, tileSize);

                if (tile == '#')
                {
                    if (gpAssets.IsInitialized) sb.Draw(gpAssets.Wall, rect, Color.White);
                    else sb.Draw(frameContext.DebugPixel, rect, new Color(38, 70, 140));
                }
            }
        }

        if (!world.TryGetResource<CollectiblesResource>(out var collectibles) || collectibles is null)
        {
            return;
        }

        foreach (var tile in collectibles.Food)
        {
            var rect = new Rectangle(offsetX + tile.X * tileSize, offsetY + tile.Y * tileSize, tileSize, tileSize);
            if (gpAssets.IsInitialized) sb.Draw(gpAssets.Food, rect, Color.White);
            else {
                var dotSize = Math.Max(2, tileSize / 8);
                var dot = new Rectangle(rect.Center.X - dotSize / 2, rect.Center.Y - dotSize / 2, dotSize, dotSize);
                sb.Draw(frameContext.DebugPixel, dot, new Color(255, 224, 170));
            }
        }

        foreach (var tile in collectibles.Pills)
        {
            var rect = new Rectangle(offsetX + tile.X * tileSize, offsetY + tile.Y * tileSize, tileSize, tileSize);
            if (gpAssets.IsInitialized) sb.Draw(gpAssets.Pill, rect, Color.White);
            else {
                var pillSize = Math.Max(4, tileSize / 4);
                var pill = new Rectangle(rect.Center.X - pillSize / 2, rect.Center.Y - pillSize / 2, pillSize, pillSize);
                sb.Draw(frameContext.DebugPixel, pill, new Color(250, 250, 250));
            }
        }
    }
}
