using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Assets.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GameEngineLab.Pacman.Features.Assets.Systems;

public sealed class AssetGroupSelectorSystem : IGameSystem
{
    public int Order => -11;

    private readonly Dictionary<string, List<Texture2D>> _groupThumbnails = new();
    private static readonly Color ColorBg = new(8, 8, 16);
    private static readonly Color ColorPanel = new(16, 16, 32);
    private static readonly Color ColorPanelAccent = new(24, 24, 48);
    private static readonly Color ColorNeonCyan = new(0, 255, 255);
    private static readonly Color ColorNeonMagenta = new(255, 0, 255);
    private static readonly Color ColorNeonGreen = new(0, 255, 128);
    private static readonly Color ColorNeonYellow = new(255, 255, 0);
    private static readonly Color ColorText = new(220, 230, 255);

    public void Update(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.AssetGroupSelector) return;

        var lib = world.GetRequiredResource<AssetLibraryResource>();
        var options = world.GetRequiredResource<OptionsResource>();
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        
        // Boosted scale for larger UI
        float autoScale = Math.Max(1.2f, Math.Min(sw / 1024f, sh / 768f));
        float scale = options.UiScale * autoScale;

        if (IsNewKeyPress(frameContext, Keys.Escape))
        {
            appMode.Mode = AppMode.Menu;
            return;
        }

        if (IsNewLeftClick(frameContext, out var mouse))
        {
            // Back Button
            if (GetRect(20, 20, 160, 60, scale).Contains(mouse))
            {
                appMode.Mode = AppMode.Menu;
                return;
            }

            // New Group Button - Anchored to right
            var newBtnW = (int)(220 * scale);
            var newBtnH = (int)(60 * scale);
            var newBtnRect = new Rectangle(sw - newBtnW - (int)(20 * scale), (int)(20 * scale), newBtnW, newBtnH);
            
            if (newBtnRect.Contains(mouse))
            {
                var newGroup = AssetEditorStorage.CreateDefaultGroup($"Group {lib.Groups.Count + 1}");
                lib.Groups.Add(newGroup);
                AssetEditorStorage.SaveLibrary(AssetPaths.DefaultAssets, lib);
                return;
            }

            // List Items
            for (int i = 0; i < lib.Groups.Count; i++)
            {
                var rect = GetItemRect(i, sw, sh, scale);
                if (rect.Contains(mouse))
                {
                    lib.SelectedGroupIndex = i;

                    var editBtn = new Rectangle(rect.Right - (int)(240 * scale), rect.Y + (int)(25 * scale), (int)(120 * scale), (int)(60 * scale));
                    if (editBtn.Contains(mouse))
                    {
                        var editor = world.GetRequiredResource<AssetEditorResource>();
                        editor.ActiveGroup = lib.Groups[i];
                        editor.SelectedAssetIndex = 0;
                        editor.SelectedFrameIndex = 0;
                        editor.Dirty = false;
                        appMode.Mode = AppMode.AssetEditor;
                    }
                    else
                    {
                        // Proceed to Game Setup (Map is already selected if they came from Map Selector)
                        appMode.Mode = AppMode.GameSetup;
                    }
                    return;
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.AssetGroupSelector) return;

        var lib = world.GetRequiredResource<AssetLibraryResource>();
        var options = world.GetRequiredResource<OptionsResource>();
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        
        float autoScale = Math.Max(1.2f, Math.Min(sw / 1024f, sh / 768f));
        float scale = options.UiScale * autoScale;

        sb.Draw(pixel, new Rectangle(0, 0, sw, sh), ColorBg);

        // Header
        var title = "ASSET PROJECTS";
        var tScale = (int)(4 * scale);
        var tSize = PixelText.Measure(title, tScale);
        PixelText.Draw(sb, pixel, title, new Vector2((sw - tSize.X) / 2, 35 * scale), tScale, ColorNeonCyan);

        // Buttons
        DrawButton(sb, pixel, GetRect(20, 20, 160, 60, scale), "BACK", ColorNeonMagenta, scale, 2);
        
        var newBtnW = (int)(220 * scale);
        var newBtnH = (int)(60 * scale);
        var newBtnRect = new Rectangle(sw - newBtnW - (int)(20 * scale), (int)(20 * scale), newBtnW, newBtnH);
        DrawButton(sb, pixel, newBtnRect, "NEW PROJECT", ColorNeonGreen, scale, 2);

        // List
        for (int i = 0; i < lib.Groups.Count; i++)
        {
            var group = lib.Groups[i];
            var rect = GetItemRect(i, sw, sh, scale);
            
            sb.Draw(pixel, rect, ColorPanel);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), ColorNeonCyan);

            PixelText.Draw(sb, pixel, group.Name, new Vector2(rect.X + (int)(25 * scale), rect.Y + (int)(35 * scale)), (int)(2 * scale), ColorText);
            
            var editBtn = new Rectangle(rect.Right - (int)(240 * scale), rect.Y + (int)(25 * scale), (int)(120 * scale), (int)(60 * scale));
            DrawButton(sb, pixel, editBtn, "EDIT", ColorNeonCyan, scale, 1);

            string status = group.IsDone ? "DONE" : "WIP";
            PixelText.Draw(sb, pixel, status, new Vector2(rect.Right - (int)(100 * scale), rect.Y + (int)(40 * scale)), (int)(1 * scale), group.IsDone ? ColorNeonGreen : ColorNeonYellow);

            // Thumbnails
            UpdateThumbnails(sb.GraphicsDevice, group);
            var thumbs = _groupThumbnails[group.Name];
            var thumbSize = (int)(80 * scale);
            for (int j = 0; j < thumbs.Count && j < 5; j++)
            {
                var tRect = new Rectangle(rect.X + (int)(320 * scale) + j * (thumbSize + (int)(15 * scale)), rect.Y + (rect.Height - thumbSize) / 2, thumbSize, thumbSize);
                sb.Draw(thumbs[j], tRect, Color.White);
                // Simple border for thumbnail
                sb.Draw(pixel, new Rectangle(tRect.X, tRect.Y, tRect.Width, 1), Color.White * 0.3f);
                sb.Draw(pixel, new Rectangle(tRect.X, tRect.Bottom-1, tRect.Width, 1), Color.White * 0.3f);
                sb.Draw(pixel, new Rectangle(tRect.X, tRect.Y, 1, tRect.Height), Color.White * 0.3f);
                sb.Draw(pixel, new Rectangle(tRect.Right-1, tRect.Y, 1, tRect.Height), Color.White * 0.3f);
            }
        }
        
