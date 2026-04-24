using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Ghosts.Components;

public enum GhostState
{
    Chase,
    Scatter,
    Frightened,
    Returning,
}

public enum GhostBehavior
{
    Blinky,
    Pinky,
    Inky,
    Clyde,
}

public struct GhostComponent : IComponent
{
    public GhostBehavior Behavior;
    public GhostState State;
    public float FrightenedTimer;
    public float Speed;
    public float Radius;
    public Point GridPosition;
    public Point PreviousGridPosition;
    public Point NextGridPosition;
    public Point CurrentDirection;
    public bool IsMoving;
    public float MoveProgress;
    public float MoveIntervalSeconds;
    public Point SpawnTile;
    public Vector2 SpawnPosition;
}
