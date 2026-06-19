using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.Core.Features.Rendering.Resources;

public static class ShapeRenderer
{
    public static void DrawCircle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float radius, Color color, int thickness = 1)
    {
        // Simple pixel-based circle rendering using a midpoint-like approach or just drawing points
        // For a filled circle, we can iterate rows
        int r = (int)radius;
        for (int y = -r; y <= r; y++)
        {
            int xSpan = (int)Math.Sqrt(r * r - y * y);
            var rect = new Rectangle((int)center.X - xSpan, (int)center.Y + y, xSpan * 2, 1);
            spriteBatch.Draw(pixel, rect, color);
        }
    }

    public static void DrawCircleOutline(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, float radius, Color color, int thickness = 1)
    {
        double step = 1.0 / radius;
        for (double a = 0; a < Math.PI * 2; a += step)
        {
            var pos = center + new Vector2((float)Math.Cos(a) * radius, (float)Math.Sin(a) * radius);
            spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y, thickness, thickness), color);
        }
    }

    public static void DrawRectangle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Vector2 size, Color color)
    {
        var rect = new Rectangle(
            (int)(center.X - size.X / 2f),
            (int)(center.Y - size.Y / 2f),
            (int)size.X,
            (int)size.Y);
        spriteBatch.Draw(pixel, rect, color);
    }

    public static void DrawRectangle(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Vector2 size, float rotation, Color color)
    {
        spriteBatch.Draw(pixel,
            new Rectangle((int)center.X, (int)center.Y, (int)size.X, (int)size.Y),
            null,
            color,
            rotation,
            new Vector2(0.5f, 0.5f), 
            SpriteEffects.None,
            0);
    }

    public static void DrawEllipse(SpriteBatch spriteBatch, Texture2D pixel, Vector2 center, Vector2 size, Color color)
    {
        float rx = size.X / 2f;
        float ry = size.Y / 2f;
        if (rx <= 0 || ry <= 0) return;

        for (int y = -(int)ry; y <= (int)ry; y++)
        {
            float xSpan = rx * (float)Math.Sqrt(Math.Max(0, 1 - (y * y) / (ry * ry)));
            var rect = new Rectangle((int)(center.X - xSpan), (int)(center.Y + y), (int)(xSpan * 2), 1);
            spriteBatch.Draw(pixel, rect, color);
        }
    }

    public static void DrawLine(SpriteBatch spriteBatch, Texture2D pixel, Vector2 start, Vector2 end, Color color, int thickness = 1)
    {
        var edge = end - start;
        var angle = (float)Math.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(pixel,
            new Rectangle(
                (int)start.X,
                (int)start.Y,
                (int)edge.Length(),
                thickness),
            null,
            color,
            angle,
            Vector2.Zero,
            SpriteEffects.None,
            0);
    }

    public static void DrawPolygon(SpriteBatch spriteBatch, Texture2D pixel, Vector2[] vertices, Color color, int thickness = 1)
    {
        if (vertices == null || vertices.Length < 2) return;

        for (int i = 0; i < vertices.Length; i++)
        {
            var start = vertices[i];
            var end = vertices[(i + 1) % vertices.Length];
            DrawLine(spriteBatch, pixel, start, end, color, thickness);
        }
    }

    public static void DrawFilledPolygon(SpriteBatch spriteBatch, Texture2D pixel, Vector2[] vertices, Color color)
    {
        if (vertices == null || vertices.Length < 3) return;

        // Find Y bounding range
        float minY = vertices[0].Y;
        float maxY = vertices[0].Y;
        for (int i = 1; i < vertices.Length; i++)
        {
            if (vertices[i].Y < minY) minY = vertices[i].Y;
            if (vertices[i].Y > maxY) maxY = vertices[i].Y;
        }

        int startY = (int)Math.Floor(minY);
        int endY = (int)Math.Ceiling(maxY);

        // Rasterize row by row
        for (int y = startY; y <= endY; y++)
        {
            float xMin = float.MaxValue;
            float xMax = float.MinValue;
            int intersects = 0;

            for (int i = 0; i < vertices.Length; i++)
            {
                var p1 = vertices[i];
                var p2 = vertices[(i + 1) % vertices.Length];

                // Detect intersection with horizontal line at y
                if ((p1.Y <= y && p2.Y > y) || (p2.Y <= y && p1.Y > y))
                {
                    float t = (y - p1.Y) / (p2.Y - p1.Y);
                    float x = p1.X + t * (p2.X - p1.X);

                    if (x < xMin) xMin = x;
                    if (x > xMax) xMax = x;
                    intersects++;
                }
            }

            if (intersects >= 2)
            {
                int ixMin = (int)Math.Floor(xMin);
                int ixMax = (int)Math.Ceiling(xMax);
                if (ixMax > ixMin)
                {
                    var rect = new Rectangle(ixMin, y, ixMax - ixMin, 1);
                    spriteBatch.Draw(pixel, rect, color);
                }
            }
        }
    }
}

