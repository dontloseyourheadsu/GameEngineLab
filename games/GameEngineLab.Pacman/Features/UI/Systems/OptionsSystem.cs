using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.UI.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameEngineLab.Pacman.Features.UI.Systems;

public sealed class OptionsSystem : IGameSystem
{
    public int Order => -3;

    private static readonly Color ColorBg = new(8, 8, 16);
    private static readonly Color ColorPanel = new(16, 16, 32);
    private static readonly Color ColorNeonCyan = new(0, 255, 255);
    private static readonly Color ColorNeonGreen = new(0, 255, 128);
    private static readonly Color ColorSliderBg = new(32, 32, 48);
    private static readonly Color ColorSliderFill = new(0, 180, 255);
    private static readonly Color ColorText = new(220, 230, 255);

    public void Update(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Options)
        {
            return;
        }

        var options = world.GetRequiredResource<OptionsResource>();
        var layout = BuildLayout(frameContext.Viewport.Width, frameContext.Viewport.Height, options.UiScale);

        if (IsNewKeyPress(frameContext, Keys.Escape))
        {
            options.SyncPendingFromCurrent();
            options.DraggingMusic = false;
            options.DraggingSfx = false;
            options.DraggingScale = false;
            appMode.Mode = AppMode.Menu;
            return;
        }

        if (IsNewLeftClick(frameContext, out var click))
        {
            if (layout.ApplyButton.Contains(click))
            {
                options.ApplyPending();
                OptionsStorage.Save(OptionsPaths.SettingsPath, options);
                return;
            }

            options.DraggingMusic = Expand(layout.SliderMusic, 10).Contains(click);
            options.DraggingSfx = Expand(layout.SliderSfx, 10).Contains(click);
            options.DraggingScale = Expand(layout.SliderScale, 10).Contains(click);
        }

        if (IsNewKeyPress(frameContext, Keys.Enter))
        {
            options.ApplyPending();
            OptionsStorage.Save(OptionsPaths.SettingsPath, options);
        }

        if (frameContext.CurrentMouse.LeftButton == ButtonState.Released)
        {
            options.DraggingMusic = false;
            options.DraggingSfx = false;
            options.DraggingScale = false;
        }

        if (options.DraggingMusic)
        {
            options.PendingMusicVolume = ReadNormalized(frameContext.CurrentMouse.X, layout.SliderMusic);
        }

        if (options.DraggingSfx)
        {
            options.PendingSfxVolume = ReadNormalized(frameContext.CurrentMouse.X, layout.SliderSfx);
        }

