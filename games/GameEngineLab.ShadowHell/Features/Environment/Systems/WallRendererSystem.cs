using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
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

        foreach (var entityId in world.GetEntitiesWith<TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            // Select boundary walls (Rectangle shape, CollisionGroup 1)
            if (body.Shape == RigidBodyShape.Rectangle && body.CollisionGroup == 1)
            {
                // Draw outer white rectangle (thick white border)
                ShapeRenderer.DrawRectangle(
                    frameContext.SpriteBatch, 
                    frameContext.DebugPixel, 
                    transform.Position, 
                    body.Size, 
                    transform.Rotation, 
                    Color.White
                );

                // Draw inner black rectangle (body of the wall)
                float borderThickness = 6f;
                Vector2 innerSize = body.Size - new Vector2(borderThickness * 2f, borderThickness * 2f);
                if (innerSize.X > 0 && innerSize.Y > 0)
                {
                    ShapeRenderer.DrawRectangle(
                        frameContext.SpriteBatch, 
                        frameContext.DebugPixel, 
                        transform.Position, 
                        innerSize, 
                        transform.Rotation, 
                        new Color(10, 8, 15) // Deep shadow black matching floor/background
                    );
                }
            }
        }
    }
}
