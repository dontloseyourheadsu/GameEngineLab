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
            Vector2 shadowCenter = transform.Position + new Vector2(0f, verticalOffset);

            ShapeRenderer.DrawEllipse(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                shadowCenter, 
                new Vector2(shadowRadius * 2f, shadowRadius * 1f), 
                new Color(0, 0, 0, 100)
            );

            // 2. Draw player body offset by JumpZ (elevation) and offset upwards slightly
            Vector2 bodyCenter = transform.Position - new Vector2(0f, player.JumpZ + verticalOffset * 0.5f);

            // Flashing effect during invincibility frames
            if (player.InvincibilityTimer > 0f && (int)(player.InvincibilityTimer * 20f) % 2 == 0)
            {
                continue;
            }

            Color glowColor = new Color(177, 0, 255); // Neon Purple
            Color innerGlowColor = new Color(220, 120, 255); // Light Violet

            // Draw soft breathing purple light aura around the player (smaller and less bright)
            if (textures != null)
            {
                float pulse = (float)Math.Sin(player.AnimationTime * 4f) * 0.03f;
                float heightFactor = player.JumpZ / 64f;
                float auraScale = (0.4f + heightFactor * 0.05f) + pulse;
                Vector2 auraSize = new Vector2(textures.LightTexture.Width, textures.LightTexture.Height) * auraScale;
                Vector2 auraTopLeft = bodyCenter - auraSize / 2f;
                Color auraColor = glowColor * (0.22f - heightFactor * 0.04f);

                frameContext.SpriteBatch.Draw(
                    textures.LightTexture,
                    new Rectangle((int)auraTopLeft.X, (int)auraTopLeft.Y, (int)auraSize.X, (int)auraSize.Y),
                    auraColor
                );
            }

            // Draw smooth flying/motion trail if rolling
            if (player.State == PlayerState.Rolling)
            {
                for (int i = 1; i <= 3; i++)
                {
                    float offsetDist = i * 8f;
                    Vector2 trailPos = bodyCenter - player.RollDirection * offsetDist;
                    float alpha = 0.45f / i;
                    
                    // Trail circles
                    ShapeRenderer.DrawCircleOutline(
                        frameContext.SpriteBatch, 
                        frameContext.DebugPixel, 
                        trailPos, 
                        body.BoundingRadius, 
                        glowColor * alpha, 
                        2
                    );
                }
            }

            // Draw solid deep black core circle
            ShapeRenderer.DrawCircle(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                bodyCenter, 
                body.BoundingRadius, 
                new Color(10, 8, 15) // Deep shadow black core
            );

            // Draw glowing purple/violet borders
            // Outer soft glow outline
            ShapeRenderer.DrawCircleOutline(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                bodyCenter, 
                body.BoundingRadius + 1f, 
                glowColor * 0.6f, 
                2
            );

            // Inner crisp outline
            ShapeRenderer.DrawCircleOutline(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                bodyCenter, 
                body.BoundingRadius, 
                innerGlowColor, 
                2
            );
        }
    }
}
