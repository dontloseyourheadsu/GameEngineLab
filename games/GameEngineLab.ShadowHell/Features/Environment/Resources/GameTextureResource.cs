using Microsoft.Xna.Framework.Graphics;

namespace GameEngineLab.ShadowHell.Features.Environment.Resources;

public sealed class GameTextureResource
{
    public required Texture2D FloorTexture { get; set; }
    public required Texture2D WallTexture { get; set; }
    public required Texture2D LightTexture { get; set; }
}
