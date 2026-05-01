using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.UI.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.UI;

public static class UiBuilder
{
    public static EntityId CreateButton(World world, int x, int y, int width, int height, string text, string actionId, string fontName = "Fonts/Silkscreen")
    {
        var entityId = world.CreateEntity();
        world.SetComponent(entityId, new UiTransformComponent(x, y, width, height));
        world.SetComponent(entityId, new UiStateComponent { State = UiState.Normal });
        world.SetComponent(entityId, new UiTextComponent(text, fontName) { CenterX = true, CenterY = true });
        world.SetComponent(entityId, new UiButtonComponent(actionId));
        return entityId;
    }

    public static EntityId CreateLabel(World world, int x, int y, string text, string fontName = "Fonts/Silkscreen", float scale = 1.0f, bool hasShadow = false)
    {
        var entityId = world.CreateEntity();
        world.SetComponent(entityId, new UiTransformComponent(x, y, 0, 0)); 
        world.SetComponent(entityId, new UiTextComponent(text, fontName) { Scale = scale, HasShadow = hasShadow });
        return entityId;
    }

    public static EntityId CreatePanel(World world, int x, int y, int width, int height)
    {
        var entityId = world.CreateEntity();
        world.SetComponent(entityId, new UiTransformComponent(x, y, width, height, -1)); 
        world.SetComponent(entityId, new UiStateComponent { State = UiState.Normal });
        world.SetComponent(entityId, new UiPanelComponent());
        return entityId;
    }

    public static EntityId CreateCheckbox(World world, int x, int y, string text, bool isChecked = false, int width = 300)
    {
        var entityId = world.CreateEntity();
        world.SetComponent(entityId, new UiTransformComponent(x, y, width, 30));
        world.SetComponent(entityId, new UiStateComponent { State = UiState.Normal });
        world.SetComponent(entityId, new UiTextComponent(text) { Scale = 0.8f }); 
        world.SetComponent(entityId, new UiCheckboxComponent(isChecked));
        return entityId;
    }

    public static EntityId CreateTextInput(World world, int x, int y, int width, int height, string initialText = "")
    {
        var entityId = world.CreateEntity();
        world.SetComponent(entityId, new UiTransformComponent(x, y, width, height));
        world.SetComponent(entityId, new UiStateComponent { State = UiState.Normal });
        world.SetComponent(entityId, new UiTextComponent(string.Empty) { Scale = 0.8f });
        world.SetComponent(entityId, new UiTextInputComponent { Text = initialText });
        return entityId;
    }

    public static EntityId CreateSlider(World world, int x, int y, int width, int height, float initialValue = 0.5f, bool vertical = false)
    {
        var entityId = world.CreateEntity();
        world.SetComponent(entityId, new UiTransformComponent(x, y, width, height));
        world.SetComponent(entityId, new UiStateComponent { State = UiState.Normal });
        world.SetComponent(entityId, new UiSliderComponent(initialValue, vertical));
        return entityId;
    }

    public static EntityId CreateSelector(World world, int x, int y, int width, int height, string text, string[] options, int initialIndex = 0)
    {
        var entityId = world.CreateEntity();
        world.SetComponent(entityId, new UiTransformComponent(x, y, width, height));
        world.SetComponent(entityId, new UiStateComponent { State = UiState.Normal });
        world.SetComponent(entityId, new UiTextComponent(text) { Scale = 0.8f });
        world.SetComponent(entityId, new UiSelectorComponent(options, initialIndex));
        return entityId;
    }
}
