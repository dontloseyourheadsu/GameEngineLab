using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Components;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.Pacman.Features.Pacman.Systems;

public sealed class PacmanRenderSystem : IGameSystem
{
    public int Order => 96;

    private float _animationTimer;

    public void Update(World world, FrameContext frameContext)
    {
        _animationTimer += frameContext.DeltaSeconds;
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch is null || frameContext.DebugPixel is null) return;

        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay) return;

        if (!world.TryGetResource<MapStateResource>(out var mapState) || mapState is null) return;

        var gpAssets = world.GetRequiredResource<GameplayAssetsResource>();
        var sb = frameContext.SpriteBatch;
        var map = mapState.Map;
        var offsetX = (frameContext.Viewport.Width - map.Width * map.TileSize) / 2;
        var offsetY = (frameContext.Viewport.Height - map.Height * map.TileSize) / 2;

        foreach (var entity in world.GetEntitiesWith<PacmanPlayerComponent, TransformComponent>())
        {
            if (!world.TryGetComponent<PacmanPlayerComponent>(entity, out var pacman)
                || !world.TryGetComponent<TransformComponent>(entity, out var transform)) continue;

            var rect = new Rectangle(
                (int)(offsetX + transform.Position.X - pacman.Radius),
                (int)(offsetY + transform.Position.Y - pacman.Radius),
                (int)(pacman.Radius * 2f),
                (int)(pacman.Radius * 2f));

            if (gpAssets.IsInitialized)
            {
                int frameIdx = (int)(_animationTimer * 10) % gpAssets.PacmanFrames.Length;
                var tex = gpAssets.PacmanFrames[frameIdx];
                
                float rotation = 0;
                SpriteEffects effects = SpriteEffects.None;

                if (pacman.CurrentDirection.X > 0) rotation = 0;
                else if (pacman.CurrentDirection.X < 0) effects = SpriteEffects.FlipHorizontally;
                else if (pacman.CurrentDirection.Y < 0) rotation = -MathHelper.PiOver2;
                else if (pacman.CurrentDirection.Y > 0) rotation = MathHelper.PiOver2;

                sb.Draw(tex, rect, null, Color.White, rotation, new Vector2(tex.Width/2, tex.Height/2), effects, 0);
            }
            else
            {
                sb.Draw(frameContext.DebugPixel, rect, Color.Gold);
            }
        }
    }
}
