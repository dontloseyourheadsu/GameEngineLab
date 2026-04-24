using System.Collections.Generic;

namespace GameEngineLab.Pacman.Features.Map.Resources;

public sealed class MapProject
{
    public string Name { get; set; } = "New Map";
    public int Width { get; set; } = 20;
    public int Height { get; set; } = 20;
    public char[][] Tiles { get; set; } = [];
    public bool IsDone { get; set; }
}

public sealed class MapLibraryResource
{
    public List<MapProject> Projects { get; set; } = new();
    public int SelectedProjectIndex { get; set; } = -1;
}

public sealed class MapEditorResource
{
    public MapProject? ActiveProject { get; set; }

    public char[] Palette { get; set; } = [];

    public int SelectedPaletteIndex { get; set; }

    public bool Dirty { get; set; }
}
