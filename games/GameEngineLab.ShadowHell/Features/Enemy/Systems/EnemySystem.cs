using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.ShadowHell.Features.Player.Components;
using GameEngineLab.ShadowHell.Features.Enemy.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace GameEngineLab.ShadowHell.Features.Enemy.Systems;

public sealed class EnemySystem : IGameSystem
{
    public int Order => 101; // Runs after physics updates and shape rendering

    public void Update(World world, FrameContext frameContext)
    {
        float dt = frameContext.DeltaSeconds;
        if (dt <= 0) return;

        // Find Player position
        Vector2 playerPos = Vector2.Zero;
        bool playerFound = false;
        foreach (var playerEntity in world.GetEntitiesWith<PlayerComponent, TransformComponent>())
        {
            world.TryGetComponent<TransformComponent>(playerEntity, out var playerTransform);
            playerPos = playerTransform.Position;
            playerFound = true;
            break;
        }

        if (!playerFound) return;

        foreach (var entityId in world.GetEntitiesWith<EnemyComponent, TransformComponent, VelocityComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<EnemyComponent>(entityId, out var enemy);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            enemy.AnimTime += dt;

            // Simple AI: steer towards the player
            Vector2 direction = playerPos - transform.Position;
            float dist = direction.Length();
            if (dist > 15f)
            {
                direction.Normalize();
                velocity.Value = direction * enemy.Speed;
            }
            else
            {
                velocity.Value = Vector2.Zero;
            }

            // Procedural crawl animation: squish and stretch the shadow body
            // We oscillate bounding radius to give it an organic blob crawl look
            float baseRadius = 20f;
            body.BoundingRadius = baseRadius + (float)Math.Sin(enemy.AnimTime * 8f) * 4f;

            world.SetComponent(entityId, enemy);
            world.SetComponent(entityId, velocity);
            world.SetComponent(entityId, body);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        // Draw glowing red eyes in the center of the black shadow enemies
        foreach (var entityId in world.GetEntitiesWith<EnemyComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            // Glowing red eye offset slightly in movement direction (using body radius as reference)
            Vector2 eyePos = transform.Position;
            
            // Draw eye core
            ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, eyePos, 4f, Color.Red);
            // Draw eye glow
            ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, eyePos, 8f, new Color(255, 0, 0, 100));
        }
    }
}
