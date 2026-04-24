using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Pacman.Resources;

public sealed class PacmanSpawnResource
{
    public required Vector2 SpawnPosition { get; init; }

    public required Point SpawnTile { get; init; }
}
