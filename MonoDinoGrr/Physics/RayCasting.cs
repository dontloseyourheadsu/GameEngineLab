using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MonoDinoGrr.Physics
{
    public static class RayCasting
    {
        private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                               q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        private static int Orientation(Vector2 p, Vector2 q, Vector2 r)
        {
            var val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0; // collinear

            return (val > 0) ? 1 : 2; // clockwise or counterclockwise
        }

        private static Vector2 DoIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            var o1 = Orientation(p1, q1, p2);
            var o2 = Orientation(p1, q1, q2);
            var o3 = Orientation(p2, q2, p1);
            var o4 = Orientation(p2, q2, q1);

            if (o1 != o2 && o3 != o4)
            {
                var intersectionX = ((p1.X * q1.Y - p1.Y * q1.X) * (p2.X - q2.X) -
                                                        (p1.X - q1.X) * (p2.X * q2.Y - p2.Y * q2.X)) /
                                    ((p1.X - q1.X) * (p2.Y - q2.Y) - (p1.Y - q1.Y) * (p2.X - q2.X));

                var intersectionY = ((p1.X * q1.Y - p1.Y * q1.X) * (p2.Y - q2.Y) -
                                                        (p1.Y - q1.Y) * (p2.X * q2.Y - p2.Y * q2.X)) /
                                    ((p1.X - q1.X) * (p2.Y - q2.Y) - (p1.Y - q1.Y) * (p2.X - q2.X));

                return new Vector2(intersectionX, intersectionY);
            }

            return new Vector2(0,0);
        }

        public static int GetRayCastingCount(Vector2 particle, List<Stick> sticks, float rayLimit)
        {
            var rayVector = new Vector2(rayLimit, particle.Y);

            var count = 0;
            for (var i = 0; i < sticks.Count; i++)
            {
                var intersectsLine = DoIntersect(
                    particle,
                    rayVector,
                    sticks[i].A.Position, sticks[i].B.Position);

                if (intersectsLine.X != 0 && intersectsLine.Y != 0)
                {
                    count++;
                }
            }

            return count;
        }

        public static (Stick, Vector2) GetClosestEdge(Vector2 particle, List<Stick> sticks)
        {
            var minDistance = float.MaxValue;
            Stick closestEdge = null;
            Vector2 closestPoint = new Vector2(0, 0);

            for (var i = 0; i < sticks.Count; i++)
            {
                // check intersection towards the center of the edge
                var edgeCenter = new Vector2((sticks[i].A.Position.X + sticks[i].B.Position.X) / 2 + 5,
                                                                (sticks[i].A.Position.Y + sticks[i].B.Position.Y) / 2 + 5);

                var intersectsLine = DoIntersect(
                    particle, edgeCenter, sticks[i].A.Position, sticks[i].B.Position);

                if (intersectsLine.X != 0 && intersectsLine.Y != 0)
                {
                    var distance = GetDistance(particle, intersectsLine);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestEdge = sticks[i];
                        closestPoint = intersectsLine;
                    }
                }
            }

            return (closestEdge, closestPoint);
        }

        private static float GetDistance(Vector2 particle, Vector2 newIntersectsLine)
        {
            return (float)Math.Sqrt(Math.Pow(particle.X - newIntersectsLine.X, 2) + Math.Pow(particle.Y - newIntersectsLine.Y, 2));
        }
    }
}
