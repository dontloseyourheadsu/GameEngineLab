using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GameEngineLab.ShadowHell.Features.Environment.Resources;

public static class NoiseTextureGenerator
{
    public static Texture2D GenerateFloorTexture(GraphicsDevice graphics, int size)
    {
        Texture2D texture = new Texture2D(graphics, size, size);
        Color[] data = new Color[size * size];

        // Saturated, stylized retro colors (Celeste / Binding of Isaac style)
        Color baseColor = new Color(55, 34, 30);      // Rich dark terracotta floor
        Color borderColor = new Color(24, 12, 12);    // Bold dark boundary lines
        Color specColor = new Color(90, 52, 45);      // Warm hand-drawn highlight
        Color detailColor = new Color(42, 24, 22);    // Shadowed floor accent

        int tileSize = 64;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int tx = x % tileSize;
                int ty = y % tileSize;

                // Bold dark tile borders (2px)
                if (tx < 2 || ty < 2 || tx > tileSize - 3 || ty > tileSize - 3)
                {
                    data[y * size + x] = borderColor;
                }
                // Inline highlights (top/left) for cel-shaded bevel effect
                else if (tx == 2 || ty == 2)
                {
                    data[y * size + x] = specColor;
                }
                // Inline shadows (bottom/right) for depth
                else if (tx == tileSize - 3 || ty == tileSize - 3)
                {
                    data[y * size + x] = detailColor;
                }
                else
                {
                    // Stylized floor rock cracks and accents (Celeste temple feel)
                    // Crack 1: diagonal line on top-left
                    if (tx + ty == 24 || tx + ty == 25)
                    {
                        data[y * size + x] = borderColor;
                    }
                    // Crack 2: bottom-right diagonal notch
                    else if (tx - ty == 30 && tx > 40)
                    {
                        data[y * size + x] = borderColor;
                    }
                    // Stylized pebbles
                    else if ((tx >= 15 && tx <= 18 && ty >= 35 && ty <= 37) || 
                             (tx >= 45 && tx <= 47 && ty >= 12 && ty <= 14))
                    {
                        // Draw a small 2D rock block
                        if (tx == 15 || tx == 18 || ty == 35 || ty == 37 ||
                            tx == 45 || tx == 47 || ty == 12 || ty == 14)
                            data[y * size + x] = borderColor;
                        else
                            data[y * size + x] = specColor;
                    }
                    else
                    {
                        // Dynamic stylized noise/dithering pattern using deterministic hash
                        int hash = (x * 37 + y * 57) % 19;
                        if (hash == 0)
                            data[y * size + x] = detailColor; // shadow speck
                        else if (hash == 1)
                            data[y * size + x] = specColor;   // light speck
                        else
                            data[y * size + x] = baseColor;
                    }
                }
            }
        }

        texture.SetData(data);
        return texture;
    }

    public static Texture2D GenerateWallTexture(GraphicsDevice graphics, int size)
    {
        Texture2D texture = new Texture2D(graphics, size, size);
        Color[] data = new Color[size * size];

        // Saturated, stylized obsidian/brick dungeon colors
        Color baseColor = new Color(42, 28, 48);      // Dark purple brick face
        Color outlineColor = new Color(20, 10, 24);   // Dark violet mortar line
        Color highlightColor = new Color(85, 55, 95); // Stylized bevel highlight
        Color shadowColor = new Color(30, 18, 36);    // Dark brick shadow accent

        int brickW = 32;
        int brickH = 16;

        for (int y = 0; y < size; y++)
        {
            int row = y / brickH;
            int offsetX = (row % 2 == 0) ? 0 : brickW / 2;

            for (int x = 0; x < size; x++)
            {
                int bx = (x + offsetX) % brickW;
                int by = y % brickH;

                // Bold dark mortar outlines
                if (by < 2 || bx < 2)
                {
                    data[y * size + x] = outlineColor;
                }
                // Bevel highlight on top/left edges
                else if (by == 2 || bx == 2)
                {
                    data[y * size + x] = highlightColor;
                }
                // Bevel shadow on bottom/right edges
                else if (by == brickH - 1 || bx == brickW - 1)
                {
                    data[y * size + x] = shadowColor;
                }
                else
                {
                    // Stylized brick cracks and erosion detail
                    if (bx - by == 12 && bx < 20)
                    {
                        data[y * size + x] = outlineColor;
                    }
                    else
                    {
                        // Clean, cel-shaded texture look with minor grit specs
                        int specHash = (x * 13 + y * 27) % 37;
                        if (specHash == 0)
                            data[y * size + x] = highlightColor;
                        else if (specHash == 1)
                            data[y * size + x] = shadowColor;
                        else
                            data[y * size + x] = baseColor;
                    }
                }
            }
        }

        texture.SetData(data);
        return texture;
    }

    public static Texture2D GenerateLightTexture(GraphicsDevice graphics, int size)
    {
        Texture2D texture = new Texture2D(graphics, size, size);
        Color[] data = new Color[size * size];

        float center = size / 2.0f;
        // Make the light aura a tight circle well within the boundaries to prevent quad edge cutting (squares)
        float maxDist = size * 0.35f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - center;
                float dy = y + 0.5f - center;
                float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                float glow = 0.0f;
                if (dist < maxDist)
                {
                    float factor = 1.0f - (dist / maxDist);
                    // Custom non-linear steep falloff for a crisp, short-range stylized aura
                    glow = factor * factor * factor;
                }

                // USE PREMULTIPLIED ALPHA: Multiply RGB channels by the alpha intensity (glow)
                int val = (int)(glow * 255);
                data[y * size + x] = new Color(val, val, val, val);
            }
        }

        texture.SetData(data);
        return texture;
    }
}
