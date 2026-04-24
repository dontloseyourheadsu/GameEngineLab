using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Map.Resources;

public sealed class CollectiblesResource
{
    public required HashSet<Point> Food { get; init; }

    public required HashSet<Point> Pills { get; init; }

    public int TotalCount => Food.Count + Pills.Count;
}
