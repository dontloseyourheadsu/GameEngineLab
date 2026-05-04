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
}
