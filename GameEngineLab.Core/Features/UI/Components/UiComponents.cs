using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.UI.Components;

public struct UiTransformComponent : IComponent
{
    public Rectangle Bounds;
    public int ZIndex;

    public UiTransformComponent(int x, int y, int width, int height, int zIndex = 0)
    {
        Bounds = new Rectangle(x, y, width, height);
        ZIndex = zIndex;
    }
}

public enum UiState
{
    Normal,
    Hovered,
    Pressed,
    Focused
}

public struct UiStateComponent : IComponent
{
    public UiState State;
}

public struct UiTextComponent : IComponent
{
    public string Text;
    public string FontName; // e.g., "Fonts/Silkscreen"
    public Color? Color;
    public bool CenterX;
    public bool CenterY;
    public bool HasShadow;
    public Color ShadowColor;
    public float Scale;

    public UiTextComponent(string text, string fontName = "Fonts/Silkscreen")
    {
        Text = text;
        FontName = fontName;
        Color = null; // Use theme if null
        CenterX = false;
        CenterY = false;
        HasShadow = false;
        ShadowColor = Microsoft.Xna.Framework.Color.Black;
        Scale = 1.0f;
    }
}

public struct UiPanelComponent : IComponent
{
    // Marker for static cards/panels
}

public struct UiButtonComponent : IComponent
{
    public string ActionId;

    public UiButtonComponent(string actionId)
    {
        ActionId = actionId;
    }
}

public struct UiModalComponent : IComponent
{
    // Marker for modal components. When present, interaction is restricted to entities with this component.
}
