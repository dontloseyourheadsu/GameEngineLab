using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.Pacman.Features.UI.Systems;

public sealed class MenuSystem : IGameSystem
{
    public int Order => -10;

    public void Update(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();

        if (appMode.Mode == AppMode.Gameplay)
        {
            if (IsNewKeyPress(frameContext, Keys.Tab))
            {
                appMode.Mode = AppMode.Menu;
            }

            return;
        }

        if (appMode.Mode is AppMode.AssetEditor or AppMode.Options)
        {
            if (IsNewKeyPress(frameContext, Keys.Escape))
            {
                appMode.Mode = AppMode.Menu;
            }

            return;
        }

        if (appMode.Mode != AppMode.Menu)
        {
            return;
        }

        if (IsNewKeyPress(frameContext, Keys.D1))
        {
            appMode.Mode = AppMode.Gameplay;
        }
        else if (IsNewKeyPress(frameContext, Keys.D2))
        {
            appMode.Mode = AppMode.MapEditor;
        }
        else if (IsNewKeyPress(frameContext, Keys.D3))
        {
            appMode.Mode = AppMode.AssetEditor;
        }
        else if (IsNewKeyPress(frameContext, Keys.D4))
        {
            appMode.Mode = AppMode.Options;
        }

        if (IsNewLeftClick(frameContext, out var mouse))
        {
            if (GetMenuButtonRect(0).Contains(mouse))
            {
                appMode.Mode = AppMode.Gameplay;
            }
            else if (GetMenuButtonRect(1).Contains(mouse))
            {
                appMode.Mode = AppMode.MapEditor;
            }
            else if (GetMenuButtonRect(2).Contains(mouse))
            {
                appMode.Mode = AppMode.AssetEditor;
            }
            else if (GetMenuButtonRect(3).Contains(mouse))
            {
                appMode.Mode = AppMode.Options;
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch is null || frameContext.DebugPixel is null)
        {
            return;
        }

        var appMode = world.GetRequiredResource<AppModeResource>();

        if (appMode.Mode == AppMode.Menu)
        {
            var bg = new Rectangle(0, 0, 1600, 1000);
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, bg, new Color(12, 18, 32));

            for (var i = 0; i < 4; i++)
            {
                var rect = GetMenuButtonRect(i);
                var color = i switch
                {
                    0 => new Color(36, 88, 170),
                    1 => new Color(42, 116, 82),
                    2 => new Color(120, 90, 44),
                    _ => new Color(82, 82, 128),
                };

                frameContext.SpriteBatch.Draw(frameContext.DebugPixel, rect, color);
                frameContext.SpriteBatch.Draw(frameContext.DebugPixel, new Rectangle(rect.X + 8, rect.Y + 8, rect.Width - 16, rect.Height - 16), new Color(255, 255, 255, 22));
            }
        }
        else if (appMode.Mode is AppMode.AssetEditor or AppMode.Options)
        {
            var shade = new Rectangle(100, 100, 800, 500);
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, shade, new Color(0, 0, 0, 160));
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, new Rectangle(130, 130, 740, 440), new Color(220, 220, 220, 30));
        }
    }

    private static Rectangle GetMenuButtonRect(int index)
    {
        return new Rectangle(320, 170 + (index * 95), 380, 72);
    }

    private static bool IsNewKeyPress(FrameContext frameContext, Keys key)
    {
        return frameContext.CurrentKeyboard.IsKeyDown(key) && frameContext.PreviousKeyboard.IsKeyUp(key);
    }

    private static bool IsNewLeftClick(FrameContext frameContext, out Point point)
    {
        point = frameContext.CurrentMouse.Position;
        return frameContext.CurrentMouse.LeftButton == ButtonState.Pressed
               && frameContext.PreviousMouse.LeftButton == ButtonState.Released;
    }
}
