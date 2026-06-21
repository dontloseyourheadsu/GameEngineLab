using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngineLab.ShadowHell.Features.Environment.Systems;

public sealed class FortressRendererSystem : IGameSystem
{
    public int Order => 102; // Runs after ShapeRenderSystem to draw brick highlights on top

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        Color brickHighlight = new Color(110, 48, 56); // Minecraft Nether Fortress brick highlight
        Color netherrackCracks = new Color(140, 50, 52, 180); // lighter crack color for walls

        world.TryGetResource<CameraResource>(out var camera);
        float minX = float.MinValue;
        float maxX = float.MaxValue;
        float minY = float.MinValue;
        float maxY = float.MaxValue;

        if (camera != null)
        {
            float halfW = frameContext.Viewport.Width / 2f;
            float halfH = frameContext.Viewport.Height / 2f;
            float margin = 160f; // boundary margin for columns/boundary rocks
            minX = camera.Position.X - halfW - margin;
            maxX = camera.Position.X + halfW + margin;
            minY = camera.Position.Y - halfH - margin;
            maxY = camera.Position.Y + halfH + margin;
        }

        foreach (var entityId in world.GetEntitiesWith<TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<TransformComponent>(entityId, out var transform);

            // Frustum Culling: skip off-screen elements
            if (camera != null)
            {
                if (transform.Position.X < minX || transform.Position.X > maxX ||
                    transform.Position.Y < minY || transform.Position.Y > maxY)
                {
                    continue;
                }
            }

            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            // Draw brick grid overlay on interior columns (Group 8)
            if (body.CollisionGroup == 8)
            {
                if (body.Shape == RigidBodyShape.Rectangle)
                {
                    // Draw outer border highlight
                    ShapeRenderer.DrawRectangle(
                        frameContext.SpriteBatch, 
                        frameContext.DebugPixel, 
                        transform.Position, 
                        body.Size, 
                        transform.Rotation, 
                        brickHighlight
                    );
                    
                    // Draw internal brick lines (horizontal & vertical segments)
                    // Let's draw 2 horizontal lines inside
                    float hw = body.Size.X / 2f;
                    float hh = body.Size.Y / 2f;
                    
                    // Generate local offsets, rotate them by transform.Rotation
                    Vector2 dirX = new Vector2((float)System.Math.Cos(transform.Rotation), (float)System.Math.Sin(transform.Rotation));
                    Vector2 dirY = new Vector2(-dirX.Y, dirX.X);

                    // Horizontal lines
                    for (float f = -0.5f; f <= 0.5f; f += 0.5f)
                    {
                        Vector2 start = transform.Position + dirY * (hh * f) - dirX * hw;
                        Vector2 end = transform.Position + dirY * (hh * f) + dirX * hw;
                        ShapeRenderer.DrawLine(frameContext.SpriteBatch, frameContext.DebugPixel, start, end, brickHighlight, 2);
                    }

                    // Vertical brick gaps (alternating offset lines)
                    Vector2 startV1 = transform.Position - dirY * hh - dirX * (hw * 0.3f);
                    Vector2 endV1 = transform.Position - dirX * (hw * 0.3f);
                    ShapeRenderer.DrawLine(frameContext.SpriteBatch, frameContext.DebugPixel, startV1, endV1, brickHighlight, 2);

                    Vector2 startV2 = transform.Position + dirX * (hw * 0.4f);
                    Vector2 endV2 = transform.Position + dirY * hh + dirX * (hw * 0.4f);
                    ShapeRenderer.DrawLine(frameContext.SpriteBatch, frameContext.DebugPixel, startV2, endV2, brickHighlight, 2);
                }
                else if (body.Shape == RigidBodyShape.Circle)
                {
                    // Circular column: draw circular brick highlights
                    ShapeRenderer.DrawCircleOutline(
                        frameContext.SpriteBatch, 
                        frameContext.DebugPixel, 
                        transform.Position, 
                        body.BoundingRadius, 
                        brickHighlight, 
                        2
                    );
                    
                    // Inner ring
                    ShapeRenderer.DrawCircleOutline(
                        frameContext.SpriteBatch, 
                        frameContext.DebugPixel, 
                        transform.Position, 
                        body.BoundingRadius * 0.6f, 
                        brickHighlight, 
                        1
                    );
                }
            }
            // Draw rocky cracks on outer boundaries (Group 1 - Netherrack hell dirt)
            else if (body.CollisionGroup == 1)
            {
                if (body.Shape == RigidBodyShape.Circle)
                {
                    // Draw outer rock texture details
                    ShapeRenderer.DrawCircleOutline(
                        frameContext.SpriteBatch, 
                        frameContext.DebugPixel, 
                        transform.Position, 
                        body.BoundingRadius, 
                        netherrackCracks, 
                        1
                    );
                }
            }
        }
    }
}
