using Microsoft.Xna.Framework;
using System;

namespace GameEngineLab.Core.Features.Physics;

public static class Raycast
{
    public static bool IntersectRayLine(Vector2 origin, Vector2 direction, Vector2 p1, Vector2 p2, out float distance, out Vector2 normal)
    {
        distance = 0;
        normal = Vector2.Zero;

        var v1 = origin - p1;
        var v2 = p2 - p1;
        var v3 = new Vector2(-direction.Y, direction.X);

        float dot = Vector2.Dot(v2, v3);
        if (Math.Abs(dot) < 0.000001f) return false;

        float t1 = (v2.X * v1.Y - v2.Y * v1.X) / dot;
        float t2 = Vector2.Dot(v1, v3) / dot;

        if (t1 >= 0 && t2 >= 0 && t2 <= 1)
        {
            distance = t1;
            var edge = p2 - p1;
            normal = Vector2.Normalize(new Vector2(-edge.Y, edge.X));
            // Ensure normal points away from ray
            if (Vector2.Dot(normal, direction) > 0) normal = -normal;
            return true;
        }

        return false;
    }

    public static bool IntersectRayPolygon(Vector2 origin, Vector2 direction, Vector2[] vertices, out float distance, out Vector2 normal)
    {
        distance = float.MaxValue;
        normal = Vector2.Zero;
        bool hit = false;

        for (int i = 0; i < vertices.Length; i++)
        {
            var p1 = vertices[i];
            var p2 = vertices[(i + 1) % vertices.Length];

            if (IntersectRayLine(origin, direction, p1, p2, out float d, out Vector2 n))
            {
                if (d < distance)
                {
                    distance = d;
                    normal = n;
                    hit = true;
                }
            }
        }

        return hit;
    }

    public static bool IntersectRayCircle(Vector2 origin, Vector2 direction, Vector2 center, float radius, out float distance, out Vector2 normal)
    {
        distance = 0;
        normal = Vector2.Zero;

        var L = center - origin;
        float tca = Vector2.Dot(L, direction);
        if (tca < 0) return false;

        float d2 = Vector2.Dot(L, L) - tca * tca;
        if (d2 > radius * radius) return false;

        float thc = (float)Math.Sqrt(radius * radius - d2);
        distance = tca - thc;
        
        var hitPoint = origin + direction * distance;
        normal = Vector2.Normalize(hitPoint - center);

        return true;
    }
}
