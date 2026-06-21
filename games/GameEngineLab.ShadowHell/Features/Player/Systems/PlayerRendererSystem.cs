using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.ShadowHell.Features.Player.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.ShadowHell.Features.Player.Systems;

public sealed class PlayerRendererSystem : IGameSystem
{
    public int Order => 105; // Render player on top of shape renderings and enemies

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        foreach (var entityId in world.GetEntitiesWith<PlayerComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<PlayerComponent>(entityId, out var player);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            // 1. Draw floor shadow (scales down as the player flies higher)
            float shadowScale = Math.Max(0.4f, 1f - (player.JumpZ / 120f));
            float shadowRadius = body.BoundingRadius * shadowScale;
            Vector2 shadowCenter = transform.Position;
            ShapeRenderer.DrawEllipse(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                shadowCenter, 
                new Vector2(shadowRadius * 2f, shadowRadius * 1f), 
                new Color(0, 0, 0, 100)
            );

            // 2. Draw white player body offset by JumpZ (elevation)
            Vector2 bodyCenter = transform.Position - new Vector2(0f, player.JumpZ);

            // Draw solid white circle
            ShapeRenderer.DrawCircle(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                bodyCenter, 
                body.BoundingRadius, 
                Color.White
            );

            // 3. Draw golden halo circle outline above the body
            float bob = (player.State == PlayerState.Idle || player.State == PlayerState.Flying)
                ? (float)Math.Sin(player.AnimationTime * 3.5f) * 2f
                : 0f;
            Vector2 haloCenter = bodyCenter + new Vector2(0f, -22f + bob);

            ShapeRenderer.DrawCircleOutline(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                haloCenter, 
                12f, 
                new Color(255, 230, 80), 
                2
            );
        }
    }
}
