using Microsoft.Xna.Framework;
using GameEngineLab.Core.Features.Rendering.Resources;
using System.Collections.Generic;

namespace GameEngineLab.GolfIt.Features.Rendering.Resources;

public static class TwilioQuestPalette
{
    public static PaletteResource Create()
    {
        return new PaletteResource
        {
            Name = "TwilioQuest 76",
            Colors = new List<Color>
            {
                PaletteResource.ParseHex("FFffffff"), PaletteResource.ParseHex("FFeaeae8"), PaletteResource.ParseHex("FFcecac9"), PaletteResource.ParseHex("FFabafb9"),
                PaletteResource.ParseHex("FFa18897"), PaletteResource.ParseHex("FF756276"), PaletteResource.ParseHex("FF5d4660"), PaletteResource.ParseHex("FF4c3250"),
                PaletteResource.ParseHex("FF432641"), PaletteResource.ParseHex("FF28192f"), PaletteResource.ParseHex("FFfb7575"), PaletteResource.ParseHex("FFfb3b64"),
                PaletteResource.ParseHex("FFc83157"), PaletteResource.ParseHex("FF8e375c"), PaletteResource.ParseHex("FF4f2351"), PaletteResource.ParseHex("FF351544"),
                PaletteResource.ParseHex("FFf74a53"), PaletteResource.ParseHex("FFf22f46"), PaletteResource.ParseHex("FFbc1642"), PaletteResource.ParseHex("FFfcc539"),
                PaletteResource.ParseHex("FFf87b1b"), PaletteResource.ParseHex("FFf8401b"), PaletteResource.ParseHex("FFbd2709"), PaletteResource.ParseHex("FF7c122b"),
                PaletteResource.ParseHex("FFffe08b"), PaletteResource.ParseHex("FFfac05a"), PaletteResource.ParseHex("FFeb8f48"), PaletteResource.ParseHex("FFd17441"),
                PaletteResource.ParseHex("FFc75239"), PaletteResource.ParseHex("FFb12935"), PaletteResource.ParseHex("FFfdbd8f"), PaletteResource.ParseHex("FFf0886b"),
                PaletteResource.ParseHex("FFd36853"), PaletteResource.ParseHex("FFae454a"), PaletteResource.ParseHex("FF8c3132"), PaletteResource.ParseHex("FF542323"),
                PaletteResource.ParseHex("FFa85848"), PaletteResource.ParseHex("FF83404c"), PaletteResource.ParseHex("FF67314b"), PaletteResource.ParseHex("FF3f2323"),
                PaletteResource.ParseHex("FFd49577"), PaletteResource.ParseHex("FF9f705a"), PaletteResource.ParseHex("FF845750"), PaletteResource.ParseHex("FF633b3f"),
                PaletteResource.ParseHex("FF7bd7a9"), PaletteResource.ParseHex("FF52b281"), PaletteResource.ParseHex("FF148568"), PaletteResource.ParseHex("FF146756"),
                PaletteResource.ParseHex("FF22474c"), PaletteResource.ParseHex("FF102f34"), PaletteResource.ParseHex("FFebff8b"), PaletteResource.ParseHex("FFb3e363"),
                PaletteResource.ParseHex("FF4cbd56"), PaletteResource.ParseHex("FF2f8735"), PaletteResource.ParseHex("FF0b5931"), PaletteResource.ParseHex("FF97bf6e"),
                PaletteResource.ParseHex("FF899f66"), PaletteResource.ParseHex("FF61855a"), PaletteResource.ParseHex("FF4c6051"), PaletteResource.ParseHex("FF73dff2"),
                PaletteResource.ParseHex("FF2abbd0"), PaletteResource.ParseHex("FF315dcd"), PaletteResource.ParseHex("FF472a9c"), PaletteResource.ParseHex("FFa0d8d7"),
                PaletteResource.ParseHex("FF7dbefa"), PaletteResource.ParseHex("FF668faf"), PaletteResource.ParseHex("FF585d81"), PaletteResource.ParseHex("FF45365d"),
                PaletteResource.ParseHex("FFf6bafe"), PaletteResource.ParseHex("FFd59ff4"), PaletteResource.ParseHex("FFb070eb"), PaletteResource.ParseHex("FF7c3ce1"),
                PaletteResource.ParseHex("FFdbcfb1"), PaletteResource.ParseHex("FFa9a48d"), PaletteResource.ParseHex("FF7b8382"), PaletteResource.ParseHex("FF5f5f6e")
            }
        };
    }
}
