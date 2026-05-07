using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngineLab.Core.Features.Rendering.Systems;

public sealed class ShapeRenderSystem : IGameSystem
{
    public int Order => 100;

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        world.TryGetResource<PaletteResource>(out var palette);
        
        foreach (var entityId in world.GetEntitiesWith<TransformComponent, RigidBodyComponent>())
        {
            if (world.HasComponent<HiddenComponent>(entityId)) continue;
            
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            
            Color color = Color.White;
            if (world.TryGetComponent<DrawColorComponent>(entityId, out var drawColor))
            {
                color = drawColor.Value;
            }

            if (world.HasComponent<SoftBodyNodeComponent>(entityId)) continue;

            if (body.Shape == RigidBodyShape.Circle)
            {
                ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, transform.Position, body.BoundingRadius, color);
            }
            else if (body.Shape == RigidBodyShape.Rectangle)
            {
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, transform.Position, body.Size, transform.Rotation, color);
            }
            else if (body.Shape == RigidBodyShape.Polygon)
            {
                if (world.TryGetComponent<PolygonComponent>(entityId, out var polygon))
                {
                    ShapeRenderer.DrawPolygon(frameContext.SpriteBatch, frameContext.DebugPixel, polygon.TransformedVertices, color, 2);
                }
            }
        }
    }
}
