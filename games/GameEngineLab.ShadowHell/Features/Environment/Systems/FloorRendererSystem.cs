using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Rendering.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using GameEngineLab.ShadowHell.Features.Environment.Resources;

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

        // Draw a solid black rectangle over the entire world map
        ShapeRenderer.DrawRectangle(
            frameContext.SpriteBatch,
            frameContext.DebugPixel,
            new Vector2(WorldWidth / 2f, WorldHeight / 2f),
            new Vector2(WorldWidth, WorldHeight),
            Color.Black
        );
    }
}
