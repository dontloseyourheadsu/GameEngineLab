using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Rendering.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.GolfIt.Features.Rendering.Systems;

public sealed class FloorRenderSystem : IGameSystem
{
    public int Order => -10; // Draw before everything else

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;
        if (!world.TryGetResource<MapBoundsResource>(out var bounds) || bounds == null) return;
        
        var tileSize = 100;
        var playArea = bounds.PlayArea;

        // Base color (Light Green)
        var lightGreen = new Color(0x2d, 0x4a, 0x1b); // Cozy Root BG or similar
        var darkGreen = new Color(0x24, 0x3b, 0x15); // Darker variant

        if (world.TryGetResource<PaletteLibraryResource>(out var library) && library != null)
        {
            lightGreen = library.Cozy.GetColor(0); 
            // Create a darker variant of lightGreen
            darkGreen = new Color(
                (int)(lightGreen.R * 0.8f),
                (int)(lightGreen.G * 0.8f),
                (int)(lightGreen.B * 0.8f));
        }

        for (int y = playArea.Top; y < playArea.Bottom; y += tileSize)
        {
            for (int x = playArea.Left; x < playArea.Right; x += tileSize)
            {
                bool isDark = ((x / tileSize) + (y / tileSize)) % 2 != 0;
                var color = isDark ? darkGreen : lightGreen;
                
                var width = Math.Min(tileSize, playArea.Right - x);
                var height = Math.Min(tileSize, playArea.Bottom - y);
                
                frameContext.SpriteBatch.Draw(
                    frameContext.DebugPixel, 
                    new Rectangle(x, y, width, height), 
                    color);
            }
        }

        // Draw Boundary Outline
        ShapeRenderer.DrawLine(frameContext.SpriteBatch, frameContext.DebugPixel, new Vector2(playArea.Left, playArea.Top), new Vector2(playArea.Right, playArea.Top), Color.White, 4);
        ShapeRenderer.DrawLine(frameContext.SpriteBatch, frameContext.DebugPixel, new Vector2(playArea.Right, playArea.Top), new Vector2(playArea.Right, playArea.Bottom), Color.White, 4);
        ShapeRenderer.DrawLine(frameContext.SpriteBatch, frameContext.DebugPixel, new Vector2(playArea.Right, playArea.Bottom), new Vector2(playArea.Left, playArea.Bottom), Color.White, 4);
        ShapeRenderer.DrawLine(frameContext.SpriteBatch, frameContext.DebugPixel, new Vector2(playArea.Left, playArea.Bottom), new Vector2(playArea.Left, playArea.Top), Color.White, 4);
    }
}
