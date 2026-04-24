using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;

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

        var map = mapState.Map;
        var tileSize = map.TileSize;

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.GetTile(x, y);
                var rect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);

                Color color = tile switch
                {
                    '#' => new Color(38, 70, 140),
                    '.' => new Color(46, 52, 92),
                    'o' => new Color(66, 72, 112),
                    'S' => new Color(72, 56, 100),
                    _ => new Color(22, 26, 46),
                };

                frameContext.SpriteBatch.Draw(frameContext.DebugPixel, rect, color);
            }
        }

        if (!world.TryGetResource<CollectiblesResource>(out var collectibles) || collectibles is null)
        {
            return;
        }

        var dotSize = Math.Max(2, tileSize / 8);
        var pillSize = Math.Max(4, tileSize / 4);

        foreach (var tile in collectibles.Food)
        {
            var centerX = tile.X * tileSize + tileSize / 2;
            var centerY = tile.Y * tileSize + tileSize / 2;
            var dot = new Rectangle(centerX - dotSize / 2, centerY - dotSize / 2, dotSize, dotSize);
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, dot, new Color(255, 224, 170));
        }

        foreach (var tile in collectibles.Pills)
        {
            var centerX = tile.X * tileSize + tileSize / 2;
            var centerY = tile.Y * tileSize + tileSize / 2;
            var pill = new Rectangle(centerX - pillSize / 2, centerY - pillSize / 2, pillSize, pillSize);
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, pill, new Color(250, 250, 250));
        }
    }
}
