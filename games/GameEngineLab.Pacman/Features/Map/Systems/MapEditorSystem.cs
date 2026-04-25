using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Maps.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.Pacman.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameEngineLab.Pacman.Features.Map.Systems;

public sealed class MapEditorSystem : IGameSystem
{
    public int Order => -5;

    private static readonly Color ColorBg = new(8, 8, 16);
    private static readonly Color ColorPanel = new(16, 16, 32);
    private static readonly Color ColorNeonCyan = new(0, 255, 255);
    private static readonly Color ColorNeonYellow = new(255, 255, 0);
    private static readonly Color ColorNeonGreen = new(0, 255, 128);
    private static readonly Color ColorNeonRed = new(255, 50, 50);
    private static readonly Color ColorText = new(220, 230, 255);
    private static readonly Color ColorTextDim = new(140, 150, 180);

    public void Update(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.MapEditor) return;

        var editor = world.GetRequiredResource<MapEditorResource>();
        if (editor.ActiveProject == null)
        {
            appMode.Mode = AppMode.MapGroupSelector;
            return;
        }

        var options = world.GetRequiredResource<OptionsResource>();
        var layout = BuildLayout(frameContext.Viewport.Width, frameContext.Viewport.Height, editor.ActiveProject, options.UiScale);

        if (IsNewKeyPress(frameContext, Keys.Escape))
        {
            appMode.Mode = AppMode.MapGroupSelector;
            return;
        }

        if (IsNewKeyPress(frameContext, Keys.Enter))
        {
            Save(world);
        }

        for (var i = 0; i < editor.Palette.Length; i++)
        {
            if (IsNewKeyPress(frameContext, (Keys)((int)Keys.D1 + i)))
            {
                editor.SelectedPaletteIndex = i;
            }
        }

