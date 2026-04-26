using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;

namespace GameEngineLab.Pacman.Features.Gameplay.Systems;

public sealed class PacmanCollectibleSystem : IGameSystem
{
    public int Order => 15;

    public void Update(World world, FrameContext frameContext)
    {
        var gameplay = world.GetRequiredResource<GameplayStateResource>();
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay || gameplay.IsGameOver || gameplay.IsWin)
        {
            return;
        }

        var collectibles = world.GetRequiredResource<CollectiblesResource>();

        foreach (var entity in world.GetEntitiesWith<PacmanPlayerComponent>())
        {
            if (!world.TryGetComponent<PacmanPlayerComponent>(entity, out var pacman))
            {
                continue;
            }

            var tile = pacman.GridPosition;

            if (collectibles.Food.Remove(tile))
            {
                gameplay.Score += 10;
            }
            else if (collectibles.Pills.Remove(tile))
            {
                gameplay.Score += 50;
                TriggerFrightened(world);
            }

            if (collectibles.TotalCount == 0)
            {
                gameplay.IsWin = true;
                appMode.Mode = AppMode.Win;
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
    }

    private static void TriggerFrightened(World world)
    {
        foreach (var entity in world.GetEntitiesWith<GhostComponent>())
        {
            if (!world.TryGetComponent<GhostComponent>(entity, out var ghost))
            {
                continue;
            }

            if (ghost.State == GhostState.Returning)
            {
                continue;
            }

            ghost.State = GhostState.Frightened;
            ghost.FrightenedTimer = 15f;
            world.SetComponent(entity, ghost);
        }
    }
}
