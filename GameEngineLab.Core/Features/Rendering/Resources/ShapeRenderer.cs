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
}
