using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.Physics.Systems;

public sealed class BoundarySystem : IGameSystem
{
    public int Order => 20;

    public void Update(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<MapBoundsResource>(out var bounds) || bounds == null) return;

        foreach (var entityId in world.GetEntitiesWith<RigidBodyComponent, TransformComponent, VelocityComponent>())
        {
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);

            var pos = transform.Position;
            var vel = velocity.Value;
            var radius = body.BoundingRadius;
            var area = bounds.PlayArea;

            bool bounced = false;

            if (pos.X - radius < area.Left)
            {
                pos.X = area.Left + radius;
                vel.X = -vel.X * body.Restitution;
                bounced = true;
            }
            else if (pos.X + radius > area.Right)
            {
                pos.X = area.Right - radius;
                vel.X = -vel.X * body.Restitution;
                bounced = true;
            }

            if (pos.Y - radius < area.Top)
            {
                pos.Y = area.Top + radius;
                vel.Y = -vel.Y * body.Restitution;
                bounced = true;
            }
            else if (pos.Y + radius > area.Bottom)
            {
                pos.Y = area.Bottom - radius;
                vel.Y = -vel.Y * body.Restitution;
                bounced = true;
            }

            if (bounced)
            {
                transform.Position = pos;
                velocity.Value = vel;
                world.SetComponent(entityId, transform);
                world.SetComponent(entityId, velocity);
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
