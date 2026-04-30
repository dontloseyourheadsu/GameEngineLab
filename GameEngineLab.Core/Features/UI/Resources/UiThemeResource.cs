using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GameEngineLab.Core.Features.UI.Resources;

public sealed class UiThemeResource
{
    public Color BorderColor { get; set; } = new Color(0x23, 0x4a, 0x57); 
    public Color SurfaceColor { get; set; } = new Color(0x5c, 0x99, 0x74); 
    public Color TextColor { get; set; } = new Color(0xe3, 0xde, 0x88); 
    public Color HighlightColor { get; set; } = new Color(0x76, 0xbd, 0x5e);
    public Color ShadowColor { get; set; } = new Color(0x0d, 0x1f, 0x07);
    public Color AccentColor { get; set; } = new Color(0xa8, 0xd8, 0x70);
    public Color WarningColor { get; set; } = new Color(0xe8, 0x7a, 0x50);

    public int ShadowOffset { get; set; } = 4;
    public int PressedContentOffset { get; set; } = 4;

    public Dictionary<string, SpriteFont> Fonts { get; } = new();
}
