using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Pacman.Components;

public struct PacmanPlayerComponent : IComponent
{
    public float Speed;
    public float Radius;
    public Point GridPosition;
    public Point PreviousGridPosition;
    public Point CurrentDirection;
    public Point DesiredDirection;
    public bool IsMoving;
    public bool IsTeleporting;
    public float MoveProgress;
    public float MoveIntervalSeconds;
}
