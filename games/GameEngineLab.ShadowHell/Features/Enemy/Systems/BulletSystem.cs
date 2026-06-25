using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.ShadowHell.Features.Player.Components;
using GameEngineLab.ShadowHell.Features.Enemy.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.ShadowHell.Features.Enemy.Systems;

public sealed class BulletSystem : IGameSystem
{
    public int Order => 102; // Runs after movement but before drawing

    private const float WorldWidth = 2048f;
    private const float WorldHeight = 1536f;
    private const float BoundaryMargin = 64f;

    public void Update(World world, FrameContext frameContext)
    {
        float dt = frameContext.DeltaSeconds;
        if (dt <= 0) return;

        // 1. Locate Player Component and Transform
        EntityId playerEntity = default;
        PlayerComponent player = default;
        TransformComponent playerTransform = default;
        bool playerFound = false;

        foreach (var entity in world.GetEntitiesWith<PlayerComponent, TransformComponent>())
        {
            world.TryGetComponent<PlayerComponent>(entity, out player);
            world.TryGetComponent<TransformComponent>(entity, out playerTransform);
            playerEntity = entity;
            playerFound = true;
            break;
        }

        // 2. Update Bullets
        foreach (var bulletEntity in world.GetEntitiesWith<BulletComponent, TransformComponent>())
        {
            world.TryGetComponent<BulletComponent>(bulletEntity, out var bullet);
            world.TryGetComponent<TransformComponent>(bulletEntity, out var transform);

            bullet.Lifetime -= dt;
            transform.Position += bullet.Velocity * dt;

            // Save position back to transform
            world.SetComponent(bulletEntity, transform);

            // Bullet expired or out of bounds (cavern wall boundary)
            if (bullet.Lifetime <= 0f || 
                transform.Position.X < BoundaryMargin || 
                transform.Position.X > WorldWidth - BoundaryMargin || 
                transform.Position.Y < BoundaryMargin || 
                transform.Position.Y > WorldHeight - BoundaryMargin)
            {
                world.DestroyEntity(bulletEntity);
                continue;
            }

            // Check collision with player (only if player is found, not rolling, and not invincible)
            if (playerFound)
            {
                bool isInvincible = player.InvincibilityTimer > 0f || player.State == PlayerState.Rolling;
                if (!isInvincible)
                {
                    float dist = Vector2.Distance(transform.Position, playerTransform.Position);
                    if (dist < 22f) // player radius (18) + bullet radius (4)
                    {
                        // Take damage (ranged bullets reduce half a life point)
                        player.Health = Math.Max(0f, player.Health - bullet.Damage);
                        player.InvincibilityTimer = 1.2f; // Invincibility frames

                        // Handle death/respawn
                        if (player.Health <= 0f)
                        {
                            player.Health = player.MaxHealth;
                            player.InvincibilityTimer = 2.0f; // longer invincibility on respawn
                            playerTransform.Position = new Vector2(WorldWidth / 2f, WorldHeight / 2f);
                            world.SetComponent(playerEntity, playerTransform);
                        }

                        world.SetComponent(playerEntity, player);
                        world.DestroyEntity(bulletEntity);
                        continue;
                    }
                }
            }

            world.SetComponent(bulletEntity, bullet);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        // Draw bullets as orange and black circles
        foreach (var bulletEntity in world.GetEntitiesWith<BulletComponent, TransformComponent>())
        {
            world.TryGetComponent<TransformComponent>(bulletEntity, out var transform);

            // Draw outer orange circle
            ShapeRenderer.DrawCircle(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                transform.Position, 
                6f, 
                new Color(255, 100, 0)
            );

            // Draw inner black core
            ShapeRenderer.DrawCircle(
                frameContext.SpriteBatch, 
                frameContext.DebugPixel, 
                transform.Position, 
                2f, 
                Color.Black
            );
        }
    }
}