        var mouse = frameContext.CurrentMouse.Position;
        if (frameContext.CurrentMouse.LeftButton == ButtonState.Pressed || frameContext.CurrentMouse.RightButton == ButtonState.Pressed)
        {
            if (layout.MapRect.Contains(mouse))
            {
                TryPaint(editor.ActiveProject, mouse, layout.MapRect, frameContext.CurrentMouse.LeftButton == ButtonState.Pressed, editor.Palette[editor.SelectedPaletteIndex]);
                editor.Dirty = true;
            }
            else if (IsNewLeftClick(frameContext, out var click))
            {
                if (layout.SaveButton.Contains(click)) Save(world);
                else if (layout.DiscardButton.Contains(click)) appMode.Mode = AppMode.MapGroupSelector;
                else
                {
                    for (int i = 0; i < editor.Palette.Length; i++)
                    {
                        if (GetPaletteRect(layout, i).Contains(click))
                        {
                            editor.SelectedPaletteIndex = i;
                            break;
                        }
                    }
                }
            }
        }
    }

    private static void Save(World world)
    {
        var lib = world.GetRequiredResource<MapLibraryResource>();
        var editor = world.GetRequiredResource<MapEditorResource>();
        if (editor.ActiveProject != null)
        {
            editor.ActiveProject.IsDone = MapEditorStorage.ValidateMap(editor.ActiveProject, out _);
            MapEditorStorage.SaveLibrary(MapPaths.MapLibrary, lib);
            editor.Dirty = false;
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.MapEditor) return;

        var editor = world.GetRequiredResource<MapEditorResource>();
        if (editor.ActiveProject == null) return;

        var options = world.GetRequiredResource<OptionsResource>();
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        var layout = BuildLayout(sw, sh, editor.ActiveProject, options.UiScale);

        sb.Draw(pixel, new Rectangle(0, 0, sw, sh), ColorBg);

        // Header
        PixelText.Draw(sb, pixel, $"MAP: {editor.ActiveProject.Name.ToUpper()}", new Vector2(20, 15), (int)Math.Max(1, 4 * options.UiScale), ColorNeonCyan);

        // Panels
        DrawPanel(sb, pixel, layout.PalettePanel, "PALETTE", ColorNeonCyan, layout.Scale);
        DrawPanel(sb, pixel, layout.InfoPanel, "INFO", ColorNeonYellow, layout.Scale);
        DrawPanel(sb, pixel, layout.ActionPanel, "ACTIONS", ColorNeonGreen, layout.Scale);

        // Map Grid
        int tileSize = layout.CellSize;
        sb.Draw(pixel, layout.MapRect, ColorPanel);
        for (int y = 0; y < editor.ActiveProject.Height; y++)
        {
            for (int x = 0; x < editor.ActiveProject.Width; x++)
            {
                var tile = editor.ActiveProject.Tiles[y][x];
                var rect = new Rectangle(layout.MapRect.X + x * tileSize, layout.MapRect.Y + y * tileSize, tileSize, tileSize);
                var color = GetColor(tile);
                if (color.A > 0) sb.Draw(pixel, rect, color);
                
                sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), new Color(255, 255, 255, 10));
                sb.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), new Color(255, 255, 255, 10));
            }
        }

        // Palette Buttons
        for (int i = 0; i < editor.Palette.Length; i++)
        {
            var pRect = GetPaletteRect(layout, i);
            var color = GetColor(editor.Palette[i]);
            sb.Draw(pixel, pRect, color);
            if (i == editor.SelectedPaletteIndex)
            {
                var border = (int)Math.Max(1, 4 * layout.Scale);
                sb.Draw(pixel, new Rectangle(pRect.X - border, pRect.Y - border, pRect.Width + border * 2, border), Color.White);
                sb.Draw(pixel, new Rectangle(pRect.X - border, pRect.Bottom, pRect.Width + border * 2, border), Color.White);
                sb.Draw(pixel, new Rectangle(pRect.X - border, pRect.Y - border, border, pRect.Height + border * 2), Color.White);
                sb.Draw(pixel, new Rectangle(pRect.Right, pRect.Y - border, border, pRect.Height + border * 2), Color.White);
            }
            PixelText.Draw(sb, pixel, (i + 1).ToString(), new Vector2(pRect.X + 5, pRect.Y - (int)(25 * layout.Scale)), (int)Math.Max(1, 2 * layout.Scale), ColorText);
        }

        // Action Buttons
        DrawButton(sb, pixel, layout.SaveButton, "SAVE", ColorNeonGreen, layout.Scale, 2);
        DrawButton(sb, pixel, layout.DiscardButton, "BACK", ColorNeonRed, layout.Scale, 2);

        // Validation / Info
        MapEditorStorage.ValidateMap(editor.ActiveProject, out var errors);
        float infoY = layout.InfoPanel.Y + 40 * layout.Scale;
        if (errors.Count == 0)
        {
            PixelText.Draw(sb, pixel, "MAP VALID", new Vector2(layout.InfoPanel.X + 25, infoY), (int)Math.Max(1, 2 * layout.Scale), ColorNeonGreen);
        }
        else
        {
            foreach (var err in errors.Take(4))
            {
                PixelText.Draw(sb, pixel, $"- {err}", new Vector2(layout.InfoPanel.X + 15, infoY), (int)Math.Max(1, 1 * layout.Scale), ColorNeonRed);
                infoY += 35 * layout.Scale;
            }
        }

        // Hover
        if (TryGetTileUnderMouse(frameContext.CurrentMouse.Position, layout.MapRect, editor.ActiveProject, out var cell))
        {
            var hRect = new Rectangle(layout.MapRect.X + cell.X * tileSize, layout.MapRect.Y + cell.Y * tileSize, tileSize, tileSize);
            sb.Draw(pixel, new Rectangle(hRect.X, hRect.Y, hRect.Width, 2), Color.White);
            sb.Draw(pixel, new Rectangle(hRect.X, hRect.Bottom - 2, hRect.Width, 2), Color.White);
            sb.Draw(pixel, new Rectangle(hRect.X, hRect.Y, 2, hRect.Height), Color.White);
            sb.Draw(pixel, new Rectangle(hRect.Right - 2, hRect.Y, 2, hRect.Height), Color.White);
        }
    }

    private static void DrawPanel(SpriteBatch sb, Texture2D pixel, Rectangle rect, string title, Color accent, float scale)
    {
        sb.Draw(pixel, rect, ColorPanel);
        // Border
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), accent);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), accent);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, 2, rect.Height), accent);
        sb.Draw(pixel, new Rectangle(rect.Right - 2, rect.Y, 2, rect.Height), accent);

        // Title
        PixelText.Draw(sb, pixel, title, new Vector2(rect.X + 8, rect.Y - (int)(18 * scale)), (int)Math.Max(1, 2 * scale), accent);
    }

    private static void DrawButton(SpriteBatch sb, Texture2D pixel, Rectangle rect, string text, Color color, float scale, int tScale)
    {
        sb.Draw(pixel, rect, ColorPanel);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), color);
        sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - 2, rect.Width, 2), color);
        var size = PixelText.Measure(text, (int)(tScale * scale));
        PixelText.Draw(sb, pixel, text, new Vector2(rect.Center.X - size.X / 2, rect.Center.Y - size.Y / 2), (int)(tScale * scale), color);
    }

    private static void TryPaint(MapProject proj, Point mouse, Rectangle mapRect, bool paint, char paletteChar)
    {
        if (TryGetTileUnderMouse(mouse, mapRect, proj, out var tile))
        {
            proj.Tiles[tile.Y][tile.X] = paint ? paletteChar : ' ';
        }
    }

    private static bool TryGetTileUnderMouse(Point mouse, Rectangle mapRect, MapProject proj, out Point tile)
    {
        tile = default;
        if (!mapRect.Contains(mouse)) return false;
        int tx = (mouse.X - mapRect.X) * proj.Width / mapRect.Width;
        int ty = (mouse.Y - mapRect.Y) * proj.Height / mapRect.Height;
        if (tx < 0 || ty < 0 || tx >= proj.Width || ty >= proj.Height) return false;
        tile = new Point(tx, ty);
        return true;
    }

    private static Color GetColor(char tile) => tile switch
    {
        '#' => new Color(30, 60, 120),
        'P' => new Color(200, 180, 50),
        'S' => new Color(80, 40, 100),
        '.' => new Color(40, 45, 80),
        'o' => new Color(100, 100, 150),
        _ => Color.Transparent,
    };

    private static EditorLayout BuildLayout(int w, int h, MapProject proj, float userScale)
    {
        float autoScale = Math.Max(1.0f, Math.Min(w / 1024f, h / 768f));
        float scale = userScale * autoScale;

        var sideW = (int)(320 * scale);
        var actionH = (int)(140 * scale);
        var headerH = (int)(100 * scale);
        var margin = (int)(25 * scale);

        var palettePanel = new Rectangle(margin, headerH, sideW, (int)(320 * scale));
        var infoPanel = new Rectangle(margin, palettePanel.Bottom + margin, sideW, h - palettePanel.Bottom - margin * 2);
        
        var actionPanel = new Rectangle(w - sideW - margin, headerH, sideW, actionH);
        
        var mapAreaX = palettePanel.Right + margin;
        var mapAreaW = w - sideW * 2 - margin * 4;
        var mapAreaH = h - headerH - margin;

        int cellSize = Math.Max(1, Math.Min(mapAreaW, mapAreaH) / Math.Max(proj.Width, proj.Height));
        int mapW = cellSize * proj.Width;
        int mapH = cellSize * proj.Height;

        return new EditorLayout
        {
            Scale = scale,
            MapRect = new Rectangle(mapAreaX + (mapAreaW - mapW) / 2, headerH + (mapAreaH - mapH) / 2, mapW, mapH),
            PalettePanel = palettePanel,
            InfoPanel = infoPanel,
            ActionPanel = actionPanel,
            CellSize = cellSize,
            SaveButton = new Rectangle(actionPanel.X + (int)(15 * scale), actionPanel.Y + (int)(40 * scale), (sideW - (int)(45 * scale)) / 2, (int)(70 * scale)),
            DiscardButton = new Rectangle(actionPanel.X + (sideW + (int)(15 * scale)) / 2, actionPanel.Y + (int)(40 * scale), (sideW - (int)(45 * scale)) / 2, (int)(70 * scale)),
        };
    }

    private static Rectangle GetPaletteRect(EditorLayout layout, int index)
    {
        var col = index % 3;
        var row = index / 3;
        var size = (int)(70 * layout.Scale);
        var gap = (int)(20 * layout.Scale);
        return new Rectangle(layout.PalettePanel.X + (int)(25 * layout.Scale) + col * (size + gap), layout.PalettePanel.Y + (int)(60 * layout.Scale) + row * (size + gap), size, size);
    }

    private static bool IsNewKeyPress(FrameContext frameContext, Keys key) => frameContext.CurrentKeyboard.IsKeyDown(key) && frameContext.PreviousKeyboard.IsKeyUp(key);
    private static bool IsNewLeftClick(FrameContext frameContext, out Point point)
    {
        point = frameContext.CurrentMouse.Position;
        return frameContext.CurrentMouse.LeftButton == ButtonState.Pressed && frameContext.PreviousMouse.LeftButton == ButtonState.Released;
    }

    private sealed class EditorLayout
    {
        public float Scale { get; init; }
        public Rectangle MapRect { get; init; }
        public Rectangle PalettePanel { get; init; }
        public Rectangle InfoPanel { get; init; }
        public Rectangle ActionPanel { get; init; }
        public int CellSize { get; init; }
        public Rectangle SaveButton { get; init; }
        public Rectangle DiscardButton { get; init; }
    }
}
