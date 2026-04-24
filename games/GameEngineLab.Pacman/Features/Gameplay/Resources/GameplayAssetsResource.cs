using Microsoft.Xna.Framework.Graphics;

namespace GameEngineLab.Pacman.Features.Gameplay.Resources;

public sealed class GameplayAssetsResource
{
    public Texture2D[] PacmanFrames { get; set; } = [];
    public Texture2D Ghost { get; set; } = null!;
    public Texture2D Wall { get; set; } = null!;
    public Texture2D Food { get; set; } = null!;
    public Texture2D Pill { get; set; } = null!;

    public bool IsInitialized => PacmanFrames.Length > 0 && Ghost != null;

    public void Dispose()
    {
        foreach (var f in PacmanFrames) f?.Dispose();
        Ghost?.Dispose();
        Wall?.Dispose();
        Food?.Dispose();
        Pill?.Dispose();
    }
}
