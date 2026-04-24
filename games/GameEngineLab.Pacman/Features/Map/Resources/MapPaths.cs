using System.IO;

namespace GameEngineLab.Pacman.Features.Map.Resources;

public static class MapPaths
{
    public static string DefaultMap => Path.Combine(AppContext.BaseDirectory, "Data", "map.json");
}
