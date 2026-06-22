using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.ShadowHell.Features.Player.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameEngineLab.ShadowHell.Features.Player.Systems;

public sealed class PlayerInputSystem : IGameSystem
{
    public int Order => 1; // Runs early, before physics

    public void Update(World world, FrameContext frameContext)
    {
        float dt = frameContext.DeltaSeconds;
        if (dt <= 0) return;

        var kState = frameContext.CurrentKeyboard;
        var prevKState = frameContext.PreviousKeyboard;

        foreach (var entityId in world.GetEntitiesWith<PlayerComponent, VelocityComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<PlayerComponent>(entityId, out var player);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            player.AnimationTime += dt;

            // 1. Standing constraint: The player NEVER rotates (Isaac-style)
            transform.Rotation = 0f;

            // 2. Read directional movement inputs
            Vector2 moveDir = Vector2.Zero;
            if (kState.IsKeyDown(Keys.W) || kState.IsKeyDown(Keys.Up)) moveDir.Y -= 1f;
            if (kState.IsKeyDown(Keys.S) || kState.IsKeyDown(Keys.Down)) moveDir.Y += 1f;
            if (kState.IsKeyDown(Keys.A) || kState.IsKeyDown(Keys.Left)) moveDir.X -= 1f;
            if (kState.IsKeyDown(Keys.D) || kState.IsKeyDown(Keys.Right)) moveDir.X += 1f;

            if (moveDir != Vector2.Zero)
            {
                moveDir.Normalize();
                player.MovementDirection = moveDir;
                
                // Track facing direction (Left vs Right) for skeleton flipping
                if (moveDir.X > 0.01f) player.FacingRight = true;
                else if (moveDir.X < -0.01f) player.FacingRight = false;
            }
            else
            {
                player.MovementDirection = Vector2.Zero;
            }

            // 3. Handle Flight/Hover Trigger (Space bar toggle)
            bool spaceJustPressed = kState.IsKeyDown(Keys.Space) && prevKState.IsKeyUp(Keys.Space);
            
            if (player.State == PlayerState.Flying)
            {
                // Active Flight State
                player.FlightTimer -= dt;
                
                // Smoothly lift up to hover height using rate limiting to prevent teleporting
                float targetHoverZ = 32f + (float)Math.Sin(player.AnimationTime * 7f) * 4f;
                float maxLiftChange = 180f * dt;
                player.JumpZ = MathHelper.Clamp(targetHoverZ, player.JumpZ - maxLiftChange, player.JumpZ + maxLiftChange);

                // Move at slightly elevated speeds in flight
                velocity.Value = player.MovementDirection * player.NormalSpeed * 1.15f;

                // Deactivate flight if timer expires or space pressed again (with cooldown to prevent X11 auto-repeat) or roll triggered
                bool rollTriggered = (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift)) &&
                                     (prevKState.IsKeyUp(Keys.LeftShift) && prevKState.IsKeyUp(Keys.RightShift));

                bool spaceToggle = spaceJustPressed && (player.FlightDuration - player.FlightTimer > 0.25f);

                if (player.FlightTimer <= 0f || spaceToggle || rollTriggered)
                {
                    // Descend back to ground
                    player.State = PlayerState.Idle;
                }
            }
            else if (player.State == PlayerState.Rolling)
            {
                // Dodge Rolling state
                player.RollTimer -= dt;
                velocity.Value = player.RollDirection * player.RollSpeed;
                
                // Decelerate height back to ground if we rolled out of air
                if (player.JumpZ > 0)
                {
                    player.JumpZ = Math.Max(0f, player.JumpZ - 120f * dt);
                }

                if (player.RollTimer <= 0f)
                {
                    player.JumpZ = 0f;
                    player.State = player.MovementDirection != Vector2.Zero ? PlayerState.Walking : PlayerState.Idle;
                }
            }
            else
            {
                // Ground State (Idle / Walking)
                // Smoothly lower height if descending
                if (player.JumpZ > 0f)
                {
                    player.JumpZ = Math.Max(0f, player.JumpZ - 180f * dt);
                }

                if (player.MovementDirection != Vector2.Zero)
                {
                    player.State = PlayerState.Walking;
                    velocity.Value = player.MovementDirection * player.NormalSpeed;
                }
                else
                {
                    player.State = PlayerState.Idle;
                    velocity.Value = Vector2.Zero;
                }

                // Trigger Flight from ground (only if fully grounded to prevent mid-air reactivation and reset)
                if (spaceJustPressed && player.JumpZ <= 0.001f)
                {
                    player.State = PlayerState.Flying;
                    player.FlightTimer = player.FlightDuration;
                    player.JumpStartPos = transform.Position;
                }

                // Trigger Roll from ground
                bool shiftJustPressed = (kState.IsKeyDown(Keys.LeftShift) || kState.IsKeyDown(Keys.RightShift)) &&
                                        (prevKState.IsKeyUp(Keys.LeftShift) && prevKState.IsKeyUp(Keys.RightShift));
                if (shiftJustPressed)
                {
                    player.State = PlayerState.Rolling;
                    player.RollTimer = player.RollDuration;
                    player.RollDirection = player.MovementDirection != Vector2.Zero ? player.MovementDirection : 
                        new Vector2(player.FacingRight ? 1f : -1f, 0f);
                }
            }

            // 4. Ground attack immunity during flight
            // Adjust collision groups or masks so that when player is flying (player.JumpZ > 20f), 
            // enemies (CollisionGroup 2) ignore collisions, but player still collides with walls (CollisionGroup 1).
            // Let's implement this!
            // Walls are Static (Mass = 0), Enemies are Dynamic (Mass = 1.2).
            // In our cavern generator, we can set CollisionGroups:
            // - Cavern walls & pillars: CollisionGroup = 1 (Static cavern structures)
            // - Player: CollisionGroup = 4
            // - Enemies: CollisionGroup = 2
            // Normally, Player collides with both walls (1) and enemies (2).
            // When flying, Player changes CollisionMask to only collide with walls (1), ignoring enemies (2)!
            // This is mathematically elegant and fully resolves the flight gameplay mechanics!
            if (player.JumpZ > 20f || player.State == PlayerState.Rolling)
            {
                // Only collide with walls (Group 1)
                body.CollisionMask = 1;
            }
            else
            {
                // Collide with both walls (Group 1) and enemies (Group 2)
                body.CollisionMask = 1 | 2;
            }

            // Save components back
            world.SetComponent(entityId, player);
            world.SetComponent(entityId, velocity);
            world.SetComponent(entityId, transform);
            world.SetComponent(entityId, body);
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