        var hint = "SELECT A PROJECT TO START EDITING";
        var hSize = PixelText.Measure(hint, (int)Math.Max(1, 1 * scale));
        PixelText.Draw(sb, pixel, hint, new Vector2((sw - hSize.X) / 2, sh - 40), (int)Math.Max(1, 1 * scale), Color.Gray);
    }

    private void UpdateThumbnails(GraphicsDevice gd, AssetGroup group)
    {
        if (!_groupThumbnails.TryGetValue(group.Name, out var list))
        {
            list = new List<Texture2D>();
            foreach (var asset in group.Assets)
            {
                var tex = new Texture2D(gd, asset.Resolution, asset.Resolution);
                tex.SetData(asset.Frames[0]);
                list.Add(tex);
            }
            _groupThumbnails[group.Name] = list;
        }
    }

    private static void DrawButton(SpriteBatch sb, Texture2D pixel, Rectangle rect, string text, Color color, float scale, int textScale = 1)
    {
        sb.Draw(pixel, rect, ColorPanel);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
        
        var tScale = (int)(textScale * scale);
        var tSize = PixelText.Measure(text, tScale);
        PixelText.Draw(sb, pixel, text, new Vector2(rect.Center.X - tSize.X / 2, rect.Center.Y - tSize.Y / 2), tScale, color);
    }

    private static Rectangle GetRect(int x, int y, int w, int h, float scale) => new((int)(x * scale), (int)(y * scale), (int)(w * scale), (int)(h * scale));

    private static Rectangle GetItemRect(int index, int sw, int sh, float scale)
    {
        var w = (int)(sw * 0.85f);
        var h = (int)(110 * scale);
        var spacing = (int)(20 * scale);
        return new Rectangle((sw - w) / 2, (int)(150 * scale) + index * (h + spacing), w, h);
    }

    private static bool IsNewKeyPress(FrameContext frameContext, Keys key) => frameContext.CurrentKeyboard.IsKeyDown(key) && frameContext.PreviousKeyboard.IsKeyUp(key);
    private static bool IsNewLeftClick(FrameContext frameContext, out Point point)
    {
        point = frameContext.CurrentMouse.Position;
        return frameContext.CurrentMouse.LeftButton == ButtonState.Pressed && frameContext.PreviousMouse.LeftButton == ButtonState.Released;
    }
}
