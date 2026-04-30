using Microsoft.Xna.Framework;
using GameEngineLab.Core.Features.Rendering.Resources;
using System.Collections.Generic;

namespace GameEngineLab.GolfIt.Features.Rendering.Resources;

public static class CozyPalette
{
    public static PaletteResource Create()
    {
        return new PaletteResource
        {
            Name = "Cozy Forest",
            Colors = new List<Color>
            {
                PaletteResource.ParseHex("#2d4a1b"), // 0: Root BG
                PaletteResource.ParseHex("#1e3a10"), // 1: Card BG
                PaletteResource.ParseHex("#5c9934"), // 2: Card Border
                PaletteResource.ParseHex("#0d1f07"), // 3: Shadow
                PaletteResource.ParseHex("#e3de88"), // 4: Text Primary
                PaletteResource.ParseHex("#76bd5e"), // 5: Label / Sub
                PaletteResource.ParseHex("#3a6b22"), // 6: Button Border
                PaletteResource.ParseHex("#b35f47"), // 7: Warn Border
                PaletteResource.ParseHex("#e87a50"), // 8: Warn Text
                PaletteResource.ParseHex("#1a2e0e"), // 9: Header Shadow
                PaletteResource.ParseHex("#a8d870"), // 10: H1 Text
                PaletteResource.ParseHex("#c8e8a0"), // 11: Body Text
            }
        };
    }
}
