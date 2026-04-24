using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Ghosts.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;

namespace GameEngineLab.Pacman.Features.Ghosts.Systems;

public sealed class GhostModeSystem : IGameSystem
{
    public int Order => 20;

    public void Update(World world, FrameContext frameContext)
    {
        var gameplay = world.GetRequiredResource<GameplayStateResource>();
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay || gameplay.IsGameOver || gameplay.IsWin)
        {
            return;
        }

        var mode = world.GetRequiredResource<GhostModeResource>();
        mode.Timer += frameContext.DeltaSeconds;

        var target = mode.IsScatterMode ? mode.ScatterDuration : mode.ChaseDuration;
        if (mode.Timer >= target)
        {
            mode.Timer = 0f;
            mode.IsScatterMode = !mode.IsScatterMode;
        }

        foreach (var entity in world.GetEntitiesWith<GhostComponent>())
        {
            if (!world.TryGetComponent<GhostComponent>(entity, out var ghost))
            {
                continue;
            }

            if (ghost.State == GhostState.Frightened)
            {
                ghost.FrightenedTimer -= frameContext.DeltaSeconds;
                if (ghost.FrightenedTimer <= 0f)
                {
                    ghost.State = GhostState.Chase;
                }
            }
            else if (ghost.State != GhostState.Returning)
            {
                ghost.State = mode.IsScatterMode ? GhostState.Scatter : GhostState.Chase;
            }

            world.SetComponent(entity, ghost);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
    }
}
