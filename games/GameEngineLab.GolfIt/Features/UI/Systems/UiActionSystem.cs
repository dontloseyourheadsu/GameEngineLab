using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.UI.Components;
using GameEngineLab.GolfIt.Features.Runtime;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.GolfIt.Features.UI.Systems;

public sealed class UiActionSystem : IGameSystem
{
    public int Order => 0;

    public void Update(World world, FrameContext frameContext)
    {
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
    }

    public void Draw(World world, FrameContext frameContext) { }

    private void HandleAction(World world, string actionId)
    {
        if (actionId == "play" && world.TryGetResource<GameStateResource>(out var state))
        {
            state!.Current = GameState.Playing;
        }
    }
}
