using System;
using System.Collections.Generic;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Ghosts.Systems;

public sealed class GhostMovementSystem : IGameSystem
{
    public int Order => 30;

    public void Update(World world, FrameContext frameContext)
    {
        var gameplay = world.GetRequiredResource<GameplayStateResource>();
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay || gameplay.IsGameOver || gameplay.IsWin)
        {
            return;
        }

        var map = world.GetRequiredResource<MapStateResource>().Map;

        if (!TryGetPacmanData(world, out var pacmanPosition, out var pacmanDirection))
        {
            return;
        }

        foreach (var entity in world.GetEntitiesWith<GhostComponent, TransformComponent>())
        {
            if (!world.TryGetComponent<GhostComponent>(entity, out var ghost)
                || !world.TryGetComponent<TransformComponent>(entity, out var transform))
            {
                continue;
            }

            if (ghost.State == GhostState.Returning && ghost.GridPosition == ghost.SpawnTile && !ghost.IsMoving)
            {
                ghost.State = GhostState.Chase;
                ghost.CurrentDirection = Point.Zero;
            }

            if (!ghost.IsMoving)
            {
                Point moveDir;
                if (ghost.State == GhostState.Returning && TryGetNextPathStep(ghost.GridPosition, ghost.SpawnTile, map, out var nextStep))
                {
                    moveDir = new Point(nextStep.X - ghost.GridPosition.X, nextStep.Y - ghost.GridPosition.Y);
                }
                else
                {
                    var target = GetTarget(ghost, transform.Position, pacmanPosition, pacmanDirection, map);
                    moveDir = ChooseDirection(ghost, target, map);
                }

                if (TryGetNextPosition(ghost.GridPosition, moveDir, map, out var nextPosition))
                {
                    ghost.CurrentDirection = moveDir;
                    ghost.PreviousGridPosition = ghost.GridPosition;
                    ghost.NextGridPosition = nextPosition;
                    ghost.IsMoving = true;
                    ghost.MoveProgress = 0f;
                }
            }

            if (ghost.IsMoving)
            {
                var interval = ghost.State == GhostState.Returning
                    ? Math.Max(0.01f, ghost.MoveIntervalSeconds / 2f)
                    : Math.Max(0.01f, ghost.MoveIntervalSeconds);

                ghost.MoveProgress += frameContext.DeltaSeconds / interval;

                if (ghost.MoveProgress >= 0.5f && ghost.GridPosition != ghost.NextGridPosition)
                {
                    ghost.GridPosition = ghost.NextGridPosition;
                }

                if (ghost.MoveProgress >= 1f)
                {
                    ghost.MoveProgress = 0f;
                    ghost.IsMoving = false;
                    ghost.GridPosition = ghost.NextGridPosition;
                    transform.Position = GetTileCenter(ghost.GridPosition, map.TileSize);
                }
                else
                {
                    var start = GetTileCenter(ghost.PreviousGridPosition, map.TileSize);
                    var end = GetTileCenter(ghost.NextGridPosition, map.TileSize);
                    transform.Position = Vector2.Lerp(start, end, ghost.MoveProgress);
                }
            }
            else
            {
                transform.Position = GetTileCenter(ghost.GridPosition, map.TileSize);
            }

            world.SetComponent(entity, ghost);
            world.SetComponent(entity, transform);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
    }

    private static bool TryGetPacmanData(World world, out Vector2 position, out Vector2 direction)
    {
        position = Vector2.Zero;
        direction = Vector2.Zero;

        foreach (var entity in world.GetEntitiesWith<PacmanPlayerComponent, TransformComponent>())
        {
            if (!world.TryGetComponent<TransformComponent>(entity, out var transform)
                || !world.TryGetComponent<VelocityComponent>(entity, out var velocity))
            {
                continue;
            }

            position = transform.Position;
            direction = velocity.Value;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            return true;
        }

        return false;
    }

