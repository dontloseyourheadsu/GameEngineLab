using System.IO;
using System;

namespace GameEngineLab.Pacman.Features.Map.Resources;

public static class MapPaths
{
    public static string DefaultMap => Path.Combine(AppContext.BaseDirectory, "Data", "map.json");
    public static string MapLibrary => Path.Combine(AppContext.BaseDirectory, "Data", "maps_library.json");
}
