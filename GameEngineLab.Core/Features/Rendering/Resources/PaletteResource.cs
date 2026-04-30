using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Globalization;

namespace GameEngineLab.Core.Features.Rendering.Resources;

public sealed class PaletteResource
{
    public string Name { get; init; } = "Default";
    public List<Color> Colors { get; init; } = new();

    public Color GetColor(int index)
    {
        if (index < 0 || index >= Colors.Count)
        {
            return Color.Magenta; // Error color
        }
        return Colors[index];
    }

    public static Color ParseHex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            hex = "FF" + hex;
        }
        
        if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint argb))
        {
            byte a = (byte)((argb & 0xFF000000) >> 24);
            byte r = (byte)((argb & 0x00FF0000) >> 16);
            byte g = (byte)((argb & 0x0000FF00) >> 8);
            byte b = (byte)(argb & 0x000000FF);
            return new Color(r, g, b, a);
        }

        return Color.Magenta;
    }
}
