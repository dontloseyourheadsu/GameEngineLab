namespace GameEngineLab.Pacman.Features.Map.Resources;

public sealed class MapEditorResource
{
    public required char[][] Tiles { get; init; }

    public required char[] Palette { get; init; }

    public int SelectedPaletteIndex { get; set; }

    public bool Dirty { get; set; }
}
