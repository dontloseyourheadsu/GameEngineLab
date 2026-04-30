using GameEngineLab.Core.Features.Rendering.Resources;

namespace GameEngineLab.GolfIt.Features.Rendering.Resources;

public sealed class PaletteLibraryResource
{
    public required PaletteResource General { get; init; }
    public required PaletteResource Specific { get; init; }
}
