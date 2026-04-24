using GameEngineLab.Core.Features.Maps.Resources;

namespace GameEngineLab.Pacman.Features.Map.Resources;

public sealed class MapStateResource
{
    public required Map2DModel Map { get; init; }
}
