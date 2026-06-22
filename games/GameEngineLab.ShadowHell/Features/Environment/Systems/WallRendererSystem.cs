using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameEngineLab.ShadowHell.Features.Environment.Resources;
using System;

namespace GameEngineLab.ShadowHell.Features.Environment.Systems;

public sealed class WallRendererSystem : IGameSystem
{
    public int Order => 103; // Render walls on top of floor but below entities/shapes

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        // Retrieve generated textures
        if (!world.TryGetResource<GameTextureResource>(out var textures) || textures == null) return;

        foreach (var entityId in world.GetEntitiesWith<TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            // Select boundary walls (Rectangle shape, CollisionGroup 1)
            if (body.Shape == RigidBodyShape.Rectangle && body.CollisionGroup == 1)
            {
                DrawTiled(frameContext.SpriteBatch, textures.WallTexture, transform.Position, body.Size);
            }
        }
    }

    private void DrawTiled(SpriteBatch spriteBatch, Texture2D texture, Vector2 center, Vector2 size)
    {
        int texW = texture.Width;
        int texH = texture.Height;

        float startX = center.X - size.X / 2f;
        float startY = center.Y - size.Y / 2f;

        for (float y = 0; y < size.Y; y += texH)
        {
            int drawH = (int)Math.Min(texH, size.Y - y);
            for (float x = 0; x < size.X; x += texW)
            {
                int drawW = (int)Math.Min(texW, size.X - x);

                var destRect = new Rectangle(
                    (int)(startX + x),
                    (int)(startY + y),
                    drawW,
                    drawH
                );

                var srcRect = new Rectangle(0, 0, drawW, drawH);

                spriteBatch.Draw(texture, destRect, srcRect, Color.White);
            }
        }
    }
}
