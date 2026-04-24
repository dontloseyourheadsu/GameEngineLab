using System;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Pacman.Features.Gameplay.Components;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Pacman.Systems;

public sealed class PacmanMovementSystem : IGameSystem
{
    public int Order => 10;

    public void Update(World world, FrameContext frameContext)
    {
        var gameplay = world.GetRequiredResource<GameplayStateResource>();
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay || gameplay.IsGameOver || gameplay.IsWin)
        {
            return;
        }

        MapBoundsResource bounds = world.GetRequiredResource<MapBoundsResource>();
        MapStateResource mapState = world.GetRequiredResource<MapStateResource>();
        var map = mapState.Map;

        foreach (var entity in world.GetEntitiesWith<TransformComponent, VelocityComponent>())
        {
            if (!world.TryGetComponent<PacmanPlayerComponent>(entity, out var pacman)
                || !world.TryGetComponent<TransformComponent>(entity, out var transform)
                || !world.TryGetComponent<VelocityComponent>(entity, out var velocity))
            {
                continue;
            }

            if (!pacman.IsMoving)
            {
                if (TryGetNextPosition(pacman.GridPosition, pacman.DesiredDirection, map, out _))
                {
                    pacman.CurrentDirection = pacman.DesiredDirection;
                }

                if (TryGetNextPosition(pacman.GridPosition, pacman.CurrentDirection, map, out var nextPosition))
                {
                    var distX = Math.Abs(nextPosition.X - pacman.GridPosition.X);
                    var distY = Math.Abs(nextPosition.Y - pacman.GridPosition.Y);
                    pacman.IsTeleporting = distX > 1 || distY > 1;
                    pacman.PreviousGridPosition = pacman.GridPosition;
                    pacman.GridPosition = nextPosition;
                    pacman.IsMoving = true;
                    pacman.MoveProgress = 0f;
                }
            }

            if (pacman.IsMoving)
            {
                var interval = Math.Max(0.01f, pacman.MoveIntervalSeconds);
                pacman.MoveProgress += frameContext.DeltaSeconds / interval;

                if (pacman.MoveProgress >= 1f)
                {
                    pacman.MoveProgress = 0f;
                    pacman.IsMoving = false;
                    pacman.IsTeleporting = false;
                    transform.Position = GetTileCenter(pacman.GridPosition, map.TileSize);
                }
                else
                {
                    var start = GetTileCenter(pacman.PreviousGridPosition, map.TileSize);
                    var end = GetTileCenter(pacman.GridPosition, map.TileSize);

                    if (!pacman.IsTeleporting)
                    {
                        var stepped = MathF.Floor(pacman.MoveProgress * 4f) / 4f;
                        transform.Position = Vector2.Lerp(start, end, stepped);
                    }
                    else
                    {
                        var stepped = MathF.Floor(pacman.MoveProgress * 4f) / 4f;
                        transform.Position = stepped >= 0.75f ? end : start;
                    }
                }
            }
            else
            {
                transform.Position = GetTileCenter(pacman.GridPosition, map.TileSize);
            }

            float minX = bounds.PlayArea.Left + pacman.Radius;
            float maxX = bounds.PlayArea.Right - pacman.Radius;
            float minY = bounds.PlayArea.Top + pacman.Radius;
            float maxY = bounds.PlayArea.Bottom - pacman.Radius;

            transform.Position = new Vector2(
                MathHelper.Clamp(transform.Position.X, minX, maxX),
                MathHelper.Clamp(transform.Position.Y, minY, maxY));

            velocity.Value = new Vector2(pacman.CurrentDirection.X, pacman.CurrentDirection.Y);
            world.SetComponent(entity, velocity);
            world.SetComponent(entity, pacman);
            world.SetComponent(entity, transform);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
    }

    private static bool TryGetNextPosition(Point current, Point direction, Map2DModel map, out Point next)
    {
        next = current;
        if (direction == Point.Zero)
        {
            return false;
        }

        var nx = current.X + direction.X;
        var ny = current.Y + direction.Y;

        if (nx < 0)
        {
            nx = map.Width - 1;
        }
        else if (nx >= map.Width)
        {
            nx = 0;
        }

        if (ny < 0)
        {
            ny = map.Height - 1;
        }
        else if (ny >= map.Height)
        {
            ny = 0;
        }

        if (map.GetTile(nx, ny) == '#')
        {
            return false;
        }

        next = new Point(nx, ny);
        return true;
    }

    private static Vector2 GetTileCenter(Point tile, int tileSize)
    {
        return new Vector2((tile.X + 0.5f) * tileSize, (tile.Y + 0.5f) * tileSize);
    }
}
