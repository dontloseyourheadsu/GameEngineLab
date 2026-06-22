using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GameEngineLab.ShadowHell.Features.Environment.Systems;

public sealed class EmberParticle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Size;
    public float MaxLifetime;
    public float Lifetime;
    public Color Color;
    public float SinOffset;
    public float SinSpeed;
}

public sealed class AtmosphereSystem : IGameSystem
{
    public int Order => 5; // Draw background before other shape rendering systems

    private readonly List<EmberParticle> _embers = new();
    private readonly Random _random = new();

    // Procedural background spikes
    private readonly List<(Vector2 Position, Vector2 Size, float Direction)> _farSpikes = new();
    private readonly List<(Vector2 Position, Vector2 Size, float Direction)> _midSpikes = new();
    private readonly List<Vector2[]> _lightRays = new();
    private Texture2D? _triangleTexture;

    private const float WorldWidth = 2048f;
    private const float WorldHeight = 1536f;

    public AtmosphereSystem()
    {
        // Pre-generate procedural background spiky ruins/cavern elements
        // Far spikes (large, deep background)
        for (int i = 0; i < 25; i++)
        {
            float x = (float)(_random.NextDouble() * (WorldWidth + 1000f) - 500f);
            float y = _random.Next(200, 500); // hanging from top
            float width = _random.Next(100, 250);
            float height = _random.Next(250, 600);
            // Capped at top (pointing down, Direction = 0f)
            _farSpikes.Add((new Vector2(x, y - height / 2f), new Vector2(width, height), 0f));
            
            // Capped at bottom (pointing up, Direction = 1f)
            float xb = (float)(_random.NextDouble() * (WorldWidth + 1000f) - 500f);
            float yb = WorldHeight - _random.Next(200, 500);
            _farSpikes.Add((new Vector2(xb, yb + height / 2f), new Vector2(width, height), 1f));
        }

        // Mid spikes (closer, smaller, more defined)
        for (int i = 0; i < 30; i++)
        {
            float x = (float)(_random.NextDouble() * (WorldWidth + 800f) - 400f);
            float y = _random.Next(150, 400);
            float width = _random.Next(60, 150);
            float height = _random.Next(150, 450);
            // Capped at top (pointing down, Direction = 0f)
            _midSpikes.Add((new Vector2(x, y - height / 2f), new Vector2(width, height), 0f));

            float xb = (float)(_random.NextDouble() * (WorldWidth + 800f) - 400f);
            float yb = WorldHeight - _random.Next(150, 400);
            // Capped at bottom (pointing up, Direction = 1f)
            _midSpikes.Add((new Vector2(xb, yb + height / 2f), new Vector2(width, height), 1f));
        }

        // Pre-generate procedural diagonal light rays
        for (int i = 0; i < 5; i++)
        {
            float xStart = i * 500f - 300f + _random.Next(-80, 80);
            float width = _random.Next(140, 260);

            var verts = new Vector2[4];
            verts[0] = new Vector2(xStart, -400f);
            verts[1] = new Vector2(xStart + width, -400f);
            verts[2] = new Vector2(xStart + width + 900f, WorldHeight + 400f);
            verts[3] = new Vector2(xStart + 900f, WorldHeight + 400f);

            _lightRays.Add(verts);
        }
    }

