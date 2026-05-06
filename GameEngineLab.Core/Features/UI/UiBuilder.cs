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
        world.SetComponent(entityId, new UiTextComponent(text) { Scale = 1.0f }); 
        world.SetComponent(entityId, new UiCheckboxComponent(isChecked));
        return entityId;
    }

    public static EntityId CreateTextInput(World world, int x, int y, int width, int height, string initialText = "")
    {
        var entityId = world.CreateEntity();
        world.SetComponent(entityId, new UiTransformComponent(x, y, width, height));
        world.SetComponent(entityId, new UiStateComponent { State = UiState.Normal });
        world.SetComponent(entityId, new UiTextComponent(string.Empty) { Scale = 1.0f });
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
        world.SetComponent(entityId, new UiTextComponent(text) { Scale = 1.0f });
        world.SetComponent(entityId, new UiSelectorComponent(options, initialIndex));
        return entityId;
    }

    public static void CreatePopup(World world, int centerX, int centerY, string title, string message, string confirmAction, string cancelAction)
    {
        var width = 500;
        var height = 300;
        var x = centerX - width / 2;
        var y = centerY - height / 2;

        var panel = CreatePanel(world, x, y, width, height);
        world.SetComponent(panel, new UiModalComponent());

        var titleLabel = CreateLabel(world, x + 20, y + 20, title, "Fonts/SilkscreenBold", 1.5f);
        world.SetComponent(titleLabel, new UiModalComponent());

        var messageLabel = CreateLabel(world, x + 20, y + 80, message, "Fonts/Silkscreen", 1.0f);
        world.SetComponent(messageLabel, new UiModalComponent());

        var confirmBtn = CreateButton(world, x + 40, y + 200, 180, 60, "CONFIRM", confirmAction, "Fonts/SilkscreenBold");
        world.SetComponent(confirmBtn, new UiModalComponent());

        var cancelBtn = CreateButton(world, x + 280, y + 200, 180, 60, "CANCEL", cancelAction, "Fonts/SilkscreenBold");
        world.SetComponent(cancelBtn, new UiModalComponent());
    }
}
