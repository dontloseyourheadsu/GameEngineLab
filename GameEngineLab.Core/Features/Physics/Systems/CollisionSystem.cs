using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineLab.Core.Features.Physics.Systems;

public sealed class CollisionSystem : IGameSystem
{
    public int Order => 25; // After Movement, before Boundary/Friction (or around there)

    public void Update(World world, FrameContext frameContext)
    {
        var entities = world.GetEntitiesWith<RigidBodyComponent, TransformComponent>().ToList();

        for (int i = 0; i < entities.Count; i++)
        {
            for (int j = i + 1; j < entities.Count; j++)
            {
                var e1 = entities[i];
                var e2 = entities[j];

                world.TryGetComponent<RigidBodyComponent>(e1, out var body1);
                world.TryGetComponent<TransformComponent>(e1, out var transform1);
                world.TryGetComponent<RigidBodyComponent>(e2, out var body2);
                world.TryGetComponent<TransformComponent>(e2, out var transform2);

                if (body1.Shape == RigidBodyShape.Circle && body2.Shape == RigidBodyShape.Rectangle)
                {
                    ResolveCircleRectangle(world, e1, body1, transform1, e2, body2, transform2);
                }
                else if (body1.Shape == RigidBodyShape.Rectangle && body2.Shape == RigidBodyShape.Circle)
                {
                    ResolveCircleRectangle(world, e2, body2, transform2, e1, body1, transform1);
                }
                // We could add Circle-Circle or Rectangle-Rectangle here if needed
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }

    private void ResolveCircleRectangle(
        World world,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId circleId, RigidBodyComponent circleBody, TransformComponent circleTransform,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId rectId, RigidBodyComponent rectBody, TransformComponent rectTransform)
    {
        var circlePos = circleTransform.Position;
        var rectPos = rectTransform.Position;
        var halfSize = rectBody.Size / 2f;

        // Find the closest point on the rectangle to the circle center
        var closestX = Math.Clamp(circlePos.X, rectPos.X - halfSize.X, rectPos.X + halfSize.X);
        var closestY = Math.Clamp(circlePos.Y, rectPos.Y - halfSize.Y, rectPos.Y + halfSize.Y);

        var distanceX = circlePos.X - closestX;
        var distanceY = circlePos.Y - closestY;
        var distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

        if (distanceSquared < (circleBody.BoundingRadius * circleBody.BoundingRadius))
        {
            var distance = (float)Math.Sqrt(distanceSquared);
            Vector2 normal;
            float penetration;

            if (distance > 0)
            {
                normal = new Vector2(distanceX / distance, distanceY / distance);
                penetration = circleBody.BoundingRadius - distance;
            }
            else
            {
                // Circle center is inside the rectangle
                // Find the shallowest axis to push out along
                var dx1 = circlePos.X - (rectPos.X - halfSize.X);
                var dx2 = (rectPos.X + halfSize.X) - circlePos.X;
                var dy1 = circlePos.Y - (rectPos.Y - halfSize.Y);
                var dy2 = (rectPos.Y + halfSize.Y) - circlePos.Y;

                var minX = Math.Min(dx1, dx2);
                var minY = Math.Min(dy1, dy2);

                if (minX < minY)
                {
                    normal = dx1 < dx2 ? new Vector2(-1, 0) : new Vector2(1, 0);
                    penetration = circleBody.BoundingRadius + minX;
                }
                else
                {
                    normal = dy1 < dy2 ? new Vector2(0, -1) : new Vector2(0, 1);
                    penetration = circleBody.BoundingRadius + minY;
                }
            }

            // Simple collision resolution (assuming rectangle is static/heavy for now)
            circleTransform.Position += normal * penetration;
            world.SetComponent(circleId, circleTransform);

            if (world.TryGetComponent<VelocityComponent>(circleId, out var velocity))
            {
                // Reflect velocity
                var dot = Vector2.Dot(velocity.Value, normal);
                if (dot < 0)
                {
                    velocity.Value -= 2 * dot * normal;
                    velocity.Value *= (circleBody.Restitution + rectBody.Restitution) / 2f;
                    world.SetComponent(circleId, velocity);
                }
            }
        }
    }
}
