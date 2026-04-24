using System.IO;

namespace GameEngineLab.Pacman.Features.Assets.Resources;

public static class AssetPaths
{
    public static string DefaultAssets => Path.Combine(AppContext.BaseDirectory, "Data", "assets.json");
}
