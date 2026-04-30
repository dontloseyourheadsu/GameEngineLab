using Microsoft.Xna.Framework;
using GameEngineLab.Core.Features.Rendering.Resources;
using System.Collections.Generic;

namespace GameEngineLab.GolfIt.Features.Rendering.Resources;

public static class SpecificAssetsPalette
{
    public static PaletteResource Create()
    {
        return new PaletteResource
        {
            Name = "Specific Assets",
            Colors = new List<Color>
            {
                PaletteResource.ParseHex("#4d0f32"),
                PaletteResource.ParseHex("#e3de88"),
                PaletteResource.ParseHex("#76bd5e"),
                PaletteResource.ParseHex("#234a57"),
                PaletteResource.ParseHex("#280a33"),
                PaletteResource.ParseHex("#b35f47"),
                PaletteResource.ParseHex("#5c9974"),
                PaletteResource.ParseHex("#080826")
            }
        };
    }
}
