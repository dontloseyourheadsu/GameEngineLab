using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Physics.Resources;
using Microsoft.Xna.Framework;
using System;

namespace GameEngineLab.Core.Features.Physics.Systems;

public sealed class ZoneSystem : IGameSystem
{
    public int Order => 35; // Run after all resolution

    public void Update(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<ActionQueueResource>(out var actionQueue) || actionQueue == null)
        {
            actionQueue = new ActionQueueResource();
            world.SetResource(actionQueue);
        }

        var zones = world.GetEntitiesWith<TriggerZoneComponent, TransformComponent, RigidBodyComponent>();
        var entities = world.GetEntitiesWith<TransformComponent, RigidBodyComponent>();

        foreach (var zoneId in zones)
        {
            world.TryGetComponent<TriggerZoneComponent>(zoneId, out var zone);
            world.TryGetComponent<TransformComponent>(zoneId, out var zoneTransform);
            world.TryGetComponent<RigidBodyComponent>(zoneId, out var zoneBody);

            foreach (var entityId in entities)
            {
                if (zoneId == entityId) continue;
                if (world.HasComponent<TriggerZoneComponent>(entityId)) continue;

                world.TryGetComponent<TransformComponent>(entityId, out var entityTransform);
                world.TryGetComponent<RigidBodyComponent>(entityId, out var entityBody);

                if (IsColliding(zoneTransform, zoneBody, entityTransform, entityBody))
                {
                    actionQueue.Enqueue(zone.ActionId);
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }

    private bool IsColliding(TransformComponent t1, RigidBodyComponent b1, TransformComponent t2, RigidBodyComponent b2)
    {
        if (b1.Shape == RigidBodyShape.Rectangle && b2.Shape == RigidBodyShape.Rectangle)
        {
            var r1 = new Rectangle((int)(t1.Position.X - b1.Size.X / 2), (int)(t1.Position.Y - b1.Size.Y / 2), (int)b1.Size.X, (int)b1.Size.Y);
            var r2 = new Rectangle((int)(t2.Position.X - b2.Size.X / 2), (int)(t2.Position.Y - b2.Size.Y / 2), (int)b2.Size.X, (int)b2.Size.Y);
            return r1.Intersects(r2);
        }
        else if (b1.Shape == RigidBodyShape.Circle && b2.Shape == RigidBodyShape.Circle)
        {
            return Vector2.Distance(t1.Position, t2.Position) < (b1.BoundingRadius + b2.BoundingRadius);
        }
        else
        {
            // Mixed Rectangle/Circle
            var rectTransform = b1.Shape == RigidBodyShape.Rectangle ? t1 : t2;
            var rectBody = b1.Shape == RigidBodyShape.Rectangle ? b1 : b2;
            var circleTransform = b1.Shape == RigidBodyShape.Rectangle ? t2 : t1;
            var circleBody = b1.Shape == RigidBodyShape.Rectangle ? b2 : b1;

            var rect = new Rectangle((int)(rectTransform.Position.X - rectBody.Size.X / 2), (int)(rectTransform.Position.Y - rectBody.Size.Y / 2), (int)rectBody.Size.X, (int)rectBody.Size.Y);
            
            // Closest point on rectangle to circle center
            float closestX = Math.Clamp(circleTransform.Position.X, rect.Left, rect.Right);
            float closestY = Math.Clamp(circleTransform.Position.Y, rect.Top, rect.Bottom);

            float distance = Vector2.Distance(circleTransform.Position, new Vector2(closestX, closestY));
            return distance < circleBody.BoundingRadius;
        }
    }
}
