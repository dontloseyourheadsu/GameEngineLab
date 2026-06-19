using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Maps;
using GameEngineLab.GolfIt.Features.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineLab.GolfIt.Features.Rendering.Systems;

public sealed class ShadowRenderSystem : IGameSystem
{
    public int Order => 50; // Draw after floor (order -10) but before shapes (order 100)

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        // 1. Gather all active light sources
        var lights = new List<Vector2>();

        // Global top-left light
        if (world.TryGetResource<MapEditorStateResource>(out var mapState) && mapState != null)
        {
            if (mapState.EnableGlobalLight)
            {
                // Position top-left (e.g. 50, 50 in base coordinates)
                lights.Add(new Vector2(50, 50));
            }
        }

        // Placed light sources
        foreach (var entityId in world.GetEntitiesWith<TransformComponent, EditorObjectComponent>())
        {
            if (world.HasComponent<TemplateComponent>(entityId)) continue;

            world.TryGetComponent<EditorObjectComponent>(entityId, out var editorObj);
            if (editorObj.ToolType == EditorTool.Light)
            {
                world.TryGetComponent<TransformComponent>(entityId, out var transform);
                lights.Add(transform.Position);
            }
        }

        if (lights.Count == 0) return;

        // 2. Render shadows for all valid shapes
        var shadowColor = new Color(0, 0, 0, 85); // Transparent black for soft shadow overlay

        foreach (var entityId in world.GetEntitiesWith<TransformComponent, RigidBodyComponent>())
        {
            if (world.HasComponent<TemplateComponent>(entityId)) continue;
            if (world.HasComponent<HiddenComponent>(entityId)) continue;

            // Don't cast shadows for light source spheres themselves
            if (world.TryGetComponent<EditorObjectComponent>(entityId, out var editorObj) && editorObj.ToolType == EditorTool.Light)
                continue;

            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            // Avoid casting shadows for soft body nodes individually (we handle the softbody shape rendering)
            if (world.HasComponent<SoftBodyNodeComponent>(entityId)) continue;

            foreach (var lightPos in lights)
            {
                if (body.Shape == RigidBodyShape.Circle)
                {
                    RenderCircleShadow(frameContext.SpriteBatch, frameContext.DebugPixel, transform.Position, body.BoundingRadius, lightPos, shadowColor);
                }
                else if (body.Shape == RigidBodyShape.Rectangle)
                {
                    RenderRectangleShadow(frameContext.SpriteBatch, frameContext.DebugPixel, transform.Position, body.Size, transform.Rotation, lightPos, shadowColor);
                }
                else if (body.Shape == RigidBodyShape.Polygon)
                {
                    if (world.TryGetComponent<PolygonComponent>(entityId, out var polygon))
                    {
                        RenderPolygonShadow(frameContext.SpriteBatch, frameContext.DebugPixel, polygon.TransformedVertices, lightPos, shadowColor);
                    }
                }
            }
        }
    }

    private void RenderCircleShadow(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float radius, Vector2 lightPos, Color color)
    {
        var dir = center - lightPos;
        var dist = dir.Length();
        if (dist <= radius) return; // Light is inside the circle

        dir.Normalize();
        var perp = new Vector2(-dir.Y, dir.X);

        // Silhouette points on the circle
        var t1 = center - perp * radius;
        var t2 = center + perp * radius;

        // Project silhouette points far away
        var t1Proj = t1 + Vector2.Normalize(t1 - lightPos) * 2000f;
        var t2Proj = t2 + Vector2.Normalize(t2 - lightPos) * 2000f;

        var verts = new[] { t1, t2, t2Proj, t1Proj };
        ShapeRenderer.DrawFilledPolygon(spriteBatch, pixel, verts, color);
    }

    private void RenderRectangleShadow(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Vector2 size, float rotation, Vector2 lightPos, Color color)
    {
        float hw = size.X / 2f;
        float hh = size.Y / 2f;

        var cos = (float)Math.Cos(rotation);
        var sin = (float)Math.Sin(rotation);

        Vector2 Rotate(Vector2 v) => new Vector2(v.X * cos - v.Y * sin, v.X * sin + v.Y * cos);

        var corners = new[]
        {
            center + Rotate(new Vector2(-hw, -hh)),
            center + Rotate(new Vector2(hw, -hh)),
            center + Rotate(new Vector2(hw, hh)),
            center + Rotate(new Vector2(-hw, hh))
        };

        RenderPolygonShadow(spriteBatch, pixel, corners, lightPos, color);
    }

    private void RenderPolygonShadow(SpriteBatch spriteBatch, Texture2D pixel, Vector2[] vertices, Vector2 lightPos, Color color)
    {
        if (vertices == null || vertices.Length < 3) return;

        // Calculate centroid
        var center = Vector2.Zero;
        foreach (var v in vertices) center += v;
        center /= vertices.Length;

        for (int i = 0; i < vertices.Length; i++)
        {
            var p1 = vertices[i];
            var p2 = vertices[(i + 1) % vertices.Length];

            var edge = p2 - p1;
            var mid = (p1 + p2) / 2f;

            // Outward facing normal
            var normal = new Vector2(-edge.Y, edge.X);
            if (Vector2.Dot(normal, mid - center) < 0)
            {
                normal = -normal;
            }

            var toLight = mid - lightPos;
            if (Vector2.Dot(normal, toLight) > 0)
            {
                // Edge is lit, project it outwards to form shadow
                var p1Proj = p1 + Vector2.Normalize(p1 - lightPos) * 2000f;
                var p2Proj = p2 + Vector2.Normalize(p2 - lightPos) * 2000f;

                var verts = new[] { p1, p2, p2Proj, p1Proj };
                ShapeRenderer.DrawFilledPolygon(spriteBatch, pixel, verts, color);
            }
        }
    }
}
