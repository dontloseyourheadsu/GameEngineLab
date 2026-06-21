using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Rendering.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.ShadowHell.Features.Environment.Systems;

public sealed class FloorRendererSystem : IGameSystem
{
    public int Order => 2; // Render first, underneath all other entities

    private const float WorldWidth = 2048f;
    private const float WorldHeight = 1536f;
    private const float TileSize = 64f;

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        // Obtain camera to perform viewport frustum culling
        world.TryGetResource<CameraResource>(out var camera);
        
        float minX = 0f;
        float maxX = WorldWidth;
        float minY = 0f;
        float maxY = WorldHeight;

        if (camera != null)
        {
            float halfW = frameContext.Viewport.Width / 2f;
            float halfH = frameContext.Viewport.Height / 2f;
            // Pad by 1 tile size to avoid border pop-in
            minX = MathHelper.Clamp(camera.Position.X - halfW - TileSize, 0f, WorldWidth);
            maxX = MathHelper.Clamp(camera.Position.X + halfW + TileSize, 0f, WorldWidth);
            minY = MathHelper.Clamp(camera.Position.Y - halfH - TileSize, 0f, WorldHeight);
            maxY = MathHelper.Clamp(camera.Position.Y + halfH + TileSize, 0f, WorldHeight);
        }

        int startCol = (int)(minX / TileSize);
        int endCol = Math.Min((int)(maxX / TileSize) + 1, (int)(WorldWidth / TileSize));
        int startRow = (int)(minY / TileSize);
        int endRow = Math.Min((int)(maxY / TileSize) + 1, (int)(WorldHeight / TileSize));

        for (int r = startRow; r < endRow; r++)
        {
            for (int c = startCol; c < endCol; c++)
            {
                // Alternate tile colors to create a subtle dirt grid texture
                bool alternate = (r + c) % 2 == 0;
                Color dirtColor = alternate 
                    ? new Color(38, 24, 20)  // Earthy dark brown dirt
                    : new Color(32, 18, 14); // Slightly darker dirt tone

                Vector2 center = new Vector2(
                    c * TileSize + TileSize / 2f, 
                    r * TileSize + TileSize / 2f
                );

                ShapeRenderer.DrawRectangle(
                    frameContext.SpriteBatch, 
                    frameContext.DebugPixel, 
                    center, 
                    new Vector2(TileSize, TileSize), 
                    dirtColor
                );
            }
        }
    }
}
