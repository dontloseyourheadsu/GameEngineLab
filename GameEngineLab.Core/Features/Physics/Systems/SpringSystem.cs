using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using Microsoft.Xna.Framework;
using System;

namespace GameEngineLab.Core.Features.Physics.Systems;

public sealed class SpringSystem : IGameSystem
{
    public int Order => 5; // Before movement

    public void Update(World world, FrameContext frameContext)
    {
        // Anchor Springs
        foreach (var entityId in world.GetEntitiesWith<SpringComponent, TransformComponent, VelocityComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<SpringComponent>(entityId, out var spring);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);

            var pos = transform.Position;
            var vel = velocity.Value;

            var displacement = pos - spring.Anchor;
            var distance = displacement.Length();
            
            if (distance > 0)
            {
                var direction = displacement / distance;
                var forceMagnitude = -spring.Stiffness * (distance - spring.RestLength);
                var springForce = direction * forceMagnitude;
                var dampingForce = -spring.Damping * vel;
                var totalForce = springForce + dampingForce;
                
                var acceleration = totalForce / (body.Mass > 0 ? body.Mass : 1f);
                velocity.Value += acceleration * frameContext.DeltaSeconds;
                world.SetComponent(entityId, velocity);
            }
        }

        // Distance Springs
        foreach (var springId in world.GetEntitiesWith<DistanceSpringComponent>())
        {
            world.TryGetComponent<DistanceSpringComponent>(springId, out var spring);

            if (!world.TryGetComponent<TransformComponent>(spring.EntityA, out var t1) ||
                !world.TryGetComponent<TransformComponent>(spring.EntityB, out var t2) ||
                !world.TryGetComponent<VelocityComponent>(spring.EntityA, out var v1) ||
                !world.TryGetComponent<VelocityComponent>(spring.EntityB, out var v2) ||
                !world.TryGetComponent<RigidBodyComponent>(spring.EntityA, out var b1) ||
                !world.TryGetComponent<RigidBodyComponent>(spring.EntityB, out var b2))
                continue;

            var pos1 = t1.Position;
            var pos2 = t2.Position;
            var vel1 = v1.Value;
            var vel2 = v2.Value;

            var displacement = pos1 - pos2;
            var distance = displacement.Length();
            if (distance <= 0) continue;

            float restLength = spring.RestLength < 0 ? 0 : spring.RestLength;
            var direction = displacement / distance;
            
            // F = -k * (x - L) - d * (v1 - v2)
            var forceMagnitude = -spring.Stiffness * (distance - restLength);
            var springForce = direction * forceMagnitude;
            var dampingForce = -spring.Damping * (vel1 - vel2);
            var totalForce = springForce + dampingForce;

            if (b1.Mass > 0)
            {
                v1.Value += (totalForce / b1.Mass) * frameContext.DeltaSeconds;
                world.SetComponent(spring.EntityA, v1);
            }
            if (b2.Mass > 0)
            {
                v2.Value -= (totalForce / b2.Mass) * frameContext.DeltaSeconds;
                world.SetComponent(spring.EntityB, v2);
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

        foreach (var springId in world.GetEntitiesWith<DistanceSpringComponent>())
        {
            world.TryGetComponent<DistanceSpringComponent>(springId, out var spring);
            if (world.TryGetComponent<TransformComponent>(spring.EntityA, out var t1) &&
                world.TryGetComponent<TransformComponent>(spring.EntityB, out var t2))
            {
                ShapeRenderer.DrawLine(
                    frameContext.SpriteBatch,
                    frameContext.DebugPixel,
                    t1.Position,
                    t2.Position,
                    Color.LightBlue * 0.5f,
                    1);
            }
        }
    }
}