    public void Update(World world, FrameContext frameContext)
    {
        float dt = frameContext.DeltaSeconds;
        if (dt <= 0) return;

        // Get camera viewport or player positions to spawn embers near player/camera
        Vector2 cameraPos = new Vector2(WorldWidth / 2f, WorldHeight / 2f);
        if (world.TryGetResource<CameraResource>(out var camera) && camera != null)
        {
            cameraPos = camera.Position;
        }

        // Spawn embers
        if (_embers.Count < 120 && _random.NextDouble() < 0.8)
        {
            float spawnX = cameraPos.X + _random.Next(-600, 600);
            float spawnY = cameraPos.Y + _random.Next(400, 600); // spawn below camera

            _embers.Add(new EmberParticle
            {
                Position = new Vector2(spawnX, spawnY),
                Velocity = new Vector2(_random.Next(-30, 30), _random.Next(-80, -40)),
                Size = _random.Next(2, 6),
                MaxLifetime = _random.Next(4, 8),
                Lifetime = 0f,
                Color = _random.Next(0, 2) == 0 ? new Color(255, 69, 0) : new Color(255, 140, 0), // OrangeRed or DarkOrange
                SinOffset = (float)(_random.NextDouble() * Math.PI * 2),
                SinSpeed = (float)(_random.NextDouble() * 2f + 1f)
            });
        }

        // Update embers
        for (int i = _embers.Count - 1; i >= 0; i--)
        {
            var p = _embers[i];
            p.Lifetime += dt;
            if (p.Lifetime >= p.MaxLifetime)
            {
                _embers.RemoveAt(i);
                continue;
            }

            // Move upwards with slight side-to-side sway
            float horizontalSway = (float)Math.Sin(p.Lifetime * p.SinSpeed + p.SinOffset) * 20f * dt;
            p.Position += p.Velocity * dt + new Vector2(horizontalSway, 0f);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        EnsureResources(frameContext.SpriteBatch.GraphicsDevice);

        // Get camera details for parallax scaling
        Vector2 cameraPos = Vector2.Zero;
        if (world.TryGetResource<CameraResource>(out var camera) && camera != null)
        {
            cameraPos = camera.Position;
        }

        // 1. Far Parallax Layer (Warm saturated crimson silhouette mountains/stalactites)
        // Parallax coefficient: 0.4f (moves slower than camera)
        Color farColor = new Color(68, 16, 20); // warm rich dark red
        Vector2 farOffset = cameraPos * 0.6f; // offset factor
        DrawSpikes(frameContext.SpriteBatch, _farSpikes, farOffset, farColor);

        // 2. Mid Parallax Layer (Medium rich dark plum)
        // Parallax coefficient: 0.7f
        Color midColor = new Color(42, 10, 12);
        Vector2 midOffset = cameraPos * 0.3f;
        DrawSpikes(frameContext.SpriteBatch, _midSpikes, midOffset, midColor);

        // ================= SWITCH TO ADDITIVE BLENDING FOR GLOWS & RAYS =================
        frameContext.SpriteBatch.End();
        
        var viewMatrix = camera != null ? camera.GetViewMatrix(frameContext.Viewport) : Matrix.Identity;
        frameContext.SpriteBatch.Begin(
            SpriteSortMode.Deferred, 
            BlendState.Additive, 
            SamplerState.PointClamp, 
            null, 
            null, 
            null, 
            viewMatrix);

        /*
        // 3. Draw light rays in deep background (parallax)
        Vector2 rayOffset = cameraPos * 0.5f;
        Color rayColor = new Color(255, 180, 80, 18); // glowing additive amber beam
        foreach (var ray in _lightRays)
        {
            var offsetVerts = new Vector2[4];
            offsetVerts[0] = ray[0] + rayOffset;
            offsetVerts[1] = ray[1] + rayOffset;
            offsetVerts[2] = ray[2] + rayOffset;
            offsetVerts[3] = ray[3] + rayOffset;
            ShapeRenderer.DrawFilledPolygon(frameContext.SpriteBatch, frameContext.DebugPixel, offsetVerts, rayColor);
        }
        */



        // 5. Draw Ember Particles
        foreach (var p in _embers)
        {
            float lifePercent = p.Lifetime / p.MaxLifetime;
            float alpha = (float)Math.Sin(lifePercent * Math.PI) * 0.8f; // fade in & out
            Color drawCol = p.Color * alpha;

            // Draw glowing circle
            ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, p.Position, p.Size, drawCol);
        }

        // ================= RESTORE ALPHA BLENDING FOR SUBSEQUENT SYSTEMS =================
        frameContext.SpriteBatch.End();
        frameContext.SpriteBatch.Begin(
            SpriteSortMode.Deferred, 
            BlendState.AlphaBlend, 
            SamplerState.PointClamp, 
            null, 
            null, 
            null, 
            viewMatrix);
    }

    private void EnsureResources(GraphicsDevice gd)
    {
        if (_triangleTexture == null)
        {
            int w = 64;
            int h = 64;
            _triangleTexture = new Texture2D(gd, w, h);
            Color[] data = new Color[w * h];
            for (int y = 0; y < h; y++)
            {
                float ratio = 1f - ((float)y / (h - 1));
                int span = (int)(w * ratio);
                int startX = (w - span) / 2;
                int endX = startX + span;
                for (int x = 0; x < w; x++)
                {
                    data[y * w + x] = (x >= startX && x < endX) ? Color.White : Color.Transparent;
                }
            }
            _triangleTexture.SetData(data);
        }
    }

    private void DrawSpikes(SpriteBatch spriteBatch, List<(Vector2 Position, Vector2 Size, float Direction)> spikes, Vector2 offset, Color color)
    {
        if (_triangleTexture == null) return;

        Vector2 origin = new Vector2(_triangleTexture.Width / 2f, _triangleTexture.Height / 2f);

        foreach (var spike in spikes)
        {
            Vector2 drawPos = spike.Position + offset;
            Vector2 scale = new Vector2(spike.Size.X / _triangleTexture.Width, spike.Size.Y / _triangleTexture.Height);
            SpriteEffects effects = spike.Direction > 0.5f ? SpriteEffects.FlipVertically : SpriteEffects.None;

            spriteBatch.Draw(
                _triangleTexture,
                drawPos,
                null,
                color,
                0f,
                origin,
                scale,
                effects,
                0f
            );
        }
    }
}
