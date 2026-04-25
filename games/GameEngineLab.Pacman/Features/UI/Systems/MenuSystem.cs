using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameEngineLab.Pacman.Features.UI.Systems;

public sealed class MenuSystem : IGameSystem
{
    public int Order => -10;

    private static readonly Color ColorBg = new(8, 8, 16);
    private static readonly Color ColorPanel = new(16, 16, 32);
    private static readonly Color ColorNeonCyan = new(0, 255, 255);
    private static readonly Color ColorNeonMagenta = new(255, 0, 255);
    private static readonly Color ColorNeonYellow = new(255, 255, 0);
    private static readonly Color ColorNeonGreen = new(0, 255, 128);

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

        if (appMode.Mode is AppMode.AssetEditor or AppMode.Options or AppMode.MapEditor)
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

        var options = world.GetRequiredResource<OptionsResource>();
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        float autoScale = Math.Max(1.0f, Math.Min(sw / 1024f, sh / 768f));
        var scale = options.UiScale * autoScale;

        if (IsNewKeyPress(frameContext, Keys.D1)) appMode.Mode = AppMode.GameSetup;
        else if (IsNewKeyPress(frameContext, Keys.D2)) appMode.Mode = AppMode.MapGroupSelector;
        else if (IsNewKeyPress(frameContext, Keys.D3)) appMode.Mode = AppMode.AssetGroupSelector;
        else if (IsNewKeyPress(frameContext, Keys.D4)) appMode.Mode = AppMode.Options;

        if (IsNewLeftClick(frameContext, out var mouse))
        {
            for (int i = 0; i < 4; i++)
            {
                if (GetMenuButtonRect(i, sw, sh, scale).Contains(mouse))
                {
                    appMode.Mode = i switch
                    {
                        0 => AppMode.GameSetup,
                        1 => AppMode.MapGroupSelector,
                        2 => AppMode.AssetGroupSelector,
                        _ => AppMode.Options
                    };
                    return;
                }
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
        if (appMode.Mode != AppMode.Menu)
        {
            return;
        }

        var options = world.GetRequiredResource<OptionsResource>();
        var sb = frameContext.SpriteBatch;
        var pixel = frameContext.DebugPixel;
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        float autoScale = Math.Max(1.0f, Math.Min(sw / 1024f, sh / 768f));
        var scale = options.UiScale * autoScale;

        sb.Draw(pixel, new Rectangle(0, 0, sw, sh), ColorBg);

        var title = "GAME ENGINE LAB";
        var tScale = (int)(5 * scale);
        var tSize = PixelText.Measure(title, tScale);
        PixelText.Draw(sb, pixel, title, new Vector2((sw - tSize.X) / 2, sh * 0.15f), tScale, ColorNeonCyan);

        var subtitle = "PACMAN EDITION";
        var sScale = (int)(2 * scale);
        var sSize = PixelText.Measure(subtitle, sScale);
        PixelText.Draw(sb, pixel, subtitle, new Vector2((sw - sSize.X) / 2, sh * 0.15f + tSize.Y + 10), sScale, ColorNeonMagenta);

        var labels = new[] { "1 PLAY", "2 MAP EDITOR", "3 ASSET EDITOR", "4 OPTIONS" };
        var colors = new[] { ColorNeonGreen, ColorNeonCyan, ColorNeonYellow, ColorNeonMagenta };

        for (var i = 0; i < 4; i++)
        {
            var rect = GetMenuButtonRect(i, sw, sh, scale);
            var color = colors[i];

            sb.Draw(pixel, rect, ColorPanel);
            // Neon border
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);

            var lScale = (int)(2 * scale);
            var lSize = PixelText.Measure(labels[i], lScale);
            PixelText.Draw(sb, pixel, labels[i], new Vector2(rect.Center.X - lSize.X / 2, rect.Center.Y - lSize.Y / 2), lScale, color);
        }

        var hint = "PRESS 1-4 OR CLICK TO START";
        var hScale = (int)(1 * scale);
        var hSize = PixelText.Measure(hint, hScale);
        PixelText.Draw(sb, pixel, hint, new Vector2((sw - hSize.X) / 2, sh - 40), hScale, Color.Gray);
    }

    private static Rectangle GetMenuButtonRect(int index, int sw, int sh, float scale)
    {
        var width = (int)(300 * scale);
        var height = (int)(60 * scale);
        var spacing = (int)(20 * scale);
        var totalH = (height * 4) + (spacing * 3);
        var startY = (sh - totalH) / 2 + (int)(50 * scale);
        return new Rectangle((sw - width) / 2, startY + index * (height + spacing), width, height);
    }

    private static bool IsNewKeyPress(FrameContext frameContext, Keys key)
    {
        return frameContext.CurrentKeyboard.IsKeyDown(key) && frameContext.PreviousKeyboard.IsKeyUp(key);
    }

    private static bool IsNewLeftClick(FrameContext frameContext, out Point point)
    {
        point = frameContext.CurrentMouse.Position;
        return frameContext.CurrentMouse.LeftButton == ButtonState.Pressed && frameContext.PreviousMouse.LeftButton == ButtonState.Released;
    }
}