        if (options.DraggingScale)
        {
            var t = ReadNormalized(frameContext.CurrentMouse.X, layout.SliderScale);
            options.PendingUiScale = 0.5f + (t * 1.5f); // 0.5x to 2.0x
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch is null || frameContext.DebugPixel is null)
        {
            return;
        }

        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Options)
        {
            return;
        }

        var options = world.GetRequiredResource<OptionsResource>();
        var sb = frameContext.SpriteBatch;
        var pixel = frameContext.DebugPixel;
        var layout = BuildLayout(frameContext.Viewport.Width, frameContext.Viewport.Height, options.UiScale);

        sb.Draw(pixel, new Rectangle(0, 0, frameContext.Viewport.Width, frameContext.Viewport.Height), ColorBg);
        
        // Panel
        sb.Draw(pixel, layout.Panel, ColorPanel);
        sb.Draw(pixel, new Rectangle(layout.Panel.X, layout.Panel.Y, layout.Panel.Width, 2), ColorNeonCyan);
        sb.Draw(pixel, new Rectangle(layout.Panel.X, layout.Panel.Bottom - 2, layout.Panel.Width, 2), ColorNeonCyan);

        PixelText.Draw(sb, pixel, "OPTIONS", new Vector2(layout.Panel.X + 30, layout.Panel.Y + 30), (int)(3 * options.UiScale), ColorNeonCyan);

        DrawSlider(frameContext, "MUSIC VOLUME", options.PendingMusicVolume, layout.SliderMusic, 0f, 1f, options.UiScale);
        DrawSlider(frameContext, "SFX VOLUME", options.PendingSfxVolume, layout.SliderSfx, 0f, 1f, options.UiScale);
        DrawSlider(frameContext, "UI SCALE", options.PendingUiScale, layout.SliderScale, 0.5f, 2.0f, options.UiScale);

        // Apply Button
        sb.Draw(pixel, layout.ApplyButton, ColorNeonGreen);
        var applyText = "APPLY";
        var applySize = PixelText.Measure(applyText, (int)(2 * options.UiScale));
        PixelText.Draw(sb, pixel, applyText, new Vector2(layout.ApplyButton.Center.X - applySize.X/2, layout.ApplyButton.Center.Y - applySize.Y/2), (int)(2 * options.UiScale), Color.Black);

        PixelText.Draw(sb, pixel, "ESC:BACK  ENTER:APPLY", new Vector2(layout.Panel.X + 30, layout.Panel.Bottom - 40), (int)(1 * options.UiScale), ColorText);
    }

    private static void DrawSlider(FrameContext frameContext, string name, float value, Rectangle rect, float min, float max, float scale)
    {
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;

        sb.Draw(pixel, rect, ColorSliderBg);

        var t = (value - min) / (max - min);
        var fill = new Rectangle(rect.X, rect.Y, (int)(rect.Width * t), rect.Height);
        sb.Draw(pixel, fill, ColorSliderFill);

        // Knob
        var knobW = (int)(10 * scale);
        var knobH = rect.Height + (int)(10 * scale);
        var knob = new Rectangle(rect.X + fill.Width - knobW / 2, rect.Y - (int)(5 * scale), knobW, knobH);
        sb.Draw(pixel, knob, Color.White);

        PixelText.Draw(sb, pixel, name, new Vector2(rect.X, rect.Y - (int)(25 * scale)), (int)(1 * scale), ColorText);

        string valText = max <= 1.01f ? $"{(int)(value * 100)}%" : $"{value:0.0}X";
        PixelText.Draw(sb, pixel, valText, new Vector2(rect.Right + 20, rect.Y), (int)(1 * scale), ColorNeonCyan);
    }

    private static Rectangle Expand(Rectangle rect, int padding)
    {
        return new Rectangle(rect.X - padding, rect.Y - padding, rect.Width + padding * 2, rect.Height + padding * 2);
    }

    private static float ReadNormalized(int mouseX, Rectangle slider)
    {
        return Math.Clamp((mouseX - slider.X) / (float)slider.Width, 0f, 1f);
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

    private static OptionsLayout BuildLayout(int w, int h, float userScale)
    {
        float autoScale = Math.Max(1.0f, Math.Min(w / 1024f, h / 768f));
        float scale = userScale * autoScale;

        var panelW = (int)(500 * scale);
        var panelH = (int)(400 * scale);
        var panel = new Rectangle((w - panelW) / 2, (h - panelH) / 2, panelW, panelH);

        var sliderW = (int)(300 * scale);
        var sliderH = (int)(16 * scale);
        var startX = panel.X + (panelW - sliderW) / 2 - (int)(20 * scale);
        
        return new OptionsLayout
        {
            Scale = scale,
            Panel = panel,
            SliderMusic = new Rectangle(startX, panel.Y + (int)(120 * scale), sliderW, sliderH),
            SliderSfx = new Rectangle(startX, panel.Y + (int)(200 * scale), sliderW, sliderH),
            SliderScale = new Rectangle(startX, panel.Y + (int)(280 * scale), sliderW, sliderH),
            ApplyButton = new Rectangle(panel.Right - (int)(120 * scale), panel.Bottom - (int)(60 * scale), (int)(100 * scale), (int)(40 * scale))
        };
    }

    private sealed class OptionsLayout
    {
        public float Scale { get; init; }
        public Rectangle Panel { get; init; }
        public Rectangle SliderMusic { get; init; }
        public Rectangle SliderSfx { get; init; }
        public Rectangle SliderScale { get; init; }
        public Rectangle ApplyButton { get; init; }
    }
}
