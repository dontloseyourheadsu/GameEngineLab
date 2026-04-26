using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GameEngineLab.Pacman.Features.Map.Systems;

public sealed class MapGroupSelectorSystem : IGameSystem
{
    public int Order => -12;

    private static readonly Color ColorBg = new(8, 8, 16);
    private static readonly Color ColorPanel = new(16, 16, 32);
    private static readonly Color ColorNeonCyan = new(0, 255, 255);
    private static readonly Color ColorNeonMagenta = new(255, 0, 255);
    private static readonly Color ColorNeonGreen = new(0, 255, 128);
    private static readonly Color ColorNeonYellow = new(255, 255, 0);
    private static readonly Color ColorText = new(220, 230, 255);
    private static readonly Color ColorTextDim = new(140, 150, 180);

    public void Update(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.MapGroupSelector) return;

        var lib = world.GetRequiredResource<MapLibraryResource>();
        var options = world.GetRequiredResource<OptionsResource>();
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        float autoScale = Math.Max(1.2f, Math.Min(sw / 1024f, sh / 768f));
        float scale = options.UiScale * autoScale;

        if (IsNewKeyPress(frameContext, Keys.Escape))
        {
            appMode.Mode = AppMode.Menu;
            return;
        }

        if (IsNewLeftClick(frameContext, out var mouse))
        {
            // Back
            if (GetRect(20, 20, 160, 60, scale).Contains(mouse))
            {
                appMode.Mode = AppMode.Menu;
                return;
            }

            // New Map
            var newBtnW = (int)(220 * scale);
            var newBtnH = (int)(60 * scale);
            var newBtnRect = new Rectangle(sw - newBtnW - (int)(20 * scale), (int)(20 * scale), newBtnW, newBtnH);
            if (newBtnRect.Contains(mouse))
            {
                var newProj = MapEditorStorage.CreateDefaultProject($"Map {lib.Projects.Count + 1}", 20, 20);
                lib.Projects.Add(newProj);
                MapEditorStorage.SaveLibrary(MapPaths.MapLibrary, lib);
                return;
            }

            // List Items
            for (int i = 0; i < lib.Projects.Count; i++)
            {
                var rect = GetItemRect(i, sw, sh, scale);
                if (rect.Contains(mouse))
                {
                    lib.SelectedProjectIndex = i;
                    
                    // Specific "EDIT" button check
                    var editBtn = new Rectangle(rect.Right - (int)(180 * scale), rect.Y + (int)(15 * scale), (int)(100 * scale), (int)(40 * scale));
                    if (editBtn.Contains(mouse))
                    {
                        var editor = world.GetRequiredResource<MapEditorResource>();
                        editor.ActiveProject = lib.Projects[i];
                        editor.Dirty = false;
                        appMode.Mode = AppMode.MapEditor;
                    }
                    else
                    {
                        appMode.Mode = AppMode.AssetGroupSelector;
                    }
                    return;
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.MapGroupSelector) return;

        var lib = world.GetRequiredResource<MapLibraryResource>();
        var options = world.GetRequiredResource<OptionsResource>();
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        float autoScale = Math.Max(1.2f, Math.Min(sw / 1024f, sh / 768f));
        float scale = options.UiScale * autoScale;

        sb.Draw(pixel, new Rectangle(0, 0, sw, sh), ColorBg);

        // Header
        var title = "MAP PROJECTS";
        var tScale = (int)(4 * scale);
        var tSize = PixelText.Measure(title, tScale);
        PixelText.Draw(sb, pixel, title, new Vector2((sw - tSize.X) / 2, 35 * scale), tScale, ColorNeonCyan);

        DrawButton(sb, pixel, GetRect(20, 20, 160, 60, scale), "BACK", ColorNeonMagenta, scale, 2);
        
        var newBtnW = (int)(220 * scale);
        var newBtnH = (int)(60 * scale);
        var newBtnRect = new Rectangle(sw - newBtnW - (int)(20 * scale), (int)(20 * scale), newBtnW, newBtnH);
        DrawButton(sb, pixel, newBtnRect, "NEW MAP", ColorNeonGreen, scale, 2);

        for (int i = 0; i < lib.Projects.Count; i++)
        {
            var proj = lib.Projects[i];
            var rect = GetItemRect(i, sw, sh, scale);
            
            sb.Draw(pixel, rect, ColorPanel);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), ColorNeonCyan);

            PixelText.Draw(sb, pixel, proj.Name, new Vector2(rect.X + (int)(25 * scale), rect.Y + (int)(30 * scale)), (int)(2 * scale), ColorText);
            PixelText.Draw(sb, pixel, $"{proj.Width}x{proj.Height}", new Vector2(rect.X + (int)(25 * scale), rect.Y + (int)(65 * scale)), (int)(1 * scale), ColorTextDim);
            
            var editBtn = new Rectangle(rect.Right - (int)(240 * scale), rect.Y + (int)(25 * scale), (int)(120 * scale), (int)(60 * scale));
            DrawButton(sb, pixel, editBtn, "EDIT", ColorNeonCyan, scale, 1);

            string status = proj.IsDone ? "DONE" : "WIP";
            PixelText.Draw(sb, pixel, status, new Vector2(rect.Right - (int)(100 * scale), rect.Y + (int)(40 * scale)), (int)(1 * scale), proj.IsDone ? ColorNeonGreen : ColorNeonYellow);

            // Small grid preview
            DrawMapPreview(sb, pixel, proj, new Rectangle(rect.X + (int)(320 * scale), rect.Y + 10, (int)(150 * scale), (int)(90 * scale)));
        }
    }

    private static void DrawMapPreview(SpriteBatch sb, Texture2D pixel, MapProject proj, Rectangle rect)
    {
        int pSize = Math.Max(1, Math.Min(rect.Width / proj.Width, rect.Height / proj.Height));
        for (int y = 0; y < proj.Height; y++)
        {
            for (int x = 0; x < proj.Width; x++)
            {
                var color = GetTileColor(proj.Tiles[y][x]);
                if (color.A > 0)
                    sb.Draw(pixel, new Rectangle(rect.X + x * pSize, rect.Y + y * pSize, pSize, pSize), color);
            }
        }
    }

    private static Color GetTileColor(char tile) => tile switch
    {
        '#' => new Color(30, 60, 120),
        'P' => new Color(200, 180, 50),
        'S' => new Color(80, 40, 100),
        '.' => new Color(40, 45, 80),
        'o' => new Color(100, 100, 150),
        _ => Color.Transparent,
    };

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
