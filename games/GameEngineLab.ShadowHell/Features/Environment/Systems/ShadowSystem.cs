using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.ShadowHell.Features.Environment.Components;
using GameEngineLab.ShadowHell.Features.Enemy.Components;
using GameEngineLab.ShadowHell.Features.Player.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GameEngineLab.ShadowHell.Features.Environment.Systems;

public sealed class ShadowSystem : IGameSystem
{
    public int Order => 10; // Draw after atmosphere background (Order 5) but before skeletons/shapes (Order 90+)

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        // 1. Gather active light sources
        var lights = new List<(Vector2 Position, LightSourceComponent Light)>();

        foreach (var entityId in world.GetEntitiesWith<TransformComponent, LightSourceComponent>())
        {
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<LightSourceComponent>(entityId, out var light);
            lights.Add((transform.Position, light));
        }

        if (lights.Count == 0) return;

        // 2. Render colored glow auras for all lights (gives rich color to the cave) using ADDITIVE blending
        world.TryGetResource<CameraResource>(out var camera);
        var viewMatrix = camera != null ? camera.GetViewMatrix(frameContext.Viewport) : Matrix.Identity;

        frameContext.SpriteBatch.End();
        frameContext.SpriteBatch.Begin(
            SpriteSortMode.Deferred, 
            BlendState.Additive, 
            SamplerState.PointClamp, 
            null, 
            null, 
            null, 
            viewMatrix);

        foreach (var lightData in lights)
        {
            Vector2 pos = lightData.Position;
            var light = lightData.Light;

            float r = light.Radius;
            Color col = light.LightColor;
            float intensity = light.Intensity;

            // Soft additive outer glows
            ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, pos, r, col * (intensity * 0.15f));
            ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, pos, r * 0.7f, col * (intensity * 0.35f));
            ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, pos, r * 0.4f, col * (intensity * 0.65f));
            ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, pos, r * 0.15f, col * (intensity * 0.95f));
        }

        // ================= RESTORE ALPHA BLENDING FOR BLACK PROJECTED SHADOWS =================
        frameContext.SpriteBatch.End();
        frameContext.SpriteBatch.Begin(
            SpriteSortMode.Deferred, 
            BlendState.AlphaBlend, 
            SamplerState.PointClamp, 
            null, 
            null, 
            null, 
            viewMatrix);

        // 3. Project semi-transparent black shadows for blockers away from each light source
        // Using semi-transparent black (alpha 210) so the background glows/rays are partially visible
        // under the shadow, creating high visual fidelity.
        var shadowColor = new Color(0, 0, 0, 210); 

        var shadowBlockers = new List<(Vector2 Position, RigidBodyComponent Body, float Rotation)>();

        foreach (var entityId in world.GetEntitiesWith<TransformComponent, RigidBodyComponent>())
        {
            // Do not cast shadows for light sources themselves or the player
            if (world.HasComponent<LightSourceComponent>(entityId) || world.HasComponent<PlayerComponent>(entityId)) 
                continue;

            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            // OPTIMIZATION: Only cast shadows for dynamic enemies (Group 2) and interior cavern pillars (Group 8)
            if (body.CollisionGroup != 2 && body.CollisionGroup != 8)
                continue;

            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            shadowBlockers.Add((transform.Position, body, transform.Rotation));
        }

        float cameraMinX = float.MinValue;
        float cameraMaxX = float.MaxValue;
        float cameraMinY = float.MinValue;
        float cameraMaxY = float.MaxValue;
        if (camera != null)
        {
            float halfW = frameContext.Viewport.Width / 2f;
            float halfH = frameContext.Viewport.Height / 2f;
            float margin = 200f; // margin for visibility
            cameraMinX = camera.Position.X - halfW - margin;
            cameraMaxX = camera.Position.X + halfW + margin;
            cameraMinY = camera.Position.Y - halfH - margin;
            cameraMaxY = camera.Position.Y + halfH + margin;
        }

        // Project shadows using highly optimized GPU rectangle draw calls (NO CPU scanline loops)
        foreach (var blocker in shadowBlockers)
        {
            // 1. Frustum Culling: skip blockers that are far off-screen
            if (camera != null)
            {
                if (blocker.Position.X < cameraMinX || blocker.Position.X > cameraMaxX ||
                    blocker.Position.Y < cameraMinY || blocker.Position.Y > cameraMaxY)
                {
                    continue;
                }
            }

            foreach (var lightData in lights)
            {
                Vector2 lightPos = lightData.Position;
                var light = lightData.Light;

                float blockerRadius = blocker.Body.Shape == RigidBodyShape.Circle 
                    ? blocker.Body.BoundingRadius 
                    : blocker.Body.Size.Length() / 2f;

                // 2. Light Range Culling: skip blockers that are outside the range of this light pool
                float dist = Vector2.Distance(blocker.Position, lightPos);
                if (dist > light.Radius + blockerRadius)
                    continue;

                if (blocker.Body.Shape == RigidBodyShape.Circle)
                {
                    RenderCircleShadow(
                        frameContext.SpriteBatch, 
                        frameContext.DebugPixel, 
                        blocker.Position, 
                        blocker.Body.BoundingRadius, 
                        lightPos, 
                        shadowColor
                    );
                }
                else if (blocker.Body.Shape == RigidBodyShape.Rectangle)
                {
                    RenderRectangleShadow(
                        frameContext.SpriteBatch, 
                        frameContext.DebugPixel, 
                        blocker.Position, 
                        blocker.Body.Size, 
                        blocker.Rotation, 
                        lightPos, 
                        shadowColor
                    );
                }
            }
        }
    }

    private void RenderCircleShadow(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float radius, Vector2 lightPos, Color color)
    {
        Vector2 dir = center - lightPos;
        float dist = dir.Length();
        if (dist <= radius) return; // Light inside blocker

        dir.Normalize();
        float angle = (float)Math.Atan2(dir.Y, dir.X);

        // Highly optimized GPU rectangle shadow projection extending 700 pixels away from the blocker
        float shadowLength = 700f;
        Vector2 shadowCenter = center + dir * (radius + shadowLength / 2f);

        // Draw rotated rectangle representing shadow volume
        ShapeRenderer.DrawRectangle(spriteBatch, pixel, shadowCenter, new Vector2(shadowLength, radius * 2f), angle, color);
    }

    private void RenderRectangleShadow(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Vector2 size, float rotation, Vector2 lightPos, Color color)
    {
        Vector2 dir = center - lightPos;
        float dist = dir.Length();
        if (dist <= 5f) return;

        dir.Normalize();
        float angle = (float)Math.Atan2(dir.Y, dir.X);

        float shadowLength = 700f;
        float blockerRadius = size.Length() / 2f;
        Vector2 shadowCenter = center + dir * (blockerRadius + shadowLength / 2f);

        // Draw rotated rectangle representing shadow volume
        ShapeRenderer.DrawRectangle(spriteBatch, pixel, shadowCenter, new Vector2(shadowLength, blockerRadius * 2f), angle, color);
    }
}
