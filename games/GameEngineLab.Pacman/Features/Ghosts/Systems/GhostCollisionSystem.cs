using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Pacman.Features.Gameplay.Components;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.Pacman.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Ghosts.Systems;

public sealed class GhostCollisionSystem : IGameSystem
{
    public int Order => 40;

    public void Update(World world, FrameContext frameContext)
    {
        var gameplay = world.GetRequiredResource<GameplayStateResource>();
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay || gameplay.IsGameOver || gameplay.IsWin)
        {
            return;
        }

        if (!TryGetPacman(world, out var pacmanEntity, out var pacmanTransform, out var pacman))
        {
            return;
        }

        foreach (var ghostEntity in world.GetEntitiesWith<GhostComponent, TransformComponent>())
        {
            if (!world.TryGetComponent<GhostComponent>(ghostEntity, out var ghost)
                || !world.TryGetComponent<TransformComponent>(ghostEntity, out var ghostTransform))
            {
                continue;
            }

            var radius = pacman.Radius + ghost.Radius;
            if (Vector2.DistanceSquared(pacmanTransform.Position, ghostTransform.Position) > radius * radius)
            {
                continue;
            }

            if (ghost.State == GhostState.Frightened)
            {
                ghost.State = GhostState.Returning;
                ghost.FrightenedTimer = 0f;
                gameplay.Score += 200;
                world.SetComponent(ghostEntity, ghost);
                continue;
            }

            if (ghost.State == GhostState.Returning)
            {
                continue;
            }

            gameplay.Lives -= 1;
            if (gameplay.Lives <= 0)
            {
                gameplay.IsGameOver = true;
                break;
            }

            ResetRound(world, pacmanEntity);
            break;
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
    }

    private static bool TryGetPacman(
        World world,
        out EntityId pacmanEntity,
        out TransformComponent transform,
        out PacmanPlayerComponent pacman)
    {
        pacmanEntity = default;
        transform = default;
        pacman = default;

        foreach (var entity in world.GetEntitiesWith<PacmanPlayerComponent, TransformComponent>())
        {
            if (!world.TryGetComponent<PacmanPlayerComponent>(entity, out pacman)
                || !world.TryGetComponent<TransformComponent>(entity, out transform))
            {
                continue;
            }

            pacmanEntity = entity;
            return true;
        }

        return false;
    }

    private static void ResetRound(World world, EntityId pacmanEntity)
    {
        var pacmanSpawn = world.GetRequiredResource<PacmanSpawnResource>();
        if (world.TryGetComponent<TransformComponent>(pacmanEntity, out var pacmanTransform))
        {
            pacmanTransform.Position = pacmanSpawn.SpawnPosition;
            world.SetComponent(pacmanEntity, pacmanTransform);
        }

        if (world.TryGetComponent<PacmanPlayerComponent>(pacmanEntity, out var pacman))
        {
            pacman.GridPosition = pacmanSpawn.SpawnTile;
            pacman.PreviousGridPosition = pacmanSpawn.SpawnTile;
            pacman.CurrentDirection = Point.Zero;
            pacman.DesiredDirection = Point.Zero;
            pacman.IsMoving = false;
            pacman.IsTeleporting = false;
            pacman.MoveProgress = 0f;
            world.SetComponent(pacmanEntity, pacman);
        }

        if (world.TryGetComponent<VelocityComponent>(pacmanEntity, out var pacmanVelocity))
        {
            pacmanVelocity.Value = Vector2.Zero;
            world.SetComponent(pacmanEntity, pacmanVelocity);
        }

        foreach (var ghostEntity in world.GetEntitiesWith<GhostComponent, TransformComponent>())
        {
            if (!world.TryGetComponent<GhostComponent>(ghostEntity, out var ghost)
                || !world.TryGetComponent<TransformComponent>(ghostEntity, out var ghostTransform))
            {
                continue;
            }

            ghost.State = GhostState.Scatter;
            ghost.FrightenedTimer = 0f;
            ghost.GridPosition = ghost.SpawnTile;
            ghost.PreviousGridPosition = ghost.GridPosition;
            ghost.NextGridPosition = ghost.GridPosition;
            ghost.CurrentDirection = Point.Zero;
            ghost.IsMoving = false;
            ghost.MoveProgress = 0f;
            ghostTransform.Position = ghost.SpawnPosition;

            world.SetComponent(ghostEntity, ghost);
            world.SetComponent(ghostEntity, ghostTransform);
        }
    }
}
