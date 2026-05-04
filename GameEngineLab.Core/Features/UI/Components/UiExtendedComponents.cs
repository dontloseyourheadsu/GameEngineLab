using GameEngineLab.Core.Features.Ecs.Components;

namespace GameEngineLab.Core.Features.UI.Components;

public struct UiCheckboxComponent : IComponent
{
    public bool Checked;

    public UiCheckboxComponent(bool isChecked = false)
    {
        Checked = isChecked;
    }
}

public struct UiTextInputComponent : IComponent
{
    public string Text;
    public int CursorPosition;
    public int MaxLength;

    public UiTextInputComponent(int maxLength = 20)
    {
        Text = string.Empty;
        CursorPosition = 0;
        MaxLength = maxLength;
    }
}

public struct UiSelectorComponent : IComponent
{
    public string[] Options;
    public int SelectedIndex;
    public bool IsOpen;

    public UiSelectorComponent(string[] options, int selectedIndex = 0)
    {
        Options = options;
        SelectedIndex = selectedIndex;
        IsOpen = false;
    }
}

public struct UiSliderComponent : IComponent // For Scroll bars and "Radius" sliders
{
    public float Value; // 0 to 1
    public bool Vertical;

    public UiSliderComponent(float value = 0.5f, bool vertical = false)
    {
        Value = value;
        Vertical = vertical;
    }
}
