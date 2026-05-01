using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.UI.Components;
using GameEngineLab.Core.Features.UI.Resources;
using Microsoft.Xna.Framework.Input;
using System.Text;

namespace GameEngineLab.Core.Features.UI.Systems;

public sealed class UiTextInputSystem : IGameSystem
{
    public int Order => -40;

    public void Update(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<UiFocusResource>(out var focusResource) || focusResource == null || focusResource.FocusedEntity.Value == -1)
        {
            return;
        }

        var focusedEntity = focusResource.FocusedEntity;
        if (!world.TryGetComponent<UiTextInputComponent>(focusedEntity, out var textInput))
        {
            return;
        }

        var keyboard = frameContext.CurrentKeyboard;
        var prevKeyboard = frameContext.PreviousKeyboard;
        var newText = new StringBuilder(textInput.Text);
        bool changed = false;

        foreach (var key in keyboard.GetPressedKeys())
        {
            if (prevKeyboard.IsKeyUp(key))
            {
                if (key == Keys.Back && newText.Length > 0)
                {
                    newText.Remove(newText.Length - 1, 1);
                    changed = true;
                }
                else if (newText.Length < textInput.MaxLength)
                {
                    char c = GetCharFromKey(key, keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift));
                    if (c != '\0')
                    {
                        newText.Append(c);
                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            textInput.Text = newText.ToString();
            world.SetComponent(focusedEntity, textInput);
        }
    }

    public void Draw(World world, FrameContext frameContext) { }

    private char GetCharFromKey(Keys key, bool shift)
    {
        // Letters
        if (key >= Keys.A && key <= Keys.Z)
        {
            return (char)((shift ? 'A' : 'a') + (key - Keys.A));
        }

        // Digits
        if (key >= Keys.D0 && key <= Keys.D9)
        {
            if (shift)
            {
                return key switch
                {
                    Keys.D1 => '!', Keys.D2 => '@', Keys.D3 => '#', Keys.D4 => '$', Keys.D5 => '%',
                    Keys.D6 => '^', Keys.D7 => '&', Keys.D8 => '*', Keys.D9 => '(', Keys.D0 => ')',
                    _ => '\0'
                };
            }
            return (char)('0' + (key - Keys.D0));
        }

        // Keypad Digits
        if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
        {
            return (char)('0' + (key - Keys.NumPad0));
        }

        // Symbols
        return key switch
        {
            Keys.Space => ' ',
            Keys.OemMinus => shift ? '_' : '-',
            Keys.OemPlus => shift ? '+' : '=',
            Keys.OemOpenBrackets => shift ? '{' : '[',
            Keys.OemCloseBrackets => shift ? '}' : ']',
            Keys.OemSemicolon => shift ? ':' : ';',
            Keys.OemQuotes => shift ? '"' : '\'',
            Keys.OemComma => shift ? '<' : ',',
            Keys.OemPeriod => shift ? '>' : '.',
            Keys.OemQuestion => shift ? '?' : '/',
            Keys.OemBackslash => shift ? '|' : '\\',
            Keys.OemTilde => shift ? '~' : '`',
            _ => '\0'
        };
    }
}
