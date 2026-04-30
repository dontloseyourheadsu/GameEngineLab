using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.UI.Components;
using GameEngineLab.Core.Features.UI.Resources;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameEngineLab.Core.Features.UI.Systems;

public sealed class UiInteractionSystem : IGameSystem
{
    public int Order => -50; // Run early, but after input abstraction if any

    public void Update(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<UiFocusResource>(out var focusResource) || focusResource == null)
        {
            focusResource = new UiFocusResource();
            world.SetResource(focusResource);
        }

        var mouseState = frameContext.CurrentMouse;
        var prevMouseState = frameContext.PreviousMouse;
        var mousePos = mouseState.Position;
        var isLeftPressed = mouseState.LeftButton == ButtonState.Pressed;
        var wasLeftPressedJustNow = mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released;
        var wasLeftClicked = mouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed;

        foreach (var entityId in world.GetEntitiesWith<UiTransformComponent, UiStateComponent>())
        {
            world.TryGetComponent<UiTransformComponent>(entityId, out var transform);
            world.TryGetComponent<UiStateComponent>(entityId, out var uiState);

            bool isHovered = transform.Bounds.Contains(mousePos);
            bool isFocused = focusResource.FocusedEntity == entityId;
            
            var newState = UiState.Normal;
            if (isHovered)
            {
                newState = isLeftPressed ? UiState.Pressed : UiState.Hovered;
            }
            
            if (isFocused && newState == UiState.Normal)
            {
                newState = UiState.Focused;
            }

            if (uiState.State != newState)
            {
                uiState.State = newState;
                world.SetComponent(entityId, uiState);
            }

            // Handle Clicks & Focus
            if (isHovered && wasLeftPressedJustNow)
            {
                // Update Global Focus on press for better responsiveness
                focusResource.FocusedEntity = entityId;
            }

            if (isHovered && wasLeftClicked)
            {
                // Toggle Checkbox
                if (world.TryGetComponent<UiCheckboxComponent>(entityId, out var checkbox))
                {
                    checkbox.Checked = !checkbox.Checked;
                    world.SetComponent(entityId, checkbox);
                }
            }
            else if (!isHovered && wasLeftClicked && isFocused)
            {
                // Clicked outside, but this was focused - clear if needed
                // (Optional: depends on if we want to clear focus on every outside click)
            }

            // Handle Sliders / Scroll bars
            if (world.TryGetComponent<UiSliderComponent>(entityId, out var slider))
            {
                if (isHovered && isLeftPressed)
                {
                    if (slider.Vertical)
                    {
                        slider.Value = Math.Clamp((float)(mousePos.Y - transform.Bounds.Top) / transform.Bounds.Height, 0, 1);
                    }
                    else
                    {
                        slider.Value = Math.Clamp((float)(mousePos.X - transform.Bounds.Left) / transform.Bounds.Width, 0, 1);
                    }
                    world.SetComponent(entityId, slider);
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
