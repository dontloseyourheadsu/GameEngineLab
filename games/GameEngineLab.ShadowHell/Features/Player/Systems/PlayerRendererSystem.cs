using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.ShadowHell.Features.Player.Components;
using GameEngineLab.ShadowHell.Features.Environment.Resources;
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

        // Retrieve generated textures
        world.TryGetResource<GameTextureResource>(out var textures);

        foreach (var entityId in world.GetEntitiesWith<PlayerComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<PlayerComponent>(entityId, out var player);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            // 1. Draw floor shadow (scales down as the player flies higher)
            float shadowScale = Math.Max(0.4f, 1f - (player.JumpZ / 120f));
            float shadowRadius = body.BoundingRadius * shadowScale;
            
            // Adjust the centers and offsets to account for top-down perspective, size, and object alignment.
            // In top-down 2D, the shadow is projected at the feet of the object (bottom of the collision circle),
            // while the body is offset upwards.
            float verticalOffset = body.BoundingRadius * 0.6f;
            Vector2 shadowCenter = (player.State == PlayerState.Flying || player.JumpZ > 0.001f)
                ? player.JumpStartPos + new Vector2(0f, verticalOffset)
                : transform.Position + new Vector2(0f, verticalOffset);

            ShapeRenderer.DrawEllipse(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                shadowCenter, 
                new Vector2(shadowRadius * 2f, shadowRadius * 1f), 
                new Color(0, 0, 0, 100)
            );

            // 2. Draw white player body offset by JumpZ (elevation) and offset upwards slightly
            Vector2 bodyCenter = transform.Position - new Vector2(0f, player.JumpZ + verticalOffset * 0.5f);

            // Draw soft breathing light aura around the player (short-range circle)
            if (textures != null)
            {
                float pulse = (float)Math.Sin(player.AnimationTime * 2.5f) * 0.04f;
                // Tight short-range light that scales slightly with flight height
                float heightFactor = player.JumpZ / 64f;
                float auraScale = (0.75f + heightFactor * 0.1f) + pulse;
                Vector2 auraSize = new Vector2(textures.LightTexture.Width, textures.LightTexture.Height) * auraScale;
                Vector2 auraTopLeft = bodyCenter - auraSize / 2f;
                Color auraColor = new Color(255, 235, 180) * (0.22f - heightFactor * 0.04f); // faint warm glow

                frameContext.SpriteBatch.Draw(
                    textures.LightTexture,
                    new Rectangle((int)auraTopLeft.X, (int)auraTopLeft.Y, (int)auraSize.X, (int)auraSize.Y),
                    auraColor
                );
            }

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
