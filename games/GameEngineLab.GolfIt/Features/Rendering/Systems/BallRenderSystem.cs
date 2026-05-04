using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Ball.Components;
using GameEngineLab.GolfIt.Features.Physics.Components;
using GameEngineLab.GolfIt.Features.Rendering.Resources;
using Microsoft.Xna.Framework;

namespace GameEngineLab.GolfIt.Features.Rendering.Systems;

public sealed class BallRenderSystem : IGameSystem
{
    public int Order => 100;

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        // Render Borders
        if (world.TryGetResource<MapBoundsResource>(out var bounds) && bounds != null &&
            world.TryGetResource<PaletteLibraryResource>(out var library) && library != null)
        {
            var borderColor = library.Specific.GetColor(3); // A dark teal/blue from the specific palette
            var area = bounds.PlayArea;
            var thickness = 10;

            // Top
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(area.Center.X, area.Top + thickness / 2f), new Vector2(area.Width, thickness), borderColor);
            // Bottom
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(area.Center.X, area.Bottom - thickness / 2f), new Vector2(area.Width, thickness), borderColor);
            // Left
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(area.Left + thickness / 2f, area.Center.Y), new Vector2(thickness, area.Height), borderColor);
            // Right
            ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, 
                new Vector2(area.Right - thickness / 2f, area.Center.Y), new Vector2(thickness, area.Height), borderColor);
        }

        // Render Balls
        foreach (var entityId in world.GetEntitiesWith<BallComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            
            var color = Color.White;
            if (world.TryGetComponent<DrawColorComponent>(entityId, out var colorComp))
            {
                color = colorComp.Value;
            }

            ShapeRenderer.DrawCircle(
                frameContext.SpriteBatch,
                frameContext.DebugPixel,
                transform.Position,
                body.BoundingRadius,
                color);
        }

        // Render Obstacles
        foreach (var entityId in world.GetEntitiesWith<ObstacleComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);

            var color = Color.DarkRed;
            if (world.TryGetComponent<DrawColorComponent>(entityId, out var colorComp))
            {
                color = colorComp.Value;
            }

            ShapeRenderer.DrawRectangle(
                frameContext.SpriteBatch,
                frameContext.DebugPixel,
                transform.Position,
                body.Size,
                color);
        }

        // Render Trigger Zones (Goals, etc.)
        foreach (var entityId in world.GetEntitiesWith<TriggerZoneComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);

            var color = Color.Black;
            if (world.TryGetComponent<DrawColorComponent>(entityId, out var colorComp))
            {
                color = colorComp.Value;
            }

            if (body.Shape == RigidBodyShape.Circle)
            {
                ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, transform.Position, body.BoundingRadius, color);
            }
            else if (body.Shape == RigidBodyShape.Rectangle)
            {
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, transform.Position, body.Size, color);
            }
        }
    }
}
