using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Physics.Components;
using Microsoft.Xna.Framework;
using System;

namespace GameEngineLab.GolfIt.Features.Physics.Systems;

public sealed class SpringSystem : IGameSystem
{
    public int Order => 5; // Before movement

    public void Update(World world, FrameContext frameContext)
    {
        foreach (var entityId in world.GetEntitiesWith<SpringComponent, TransformComponent, VelocityComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<SpringComponent>(entityId, out var spring);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            var pos = transform.Position;
            var vel = velocity.Value;

            // F = -k * (x - L) - d * v
            var displacement = pos - spring.Anchor;
            var distance = displacement.Length();
            
            if (distance > 0)
            {
                var direction = displacement / distance;
                var forceMagnitude = -spring.Stiffness * (distance - spring.RestLength);
                var springForce = direction * forceMagnitude;
                
                var dampingForce = -spring.Damping * vel;
                
                var totalForce = springForce + dampingForce;
                
                // a = F / m
                var acceleration = totalForce / (body.Mass > 0 ? body.Mass : 1f);
                
                velocity.Value += acceleration * frameContext.DeltaSeconds;
                world.SetComponent(entityId, velocity);
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        foreach (var entityId in world.GetEntitiesWith<SpringComponent, TransformComponent>())
        {
            world.TryGetComponent<SpringComponent>(entityId, out var spring);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);

            ShapeRenderer.DrawLine(
                frameContext.SpriteBatch,
                frameContext.DebugPixel,
                spring.Anchor,
                transform.Position,
                Color.Gray * 0.5f,
                2);
            
            ShapeRenderer.DrawCircle(
                frameContext.SpriteBatch,
                frameContext.DebugPixel,
                spring.Anchor,
                4,
                Color.DarkSlateGray);
        }
    }
}