    private static Point ChooseDirection(GhostComponent ghost, Vector2 target, Map2DModel map)
    {
        var directions = new List<Point>
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1),
        };

        var reverse = new Point(-ghost.CurrentDirection.X, -ghost.CurrentDirection.Y);

        var valid = new List<Point>();
        foreach (var dir in directions)
        {
            if (TryGetNextPosition(ghost.GridPosition, dir, map, out _))
            {
                valid.Add(dir);
            }
        }

        if (valid.Count > 1 && ghost.CurrentDirection != Point.Zero)
        {
            valid.RemoveAll(d => d == reverse);
        }

        if (valid.Count == 0)
        {
            return Point.Zero;
        }

        var best = valid[0];
        var minDist = float.MaxValue;

        foreach (var option in valid)
        {
            if (!TryGetNextPosition(ghost.GridPosition, option, map, out var nextTile))
            {
                continue;
            }

            var next = GetTileCenter(nextTile, map.TileSize);
            var dist = Vector2.DistanceSquared(next, target);
            if (dist < minDist)
            {
                minDist = dist;
                best = option;
            }
        }

        return best;
    }

    private static Vector2 GetTarget(
        GhostComponent ghost,
        Vector2 ghostPosition,
        Vector2 pacmanPosition,
        Vector2 pacmanDirection,
        Map2DModel map)
    {
        var tile = map.TileSize;

        if (ghost.State == GhostState.Returning)
        {
            return ghost.SpawnPosition;
        }

        if (ghost.State == GhostState.Frightened)
        {
            var flee = ghostPosition - pacmanPosition;
            if (flee == Vector2.Zero)
            {
                return ghostPosition + new Vector2(tile * 4f, tile * 4f);
            }

            flee.Normalize();
            return ghostPosition + flee * (tile * 8f);
        }

        if (ghost.State == GhostState.Scatter)
        {
            return ghost.Behavior switch
            {
                GhostBehavior.Blinky => new Vector2(map.Width * tile, 0f),
                GhostBehavior.Pinky => Vector2.Zero,
                GhostBehavior.Inky => new Vector2(map.Width * tile, map.Height * tile),
                GhostBehavior.Clyde => new Vector2(0f, map.Height * tile),
                _ => pacmanPosition,
            };
        }

        return ghost.Behavior switch
        {
            GhostBehavior.Blinky => pacmanPosition,
            GhostBehavior.Pinky => pacmanPosition + (pacmanDirection * (tile * 4f)),
            GhostBehavior.Inky => pacmanPosition - (pacmanDirection * (tile * 2f)),
            GhostBehavior.Clyde => Vector2.DistanceSquared(ghostPosition, pacmanPosition) > (tile * 8f) * (tile * 8f)
                ? pacmanPosition
                : new Vector2(0f, map.Height * tile),
            _ => pacmanPosition,
        };
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
        if (nx < 0 || ny < 0 || nx >= map.Width || ny >= map.Height)
        {
            return false;
        }

        if (map.GetTile(nx, ny) == '#')
        {
            return false;
        }

        next = new Point(nx, ny);
        return true;
    }

    private static bool TryGetNextPathStep(Point start, Point target, Map2DModel map, out Point nextStep)
    {
        nextStep = default;
        if (start == target)
        {
            return false;
        }

        var queue = new Queue<Point>();
        var visited = new HashSet<Point> { start };
        var cameFrom = new Dictionary<Point, Point>();
        queue.Enqueue(start);

        var dirs = new[] { new Point(1, 0), new Point(-1, 0), new Point(0, 1), new Point(0, -1) };

        var found = false;
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == target)
            {
                found = true;
                break;
            }

            foreach (var dir in dirs)
            {
                if (!TryGetNextPosition(current, dir, map, out var neighbor))
                {
                    continue;
                }

                if (visited.Contains(neighbor))
                {
                    continue;
                }

                visited.Add(neighbor);
                cameFrom[neighbor] = current;
                queue.Enqueue(neighbor);
            }
        }

        if (!found)
        {
            return false;
        }

        var step = target;
        while (cameFrom.TryGetValue(step, out var parent) && parent != start)
        {
            step = parent;
        }

        if (!cameFrom.ContainsKey(step) && step != target)
        {
            return false;
        }

        nextStep = step;
        return true;
    }

    private static Vector2 GetTileCenter(Point tile, int tileSize)
    {
        return new Vector2((tile.X + 0.5f) * tileSize, (tile.Y + 0.5f) * tileSize);
    }
}
