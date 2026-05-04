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
    public int Order => 20; // After Movement/PolygonUpdate, before Boundary

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
                world.TryGetComponent<RigidBodyComponent>(e2, out var body2);

                // Collision Filtering
                if (!CanCollide(body1, body2)) continue;

                world.TryGetComponent<TransformComponent>(e1, out var transform1);
                world.TryGetComponent<TransformComponent>(e2, out var transform2);

                if (body1.Shape == RigidBodyShape.Circle && body2.Shape == RigidBodyShape.Circle)
                {
                    ResolveCircleCircle(world, e1, body1, transform1, e2, body2, transform2);
                }
                else if (body1.Shape == RigidBodyShape.Circle && (body2.Shape == RigidBodyShape.Rectangle || body2.Shape == RigidBodyShape.Polygon))
                {
                    ResolveCirclePolygon(world, e1, body1, transform1, e2, body2, transform2);
                }
                else if ((body1.Shape == RigidBodyShape.Rectangle || body1.Shape == RigidBodyShape.Polygon) && body2.Shape == RigidBodyShape.Circle)
                {
                    ResolveCirclePolygon(world, e2, body2, transform2, e1, body1, transform1);
                }
                else
                {
                    // Polygon-Polygon or Rectangle-Polygon or Rectangle-Rectangle
                    ResolvePolygonPolygon(world, e1, body1, transform1, e2, body2, transform2);
                }
            }
        }
    }

    private bool CanCollide(RigidBodyComponent a, RigidBodyComponent b)
    {
        // Both static can't collide
        if (a.Mass == 0 && b.Mass == 0) return false;

        bool aCanHitB = (a.CollisionMask & (1 << b.CollisionGroup)) != 0;
        bool bCanHitA = (b.CollisionMask & (1 << a.CollisionGroup)) != 0;
        return aCanHitB && bCanHitA;
    }

    public void Draw(World world, FrameContext frameContext) { }

    private void ResolveCircleCircle(
        World world,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId e1, RigidBodyComponent b1, TransformComponent t1,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId e2, RigidBodyComponent b2, TransformComponent t2)
    {
        var diff = t2.Position - t1.Position;
        var distSq = diff.LengthSquared();
        var radiusSum = b1.BoundingRadius + b2.BoundingRadius;
        if (distSq < radiusSum * radiusSum)
        {
            var dist = (float)Math.Sqrt(distSq);
            var normal = dist > 0 ? diff / dist : new Vector2(1, 0);
            var penetration = radiusSum - dist;
            // normal points from e1 to e2
            ResolveCollision(world, e1, t1, b1, e2, t2, b2, normal, penetration);
        }
    }

    private void ResolveCirclePolygon(
        World world,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId circleId, RigidBodyComponent circleBody, TransformComponent circleTransform,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId polyId, RigidBodyComponent polyBody, TransformComponent polyTransform)
    {
        var vertices = GetVertices(world, polyId, polyBody, polyTransform);
        if (vertices == null || vertices.Length < 3) return;

        Vector2? mtvNormal = null;
        float mtvOverlap = float.MaxValue;

        // 1. Check polygon axes (edge normals)
        for (int i = 0; i < vertices.Length; i++)
        {
            var p1 = vertices[i];
            var p2 = vertices[(i + 1) % vertices.Length];
            var edge = p2 - p1;
            var axis = new Vector2(-edge.Y, edge.X);
            axis.Normalize();

            if (!TestAxis(vertices, circleTransform.Position, circleBody.BoundingRadius, axis, ref mtvNormal, ref mtvOverlap))
                return;
        }

        // 2. Check axis from closest vertex to circle center
        var closestVertex = vertices[0];
        float minDistSq = Vector2.DistanceSquared(circleTransform.Position, closestVertex);
        for (int i = 1; i < vertices.Length; i++)
        {
            float distSq = Vector2.DistanceSquared(circleTransform.Position, vertices[i]);
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                closestVertex = vertices[i];
            }
        }

        var vToC = circleTransform.Position - closestVertex;
        if (vToC != Vector2.Zero)
        {
            var axis = Vector2.Normalize(vToC);
            if (!TestAxis(vertices, circleTransform.Position, circleBody.BoundingRadius, axis, ref mtvNormal, ref mtvOverlap))
                return;
        }

        if (mtvNormal.HasValue)
        {
            // TestAxis logic ensured mtvNormal points from Poly to Circle
            ResolveCollision(world, polyId, polyTransform, polyBody, circleId, circleTransform, circleBody, mtvNormal.Value, mtvOverlap);
        }
    }

    private void ResolvePolygonPolygon(
        World world,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId e1, RigidBodyComponent b1, TransformComponent t1,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId e2, RigidBodyComponent b2, TransformComponent t2)
    {
        var v1 = GetVertices(world, e1, b1, t1);
        var v2 = GetVertices(world, e2, b2, t2);
        if (v1 == null || v2 == null || v1.Length == 0 || v2.Length == 0) return;

        Vector2? mtvNormal = null;
        float mtvOverlap = float.MaxValue;

        // Check axes of first polygon
        for (int i = 0; i < v1.Length; i++)
        {
            var edge = v1[(i + 1) % v1.Length] - v1[i];
            var axis = Vector2.Normalize(new Vector2(-edge.Y, edge.X));
            if (!TestAxis(v1, v2, axis, ref mtvNormal, ref mtvOverlap)) return;
        }

        // Check axes of second polygon
        for (int i = 0; i < v2.Length; i++)
        {
            var edge = v2[(i + 1) % v2.Length] - v2[i];
            var axis = Vector2.Normalize(new Vector2(-edge.Y, edge.X));
            if (!TestAxis(v1, v2, axis, ref mtvNormal, ref mtvOverlap)) return;
        }

        if (mtvNormal.HasValue)
        {
            // TestAxis ensured mtvNormal points from e1 to e2
            ResolveCollision(world, e1, t1, b1, e2, t2, b2, mtvNormal.Value, mtvOverlap);
        }
    }

    private bool TestAxis(Vector2[] v1, Vector2 circleCenter, float radius, Vector2 axis, ref Vector2? mtvNormal, ref float mtvOverlap)
    {
        ProjectPolygon(v1, axis, out float min1, out float max1);
        ProjectCircle(circleCenter, radius, axis, out float min2, out float max2);

        float overlap = Math.Min(max1, max2) - Math.Max(min1, min2);
        if (overlap <= 0) return false;

        if (overlap < mtvOverlap)
        {
            mtvOverlap = overlap;
            mtvNormal = axis;
            // Ensure normal points from polygon (v1) to circle (circleCenter)
            if (Vector2.Dot(axis, circleCenter - GetCentroid(v1)) < 0)
                mtvNormal = -axis;
        }
        return true;
    }

    private bool TestAxis(Vector2[] v1, Vector2[] v2, Vector2 axis, ref Vector2? mtvNormal, ref float mtvOverlap)
    {
        ProjectPolygon(v1, axis, out float min1, out float max1);
        ProjectPolygon(v2, axis, out float min2, out float max2);

        float overlap = Math.Min(max1, max2) - Math.Max(min1, min2);
        if (overlap <= 0) return false;

        if (overlap < mtvOverlap)
        {
            mtvOverlap = overlap;
            mtvNormal = axis;
            // Ensure normal points from v1 to v2
            if (Vector2.Dot(axis, GetCentroid(v2) - GetCentroid(v1)) < 0)
                mtvNormal = -axis;
        }
        return true;
    }

    private void ResolveCollision(
        World world,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId e1, TransformComponent t1, RigidBodyComponent b1,
        GameEngineLab.Core.Features.Ecs.Entities.EntityId e2, TransformComponent t2, RigidBodyComponent b2,
        Vector2 normal, float penetration)
    {
        // normal points from e1 to e2
        const float slop = 0.05f;
        const float percent = 0.5f; 
        
        float invM1 = b1.Mass > 0 ? 1f / b1.Mass : 0;
        float invM2 = b2.Mass > 0 ? 1f / b2.Mass : 0;
        float totalInvMass = invM1 + invM2;
        if (totalInvMass == 0) return;

        Vector2 correction = (Math.Max(penetration - slop, 0.0f) / totalInvMass) * percent * normal;
        
        t1.Position -= invM1 * correction;
        t2.Position += invM2 * correction;

        world.SetComponent(e1, t1);
        world.SetComponent(e2, t2);

        // Reflect velocities
        Vector2 v1Val = Vector2.Zero;
        Vector2 v2Val = Vector2.Zero;
        bool hasV1 = world.TryGetComponent<VelocityComponent>(e1, out var v1comp);
        bool hasV2 = world.TryGetComponent<VelocityComponent>(e2, out var v2comp);
        if (hasV1) v1Val = v1comp.Value;
        if (hasV2) v2Val = v2comp.Value;

        var relVel = v2Val - v1Val;
        float velAlongNormal = Vector2.Dot(relVel, normal);

        if (velAlongNormal < 0)
        {
            float e = Math.Min(b1.Restitution, b2.Restitution);
            float j = -(1 + e) * velAlongNormal;
            j /= totalInvMass;

            Vector2 impulse = j * normal;
            if (hasV1)
            {
                v1comp.Value -= invM1 * impulse;
                world.SetComponent(e1, v1comp);
            }
            if (hasV2)
            {
                v2comp.Value += invM2 * impulse;
                world.SetComponent(e2, v2comp);
            }
        }
    }

    private Vector2[] GetVertices(World world, GameEngineLab.Core.Features.Ecs.Entities.EntityId id, RigidBodyComponent body, TransformComponent transform)
    {
        if (body.Shape == RigidBodyShape.Polygon)
        {
            if (world.TryGetComponent<PolygonComponent>(id, out var poly)) return poly.TransformedVertices;
            return Array.Empty<Vector2>();
        }
        else if (body.Shape == RigidBodyShape.Rectangle)
        {
            var halfSize = body.Size / 2f;
            var v = new Vector2[4];
            v[0] = new Vector2(-halfSize.X, -halfSize.Y);
            v[1] = new Vector2(halfSize.X, -halfSize.Y);
            v[2] = new Vector2(halfSize.X, halfSize.Y);
            v[3] = new Vector2(-halfSize.X, halfSize.Y);

            float cos = (float)Math.Cos(transform.Rotation);
            float sin = (float)Math.Sin(transform.Rotation);

            for (int i = 0; i < 4; i++)
            {
                float rx = v[i].X * cos - v[i].Y * sin;
                float ry = v[i].X * sin + v[i].Y * cos;
                v[i] = new Vector2(rx, ry) + transform.Position;
            }
            return v;
        }
        return Array.Empty<Vector2>();
    }

    private void ProjectPolygon(Vector2[] vertices, Vector2 axis, out float min, out float max)
    {
        min = float.MaxValue;
        max = float.MinValue;
        for (int i = 0; i < vertices.Length; i++)
        {
            float dot = Vector2.Dot(vertices[i], axis);
            if (dot < min) min = dot;
            if (dot > max) max = dot;
        }
    }

    private void ProjectCircle(Vector2 center, float radius, Vector2 axis, out float min, out float max)
    {
        float dot = Vector2.Dot(center, axis);
        min = dot - radius;
        max = dot + radius;
    }

    private Vector2 GetCentroid(Vector2[] vertices)
    {
        if (vertices.Length == 0) return Vector2.Zero;
        Vector2 sum = Vector2.Zero;
        for (int i = 0; i < vertices.Length; i++) sum += vertices[i];
        return sum / vertices.Length;
    }
}
