using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;
using System;

namespace GameEngineLab.Core.Features.Physics.Systems;

public sealed class PolygonUpdateSystem : IGameSystem
{
    public int Order => 11; // After Movement, before Collision

    public void Update(World world, FrameContext frameContext)
    {
        foreach (var entityId in world.GetEntitiesWith<PolygonComponent, TransformComponent>())
        {
            world.TryGetComponent<PolygonComponent>(entityId, out var polygon);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);

            if (polygon.Vertices == null || polygon.Vertices.Length == 0) continue;

            float cos = (float)Math.Cos(transform.Rotation);
            float sin = (float)Math.Sin(transform.Rotation);

            for (int i = 0; i < polygon.Vertices.Length; i++)
            {
                var vertex = polygon.Vertices[i];
                
                // Rotate
                float rx = vertex.X * cos - vertex.Y * sin;
                float ry = vertex.X * sin + vertex.Y * cos;
                
                // Translate
                polygon.TransformedVertices[i] = new Vector2(rx, ry) + transform.Position;
            }
            
            // Note: Since PolygonComponent is a struct, we need to set it back if we modified its fields.
            // However, TransformedVertices is an array (reference type), so the contents are updated.
            // But if we want to be safe or if we add other value types, we should SetComponent.
            world.SetComponent(entityId, polygon);
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
