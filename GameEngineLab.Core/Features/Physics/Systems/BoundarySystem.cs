using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.Physics.Systems;

public sealed class BoundarySystem : IGameSystem
{
    public int Order => 25;

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
            var area = bounds.PlayArea;

            bool bounced = false;

            if (body.Shape == RigidBodyShape.Circle)
            {
                var radius = body.BoundingRadius;

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
            }
            else if (body.Shape == RigidBodyShape.Rectangle)
            {
                var halfSize = body.Size / 2f;

                if (pos.X - halfSize.X < area.Left)
                {
                    pos.X = area.Left + halfSize.X;
                    vel.X = -vel.X * body.Restitution;
                    bounced = true;
                }
                else if (pos.X + halfSize.X > area.Right)
                {
                    pos.X = area.Right - halfSize.X;
                    vel.X = -vel.X * body.Restitution;
                    bounced = true;
                }

                if (pos.Y - halfSize.Y < area.Top)
                {
                    pos.Y = area.Top + halfSize.Y;
                    vel.Y = -vel.Y * body.Restitution;
                    bounced = true;
                }
                else if (pos.Y + halfSize.Y > area.Bottom)
                {
                    pos.Y = area.Bottom - halfSize.Y;
                    vel.Y = -vel.Y * body.Restitution;
                    bounced = true;
                }
            }
            else if (body.Shape == RigidBodyShape.Polygon)
            {
                if (world.TryGetComponent<PolygonComponent>(entityId, out var polygon))
                {
                    var vertices = polygon.TransformedVertices;
                    if (vertices != null && vertices.Length > 0)
                    {
                        float minX = float.MaxValue, maxX = float.MinValue;
                        float minY = float.MaxValue, maxY = float.MinValue;
                        foreach (var v in vertices)
                        {
                            if (v.X < minX) minX = v.X;
                            if (v.X > maxX) maxX = v.X;
                            if (v.Y < minY) minY = v.Y;
                            if (v.Y > maxY) maxY = v.Y;
                        }

                        if (minX < area.Left) { pos.X += (area.Left - minX); vel.X = -vel.X * body.Restitution; bounced = true; }
                        else if (maxX > area.Right) { pos.X -= (maxX - area.Right); vel.X = -vel.X * body.Restitution; bounced = true; }

                        if (minY < area.Top) { pos.Y += (area.Top - minY); vel.Y = -vel.Y * body.Restitution; bounced = true; }
                        else if (maxY > area.Bottom) { pos.Y -= (maxY - area.Bottom); vel.Y = -vel.Y * body.Restitution; bounced = true; }
                    }
                }
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
