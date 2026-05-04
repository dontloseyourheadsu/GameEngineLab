using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Resources;
using GameEngineLab.Core.Features.UI.Components;
using GameEngineLab.GolfIt.Features.Runtime;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.GolfIt.Features.UI.Systems;

public sealed class UiActionSystem : IGameSystem
{
    public int Order => 0;

    public void Update(World world, FrameContext frameContext)
    {
        // Handle Button Clicks
        if (frameContext.CurrentMouse.LeftButton == ButtonState.Released && 
            frameContext.PreviousMouse.LeftButton == ButtonState.Pressed)
        {
            foreach (var entityId in world.GetEntitiesWith<UiButtonComponent, UiStateComponent>())
            {
                world.TryGetComponent<UiStateComponent>(entityId, out var uiState);
                if (uiState.State == UiState.Hovered || uiState.State == UiState.Pressed)
                {
                    world.TryGetComponent<UiButtonComponent>(entityId, out var button);
                    HandleAction(world, button.ActionId);
                }
            }
        }

        // Handle Queued Actions (from Zones, etc.)
        if (world.TryGetResource<ActionQueueResource>(out var actionQueue) && actionQueue != null)
        {
            while (actionQueue.TryDequeue(out var actionId))
            {
                if (actionId != null)
                {
                    HandleAction(world, actionId);
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }

    private void HandleAction(World world, string actionId)
    {
        if (!world.TryGetResource<GameStateResource>(out var state) || state == null) return;

        if (actionId == "play")
        {
            state.Current = GameState.Playing;
        }
        else if (actionId == "settings")
        {
            state.Current = GameState.Settings;
        }
        else if (actionId == "back_to_menu" || actionId == "goal")
        {
            state.Current = GameState.Menu;
        }
    }
}
