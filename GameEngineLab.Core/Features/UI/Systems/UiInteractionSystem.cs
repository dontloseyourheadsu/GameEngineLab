using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.UI.Components;
using GameEngineLab.Core.Features.UI.Resources;
using Microsoft.Xna.Framework;
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

        if (!world.TryGetResource<UiThemeResource>(out var theme) || theme == null) return;
        var globalScale = theme.GlobalScale;

        // Check for an open selector first to handle its "modal" behavior
        EntityId? openSelectorEntity = null;
        UiSelectorComponent openSelector = default;
        UiTransformComponent openSelectorTransform = default;

        foreach (var entityId in world.GetEntitiesWith<UiSelectorComponent>())
        {
            if (world.TryGetComponent<UiSelectorComponent>(entityId, out var s) && s.IsOpen)
            {
                openSelectorEntity = entityId;
                openSelector = s;
                world.TryGetComponent<UiTransformComponent>(entityId, out openSelectorTransform);
                break;
            }
        }

        if (openSelectorEntity is { } openId && wasLeftClicked)
        {
            var bounds = new Rectangle(
                (int)(openSelectorTransform.Bounds.X * globalScale),
                (int)(openSelectorTransform.Bounds.Y * globalScale),
                (int)(openSelectorTransform.Bounds.Width * globalScale),
                (int)(openSelectorTransform.Bounds.Height * globalScale)
            );
            var optionsCount = openSelector.Options?.Length ?? 0;
            var overlayHeight = bounds.Height * optionsCount;
            var overlayBounds = new Rectangle(bounds.X, bounds.Bottom, bounds.Width, overlayHeight);

            if (overlayBounds.Contains(mousePos) && optionsCount > 0)
            {
                int index = (mousePos.Y - overlayBounds.Top) / bounds.Height;
                if (index >= 0 && index < optionsCount)
                {
                    openSelector.SelectedIndex = index;
                }
                openSelector.IsOpen = false;
                world.SetComponent(openId, openSelector);
                return; 
            }
            
            // If we click anywhere else (including the selector itself), close it
            openSelector.IsOpen = false;
            world.SetComponent(openId, openSelector);
            if (bounds.Contains(mousePos)) return; 
        }

        // Check for modal
        EntityId? modalEntity = world.GetEntitiesWith<UiModalComponent>().Cast<EntityId?>().FirstOrDefault();

        foreach (var entityId in world.GetEntitiesWith<UiTransformComponent, UiStateComponent>())
        {
            // If a selector is open, other UI elements don't respond to interaction
            if (openSelectorEntity.HasValue && openSelectorEntity.Value != entityId) continue;

            // If a modal is present, only the modal (and its components) are interactable
            if (modalEntity.HasValue && !world.HasComponent<UiModalComponent>(entityId))
            {
                // Reset state to normal if it was something else
                if (world.TryGetComponent<UiStateComponent>(entityId, out var s) && s.State != UiState.Normal)
                {
                    s.State = UiState.Normal;
                    world.SetComponent(entityId, s);
                }
                continue;
            }

            world.TryGetComponent<UiTransformComponent>(entityId, out var transform);
            world.TryGetComponent<UiStateComponent>(entityId, out var uiState);

            var bounds = new Rectangle(
                (int)(transform.Bounds.X * globalScale),
                (int)(transform.Bounds.Y * globalScale),
                (int)(transform.Bounds.Width * globalScale),
                (int)(transform.Bounds.Height * globalScale)
            );

            bool isHovered = bounds.Contains(mousePos);
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
                // Only allow interactive components to take focus
                bool isInteractive = world.HasComponent<UiButtonComponent>(entityId) || 
                                     world.HasComponent<UiCheckboxComponent>(entityId) || 
                                     world.HasComponent<UiSliderComponent>(entityId) || 
                                     world.HasComponent<UiTextInputComponent>(entityId) ||
                                     world.HasComponent<UiSelectorComponent>(entityId);

                if (isInteractive)
                {
                    focusResource.FocusedEntity = entityId;
                }
            }

            if (isHovered && wasLeftClicked)
            {
                // Toggle Checkbox
                if (world.TryGetComponent<UiCheckboxComponent>(entityId, out var checkbox))
                {
                    checkbox.Checked = !checkbox.Checked;
                    world.SetComponent(entityId, checkbox);
                }

                // Toggle Selector Open State
                if (world.TryGetComponent<UiSelectorComponent>(entityId, out var selector))
                {
                    selector.IsOpen = !selector.IsOpen;
                    world.SetComponent(entityId, selector);
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
                        slider.Value = Math.Clamp((float)(mousePos.Y - bounds.Top) / bounds.Height, 0, 1);
                    }
                    else
                    {
                        slider.Value = Math.Clamp((float)(mousePos.X - bounds.Left) / bounds.Width, 0, 1);
                    }
                    world.SetComponent(entityId, slider);
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
