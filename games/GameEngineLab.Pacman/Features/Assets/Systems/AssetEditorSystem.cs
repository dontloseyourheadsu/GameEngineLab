using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Identity.Resources;
using GameEngineLab.Core.Features.Online.Resources;
using GameEngineLab.Core.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.Assets.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineLab.Pacman.Features.Assets.Systems;

public sealed class AssetEditorSystem : IGameSystem
{
    public int Order => -4;

    private Texture2D? _canvasTexture;
    private int _textureRes = -1;
    
    // For asset list thumbnails
    private readonly Dictionary<string, Texture2D> _assetThumbnails = new();

    // Theme Colors
    private static readonly Color ColorBg = new(8, 8, 16);
    private static readonly Color ColorPanel = new(16, 16, 32);
    private static readonly Color ColorPanelAccent = new(24, 24, 48);
    private static readonly Color ColorNeonCyan = new(0, 255, 255);
    private static readonly Color ColorNeonMagenta = new(255, 0, 255);
    private static readonly Color ColorNeonYellow = new(255, 255, 0);
    private static readonly Color ColorNeonGreen = new(50, 255, 50);
    private static readonly Color ColorNeonRed = new(255, 50, 50);
    private static readonly Color ColorText = new(220, 230, 255);
    private static readonly Color ColorTextDim = new(140, 150, 180);

    public void Update(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.AssetEditor)
        {
            return;
        }

        var editor = world.GetRequiredResource<AssetEditorResource>();
        if (editor.ActiveGroup == null)
        {
            appMode.Mode = AppMode.AssetGroupSelector;
            return;
        }

        var options = world.GetRequiredResource<OptionsResource>();
        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];
        
        var layout = BuildLayout(frameContext.Viewport.Width, frameContext.Viewport.Height, options.UiScale, selectedAsset.Resolution);

        if (editor.IsConfirmingModeSwitch)
        {
            if (IsNewKeyPress(frameContext, Keys.Y)) ConfirmModeSwitch(editor);
            if (IsNewKeyPress(frameContext, Keys.N) || IsNewKeyPress(frameContext, Keys.Escape)) editor.IsConfirmingModeSwitch = false;
            
            if (IsNewLeftClick(frameContext, out var mouse))
            {
                if (layout.ConfirmYesButton.Contains(mouse)) ConfirmModeSwitch(editor);
                if (layout.ConfirmNoButton.Contains(mouse)) editor.IsConfirmingModeSwitch = false;
            }
            return;
        }

        if (IsNewKeyPress(frameContext, Keys.Escape))
        {
            editor.LastStrokePoint = null;
            appMode.Mode = AppMode.AssetGroupSelector;
            return;
        }

        if (IsNewKeyPress(frameContext, Keys.Enter))
        {
            Save(world);
        }

        HandleKeyboardSelection(frameContext, editor);

        if (IsNewLeftClick(frameContext, out var clickPosition))
        {
            if (layout.SaveButton.Contains(clickPosition))
            {
                Save(world);
            }
            else if (layout.DiscardButton.Contains(clickPosition))
            {
                appMode.Mode = AppMode.AssetGroupSelector;
                _assetThumbnails.Clear();
            }
            else
            {
                HandlePanelClick(world, editor, layout, clickPosition);
            }
        }

        HandlePaint(editor, frameContext, layout);
    }

    private static void Save(World world)
    {
        var lib = world.GetRequiredResource<AssetLibraryResource>();
        var editor = world.GetRequiredResource<AssetEditorResource>();
        if (editor.ActiveGroup != null)
        {
            editor.ActiveGroup.IsDone = CheckIfGroupDone(editor.ActiveGroup);
            AssetEditorStorage.SaveLibrary(AssetPaths.DefaultAssets, lib);
            editor.Dirty = false;
        }
    }

    private static bool CheckIfGroupDone(AssetGroup group)
    {
        foreach (var asset in group.Assets)
        {
            foreach (var frame in asset.Frames)
            {
                if (frame.Any(c => c.A > 0)) return true; // At least one pixel in the whole group is enough for lab status? 
                // Better: check if every asset has at least one frame drawn
            }
        }
        return false;
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch is null || frameContext.DebugPixel is null)
        {
            return;
        }

        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.AssetEditor)
        {
            return;
        }

        var editor = world.GetRequiredResource<AssetEditorResource>();
        if (editor.ActiveGroup == null) return;

        var options = world.GetRequiredResource<OptionsResource>();
        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];
        var sb = frameContext.SpriteBatch;
        var pixel = frameContext.DebugPixel;

        var layout = BuildLayout(frameContext.Viewport.Width, frameContext.Viewport.Height, options.UiScale, selectedAsset.Resolution);

        // Background
        sb.Draw(pixel, new Rectangle(0, 0, frameContext.Viewport.Width, frameContext.Viewport.Height), ColorBg);

        // Header
        PixelText.Draw(sb, pixel, $"EDITING: {editor.ActiveGroup.Name.ToUpper()}", new Vector2(20, 15), (int)Math.Max(1, 3 * options.UiScale), ColorNeonCyan);

        // Draw Panels
        DrawPanel(sb, pixel, layout.LeftPanel, "ASSETS", ColorNeonCyan, layout.Scale);
        DrawPanel(sb, pixel, layout.CanvasPanel, "CANVAS", ColorNeonMagenta, layout.Scale);
        DrawPanel(sb, pixel, layout.ActionPanel, "ACTIONS", ColorNeonGreen, layout.Scale);
        DrawPanel(sb, pixel, layout.RightPanel, "TOOLS", ColorNeonYellow, layout.Scale);
        DrawPanel(sb, pixel, layout.BottomPanel, "FRAMES", ColorText, layout.Scale);

        // Canvas Area
        bool showGrid = selectedAsset.Mode == AssetCanvasMode.Pixel;
        DrawChecker(frameContext, layout.CanvasRect, layout.CellSize, selectedAsset.Resolution, showGrid, editor.BackgroundColor);
        DrawActiveFrame(editor, frameContext, layout.CanvasRect);
        
        // Palette
        DrawPalette(editor, frameContext, layout);
        
        // Asset List
        DrawAssetList(editor, frameContext, layout);
        
        // Frame Strip
        DrawFrameStrip(editor, frameContext, layout);
        
        // Controls
        DrawControls(world, editor, frameContext, layout);
        
        // Overlay for Confirmation
        if (editor.IsConfirmingModeSwitch)
        {
            sb.Draw(pixel, new Rectangle(0, 0, frameContext.Viewport.Width, frameContext.Viewport.Height), new Color(0, 0, 0, 180));
            sb.Draw(pixel, layout.ConfirmPanel, ColorPanel);
            sb.Draw(pixel, new Rectangle(layout.ConfirmPanel.X, layout.ConfirmPanel.Y, layout.ConfirmPanel.Width, 2), ColorNeonRed);
            
            var msg = $"SWITCH TO {editor.PendingCanvasMode.ToString().ToUpper()} MODE?";
            var subMsg = "THIS WILL CLEAR ALL FRAMES!";
            var mSize = PixelText.Measure(msg, (int)Math.Max(1, 2 * layout.Scale));
            var smSize = PixelText.Measure(subMsg, (int)Math.Max(1, 1 * layout.Scale));
            
            PixelText.Draw(sb, pixel, msg, new Vector2(layout.ConfirmPanel.Center.X - mSize.X/2, layout.ConfirmPanel.Y + (int)(60 * layout.Scale)), (int)Math.Max(1, 2 * layout.Scale), ColorText);
            PixelText.Draw(sb, pixel, subMsg, new Vector2(layout.ConfirmPanel.Center.X - smSize.X/2, layout.ConfirmPanel.Y + (int)(110 * layout.Scale)), (int)Math.Max(1, 1 * layout.Scale), ColorNeonRed);

            DrawButton(sb, pixel, layout.ConfirmYesButton, "YES (Y)", false, ColorNeonRed, layout.Scale);
            DrawButton(sb, pixel, layout.ConfirmNoButton, "NO (N)", false, ColorNeonCyan, layout.Scale);
        }

        // Footer Hints
        var hint = "ENTER:SAVE  ESC:BACK  [ ]:ASSET  +/-:FRAME  1-9:COLOR  F:FREE  P:PIXEL";
        PixelText.Draw(sb, pixel, hint, new Vector2(20, frameContext.Viewport.Height - 35), (int)Math.Max(1, 1 * layout.Scale), ColorTextDim);
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
        PixelText.Draw(sb, pixel, title, new Vector2(rect.X + 8, rect.Y - (int)(16 * scale)), (int)Math.Max(1, 1 * scale), accent);
    }

    private static void HandleKeyboardSelection(FrameContext frameContext, AssetEditorResource editor)
    {
        if (editor.ActiveGroup == null) return;

        for (var i = 0; i < editor.Palette.Length && i < 9; i++)
        {
            var key = (Keys)((int)Keys.D1 + i);
            if (IsNewKeyPress(frameContext, key))
            {
                editor.SelectedColorIndex = i;
            }
        }

        if (IsNewKeyPress(frameContext, Keys.OemOpenBrackets))
        {
            editor.SelectedAssetIndex = (editor.SelectedAssetIndex + editor.ActiveGroup.Assets.Count - 1) % editor.ActiveGroup.Assets.Count;
            editor.SelectedFrameIndex = 0;
            editor.LastStrokePoint = null;
        }

        if (IsNewKeyPress(frameContext, Keys.OemCloseBrackets))
        {
            editor.SelectedAssetIndex = (editor.SelectedAssetIndex + 1) % editor.ActiveGroup.Assets.Count;
            editor.SelectedFrameIndex = 0;
            editor.LastStrokePoint = null;
        }

        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];
        var frameCount = selectedAsset.Frames.Count;

        if (IsNewKeyPress(frameContext, Keys.OemMinus))
        {
            editor.SelectedFrameIndex = (editor.SelectedFrameIndex + frameCount - 1) % frameCount;
            editor.LastStrokePoint = null;
        }

        if (IsNewKeyPress(frameContext, Keys.OemPlus) || IsNewKeyPress(frameContext, Keys.Add))
        {
            editor.SelectedFrameIndex = (editor.SelectedFrameIndex + 1) % frameCount;
            editor.LastStrokePoint = null;
        }

        if (IsNewKeyPress(frameContext, Keys.F))
        {
            if (selectedAsset.Mode != AssetCanvasMode.Free)
            {
                editor.IsConfirmingModeSwitch = true;
                editor.PendingCanvasMode = AssetCanvasMode.Free;
            }
        }

        if (IsNewKeyPress(frameContext, Keys.P))
        {
            if (selectedAsset.Mode != AssetCanvasMode.Pixel)
            {
                editor.IsConfirmingModeSwitch = true;
                editor.PendingCanvasMode = AssetCanvasMode.Pixel;
            }
        }
    }

    private static void HandlePanelClick(World world, AssetEditorResource editor, EditorLayout layout, Point mouse)
    {
        if (editor.ActiveGroup == null) return;

        // Palette
        for (var i = 0; i < editor.Palette.Length; i++)
        {
            if (GetPaletteRect(layout, i).Contains(mouse))
            {
                editor.SelectedColorIndex = i;
                editor.IsUsingCustomColor = false;
                return;
            }
        }

        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];

        // Tools
        if (layout.ToolBrushButton.Contains(mouse)) { editor.SelectedTool = AssetEditorTool.Brush; return; }
        if (layout.ToolBucketButton.Contains(mouse)) { editor.SelectedTool = AssetEditorTool.Bucket; return; }

        // Custom Color
        if (layout.CustomColorButton.Contains(mouse))
        {
            editor.IsUsingCustomColor = true;
            // Cycle through some colors for "custom" in this lab
            var colors = new[] { Color.White, Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Magenta, Color.Cyan, Color.Orange, Color.Purple };
            var idx = Array.IndexOf(colors, editor.CustomColor);
            editor.CustomColor = colors[(idx + 1) % colors.Length];
            return;
        }

        // Bg Color
        if (layout.BgColorButton.Contains(mouse))
        {
            var bgs = new[] { Color.Transparent, Color.Black, Color.White, new Color(100, 100, 100), new Color(0, 50, 0), new Color(50, 0, 0) };
            var idx = Array.IndexOf(bgs, editor.BackgroundColor);
            editor.BackgroundColor = bgs[(idx + 1) % bgs.Length];
            return;
        }

        // Transfer
        if (layout.TransferButton.Contains(mouse))
        {
            if (world.TryGetResource<NetworkResource>(out var net) && net is { IsConnected: true })
            {
                // Simulated Transfer logic
                // In a real scenario, this would serialize the group and send it over the wire.
                // For now, we simulate changing the owner.
                Console.WriteLine($"[Network] Transferring asset group '{editor.ActiveGroup.Name}' to peer...");
                editor.ActiveGroup.OwnerId = Guid.NewGuid(); // Simulated peer ID
                foreach (var asset in editor.ActiveGroup.Assets)
                {
                    asset.OwnerId = editor.ActiveGroup.OwnerId;
                }
                editor.Dirty = true;
                Console.WriteLine("[Network] Transfer complete.");
            }
            else
            {
                Console.WriteLine("[Network] Cannot transfer: Not connected to any peer.");
            }
            return;
        }

        // Mode Buttons
        if (layout.ModePixelButton.Contains(mouse))
        {
            if (selectedAsset.Mode != AssetCanvasMode.Pixel)
            {
                editor.IsConfirmingModeSwitch = true;
                editor.PendingCanvasMode = AssetCanvasMode.Pixel;
            }
            return;
        }
        if (layout.ModeFreeButton.Contains(mouse))
        {
            if (selectedAsset.Mode != AssetCanvasMode.Free)
            {
                editor.IsConfirmingModeSwitch = true;
                editor.PendingCanvasMode = AssetCanvasMode.Free;
            }
            return;
        }

        // Resolution Buttons
        if (selectedAsset.Mode == AssetCanvasMode.Pixel)
        {
            if (layout.Res16Button.Contains(mouse)) ResizeAsset(selectedAsset, 16);
            if (layout.Res32Button.Contains(mouse)) ResizeAsset(selectedAsset, 32);
            if (layout.Res64Button.Contains(mouse)) ResizeAsset(selectedAsset, 64);
        }
        else
        {
            if (layout.Res256Button.Contains(mouse)) ResizeAsset(selectedAsset, 256);
            if (layout.Res512Button.Contains(mouse)) ResizeAsset(selectedAsset, 512);
        }

        // Brush Buttons
        if (layout.BrushMinusButton.Contains(mouse))
        {
            editor.BrushSize = Math.Max(MinBrushSize, editor.BrushSize - 1);
            return;
        }
        if (layout.BrushPlusButton.Contains(mouse))
        {
            editor.BrushSize = Math.Min(MaxBrushSize, editor.BrushSize + 1);
            return;
        }

        // Asset List
        for (var i = 0; i < editor.ActiveGroup.Assets.Count; i++)
        {
            if (GetAssetRowRect(layout, i).Contains(mouse))
            {
                editor.SelectedAssetIndex = i;
                editor.SelectedFrameIndex = 0;
                editor.LastStrokePoint = null;
                return;
            }
        }

        // Frame Strip
        for (var i = 0; i < selectedAsset.Frames.Count; i++)
        {
            if (GetFrameRect(layout, i, selectedAsset.Frames.Count).Contains(mouse))
            {
                editor.SelectedFrameIndex = i;
                editor.LastStrokePoint = null;
                return;
            }
        }
    }

    private static void ConfirmModeSwitch(AssetEditorResource editor)
    {
        if (editor.ActiveGroup == null) return;
        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];
        selectedAsset.Mode = editor.PendingCanvasMode;
        
        int targetRes = (selectedAsset.Mode == AssetCanvasMode.Pixel) ? 32 : 256;
        
        ResizeAsset(selectedAsset, targetRes);
        
        editor.IsConfirmingModeSwitch = false;
        editor.Dirty = true;
    }

    private static void ResizeAsset(EditableAsset asset, int newRes)
    {
        asset.Resolution = newRes;
        asset.Width = newRes;
        asset.Height = newRes;
        for (int i = 0; i < asset.Frames.Count; i++)
        {
            asset.Frames[i] = new Color[newRes * newRes];
            Array.Fill(asset.Frames[i], Color.Transparent);
        }
    }

    private static void HandlePaint(AssetEditorResource editor, FrameContext frameContext, EditorLayout layout)
    {
        if (editor.ActiveGroup == null) return;

        var leftDown = frameContext.CurrentMouse.LeftButton == ButtonState.Pressed;
        var rightDown = frameContext.CurrentMouse.RightButton == ButtonState.Pressed;

        if (!leftDown && !rightDown)
        {
            editor.LastStrokePoint = null;
            return;
        }

        if (!layout.CanvasRect.Contains(frameContext.CurrentMouse.Position))
        {
            editor.LastStrokePoint = null;
            return;
        }

        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];
        var frame = selectedAsset.Frames[editor.SelectedFrameIndex];
        var color = leftDown ? editor.GetActiveColor() : Color.Transparent;

        if (selectedAsset.Mode == AssetCanvasMode.Pixel)
        {
            if (TryGetCanvasPixel(frameContext.CurrentMouse.Position, layout, selectedAsset, out var pixel))
            {
                if (editor.SelectedTool == AssetEditorTool.Bucket && leftDown)
                {
                    if (IsNewLeftClick(frameContext, out _))
                    {
                        FloodFill(frame, selectedAsset.Width, selectedAsset.Height, pixel, color);
                        editor.Dirty = true;
                    }
                }
                else
                {
                    PaintBrush(frame, selectedAsset.Width, selectedAsset.Height, pixel, editor.BrushSize, color);
                    editor.Dirty = true;
                }
            }
            editor.LastStrokePoint = null;
        }
        else
        {
            if (TryGetCanvasLocal(frameContext.CurrentMouse.Position, layout, selectedAsset, out var currentPoint))
            {
                if (editor.SelectedTool == AssetEditorTool.Bucket && leftDown)
                {
                     if (IsNewLeftClick(frameContext, out _))
                     {
                        Point p = new((int)currentPoint.X, (int)currentPoint.Y);
                        FloodFill(frame, selectedAsset.Width, selectedAsset.Height, p, color);
                        editor.Dirty = true;
                     }
                }
                else
                {
                    if (editor.LastStrokePoint is Vector2 last)
                    {
                        PaintLineContinuous(frame, selectedAsset.Width, selectedAsset.Height, last, currentPoint, editor.BrushSize, color);
                    }
                    else
                    {
                        PaintBrushContinuous(frame, selectedAsset.Width, selectedAsset.Height, currentPoint, editor.BrushSize, color);
                    }
                    editor.Dirty = true;
                }
                editor.LastStrokePoint = currentPoint;
            }
        }
    }

    private static void FloodFill(Color[] frame, int width, int height, Point start, Color newColor)
    {
        Color targetColor = frame[start.Y * width + start.X];
        if (targetColor == newColor) return;

        var queue = new Queue<Point>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            if (frame[p.Y * width + p.X] != targetColor) continue;

            frame[p.Y * width + p.X] = newColor;

            if (p.X > 0) queue.Enqueue(new Point(p.X - 1, p.Y));
            if (p.X < width - 1) queue.Enqueue(new Point(p.X + 1, p.Y));
            if (p.Y > 0) queue.Enqueue(new Point(p.X, p.Y - 1));
            if (p.Y < height - 1) queue.Enqueue(new Point(p.X, p.Y + 1));
        }
    }

    private void DrawActiveFrame(AssetEditorResource editor, FrameContext frameContext, Rectangle canvasRect)
    {
        if (editor.ActiveGroup == null) return;
        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];
        var frame = selectedAsset.Frames[editor.SelectedFrameIndex];
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;

        if (selectedAsset.Mode == AssetCanvasMode.Pixel)
        {
            for (var y = 0; y < selectedAsset.Height; y++)
            {
                for (var x = 0; x < selectedAsset.Width; x++)
                {
                    var color = frame[(y * selectedAsset.Width) + x];
                    if (color.A == 0) continue;

                    var rect = new Rectangle(
                        canvasRect.X + x * canvasRect.Width / selectedAsset.Width,
                        canvasRect.Y + y * canvasRect.Height / selectedAsset.Height,
                        canvasRect.Width / selectedAsset.Width,
                        canvasRect.Height / selectedAsset.Height
                    );
                    sb.Draw(pixel, rect, color);
                }
            }
        }
        else
        {
            var gd = sb.GraphicsDevice;
            if (_canvasTexture == null || _textureRes != selectedAsset.Resolution)
            {
                _canvasTexture?.Dispose();
                _canvasTexture = new Texture2D(gd, selectedAsset.Resolution, selectedAsset.Resolution);
                _textureRes = selectedAsset.Resolution;
            }

            _canvasTexture.SetData(frame);
            sb.Draw(_canvasTexture, canvasRect, Color.White);
        }
    }

    private static void DrawChecker(FrameContext frameContext, Rectangle canvasRect, int cellSize, int resolution, bool showGrid, Color backgroundColor)
    {
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        
        if (backgroundColor.A > 0)
        {
            sb.Draw(pixel, canvasRect, backgroundColor);
        }

        int bigChecker = Math.Max(2, cellSize * Math.Max(1, resolution / 8));
        for (var y = 0; y < canvasRect.Height / bigChecker + 1; y++)
        {
            for (var x = 0; x < canvasRect.Width / bigChecker + 1; x++)
            {
                var isEven = ((x + y) % 2) == 0;
                var color = isEven ? new Color(24, 24, 36, 100) : new Color(32, 32, 44, 100);
                if (backgroundColor.A > 0)
                {
                    color = isEven ? new Color(0,0,0, 20) : new Color(255,255,255, 20);
                }

                var r = new Rectangle(canvasRect.X + x * bigChecker, canvasRect.Y + y * bigChecker, bigChecker, bigChecker);
                
                int intersectX = Math.Max(r.X, canvasRect.X);
                int intersectY = Math.Max(r.Y, canvasRect.Y);
                int intersectW = Math.Min(r.Right, canvasRect.Right) - intersectX;
                int intersectH = Math.Min(r.Bottom, canvasRect.Bottom) - intersectY;
                
                if (intersectW > 0 && intersectH > 0)
                    sb.Draw(pixel, new Rectangle(intersectX, intersectY, intersectW, intersectH), color);
            }
        }

        if (showGrid)
        {
            var gridColor = new Color(255, 255, 255, 15);
            for (var i = 0; i <= resolution; i++)
            {
                int x = canvasRect.X + (i * canvasRect.Width / resolution);
                if (x >= canvasRect.X && x <= canvasRect.Right)
                    sb.Draw(pixel, new Rectangle(x, canvasRect.Y, 1, canvasRect.Height), gridColor);
                
                int y = canvasRect.Y + (i * canvasRect.Height / resolution);
                if (y >= canvasRect.Y && y <= canvasRect.Bottom)
                    sb.Draw(pixel, new Rectangle(canvasRect.X, y, canvasRect.Width, 1), gridColor);
            }
        }
    }

    private static void DrawPalette(AssetEditorResource editor, FrameContext frameContext, EditorLayout layout)
    {
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;

        for (var i = 0; i < editor.Palette.Length; i++)
        {
            var rect = GetPaletteRect(layout, i);
            var color = editor.Palette[i];
            
            if (color.A == 0)
            {
                sb.Draw(pixel, rect, new Color(40, 40, 40));
                sb.Draw(pixel, new Rectangle(rect.X + 2, rect.Y + rect.Height/2 - 1, rect.Width - 4, 2), Color.Red);
            }
            else
            {
                sb.Draw(pixel, rect, color);
            }

            if (i == editor.SelectedColorIndex && !editor.IsUsingCustomColor)
            {
                var border = (int)Math.Max(1, 4 * layout.Scale);
                sb.Draw(pixel, new Rectangle(rect.X - border, rect.Y - border, rect.Width + border * 2, border), Color.White);
                sb.Draw(pixel, new Rectangle(rect.X - border, rect.Bottom, rect.Width + border * 2, border), Color.White);
                sb.Draw(pixel, new Rectangle(rect.X - border, rect.Y - border, border, rect.Height + border * 2), Color.White);
                sb.Draw(pixel, new Rectangle(rect.Right, rect.Y - border, border, rect.Height + border * 2), Color.White);
            }
        }
    }

    private void DrawAssetList(AssetEditorResource editor, FrameContext frameContext, EditorLayout layout)
    {
        if (editor.ActiveGroup == null) return;

        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var gd = sb.GraphicsDevice;
        var scale = layout.Scale;

        for (var i = 0; i < editor.ActiveGroup.Assets.Count; i++)
        {
            var rect = GetAssetRowRect(layout, i);
            var active = i == editor.SelectedAssetIndex;
            var asset = editor.ActiveGroup.Assets[i];
            
            sb.Draw(pixel, rect, active ? ColorPanelAccent : ColorPanel);
            if (active)
            {
                sb.Draw(pixel, new Rectangle(rect.X, rect.Y, (int)Math.Max(1, 6 * scale), rect.Height), ColorNeonCyan);
            }

            // Thumbnail
            var thumbRect = new Rectangle(rect.X + (int)(10 * scale), rect.Y + (int)(10 * scale), rect.Height - (int)(20 * scale), rect.Height - (int)(20 * scale));
            sb.Draw(pixel, thumbRect, Color.Black);
            
            if (asset.Frames.Count > 0)
            {
                string key = $"{editor.ActiveGroup.Name}_{i}";
                if (!_assetThumbnails.TryGetValue(key, out var tex) || tex.Width != asset.Resolution)
                {
                    tex?.Dispose();
                    tex = new Texture2D(gd, asset.Resolution, asset.Resolution);
                    tex.SetData(asset.Frames[0]);
                    _assetThumbnails[key] = tex;
                }
                else if (active && editor.Dirty) 
                {
                    tex.SetData(asset.Frames[0]);
                }
                sb.Draw(tex, thumbRect, Color.White);
            }

            PixelText.Draw(sb, pixel, asset.Name, new Vector2(thumbRect.Right + (int)(15 * scale), rect.Y + (int)(15 * scale)), (int)Math.Max(1, 2 * scale), active ? ColorNeonCyan : ColorText);
            
            var info = $"{asset.Resolution}X{asset.Resolution}";
            PixelText.Draw(sb, pixel, info, new Vector2(thumbRect.Right + (int)(15 * scale), rect.Y + (int)(40 * scale)), (int)Math.Max(1, 1 * scale), ColorTextDim);
        }
    }

    private static void DrawFrameStrip(AssetEditorResource editor, FrameContext frameContext, EditorLayout layout)
    {
        if (editor.ActiveGroup == null) return;
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];

        for (var i = 0; i < selectedAsset.Frames.Count; i++)
        {
            var rect = GetFrameRect(layout, i, selectedAsset.Frames.Count);
            var active = i == editor.SelectedFrameIndex;

            sb.Draw(pixel, rect, ColorPanelAccent);
            DrawFramePreview(frameContext, selectedAsset, i, rect);

            if (active)
            {
                var border = (int)Math.Max(1, 4 * layout.Scale);
                sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, border), Color.White);
                sb.Draw(pixel, new Rectangle(rect.X, rect.Bottom - border, rect.Width, border), Color.White);
                sb.Draw(pixel, new Rectangle(rect.X, rect.Y, border, rect.Height), Color.White);
                sb.Draw(pixel, new Rectangle(rect.Right - border, rect.Y, border, rect.Height), Color.White);
            }
        }
    }

    private static void DrawFramePreview(FrameContext frameContext, EditableAsset asset, int frameIndex, Rectangle rect)
    {
        var frame = asset.Frames[frameIndex];
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        
        var margin = 6;
        var availableW = rect.Width - margin * 2;
        var availableH = rect.Height - margin * 2;
        var pSize = Math.Max(1, Math.Min(availableW / asset.Width, availableH / asset.Height));
        
        var startX = rect.X + margin + (availableW - asset.Width * pSize) / 2;
        var startY = rect.Y + margin + (availableH - asset.Height * pSize) / 2;

        for (var y = 0; y < asset.Height; y++)
        {
            for (var x = 0; x < asset.Width; x++)
            {
                var color = frame[(y * asset.Width) + x];
                if (color.A == 0) continue;
                sb.Draw(pixel, new Rectangle(startX + x * pSize, startY + y * pSize, pSize, pSize), color);
            }
        }
    }

    private static void DrawControls(World world, AssetEditorResource editor, FrameContext frameContext, EditorLayout layout)
    {
        if (editor.ActiveGroup == null) return;
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var scale = layout.Scale;
        var selectedAsset = editor.ActiveGroup.Assets[editor.SelectedAssetIndex];

        // Mode Buttons
        PixelText.Draw(sb, pixel, "MODE", new Vector2(layout.ModePixelButton.X, layout.ModePixelButton.Y - (int)(25 * scale)), (int)Math.Max(1, 2 * scale), ColorText);
        DrawButton(sb, pixel, layout.ModePixelButton, "PIXEL", selectedAsset.Mode == AssetCanvasMode.Pixel, ColorNeonCyan, scale);
        DrawButton(sb, pixel, layout.ModeFreeButton, "FREE", selectedAsset.Mode == AssetCanvasMode.Free, ColorNeonMagenta, scale);

        // Resolution
        PixelText.Draw(sb, pixel, "RES", new Vector2(layout.Res16Button.X, layout.Res16Button.Y - (int)(25 * scale)), (int)Math.Max(1, 2 * scale), ColorText);
        if (selectedAsset.Mode == AssetCanvasMode.Pixel)
        {
            DrawButton(sb, pixel, layout.Res16Button, "16", selectedAsset.Resolution == 16, ColorNeonCyan, scale);
            DrawButton(sb, pixel, layout.Res32Button, "32", selectedAsset.Resolution == 32, ColorNeonCyan, scale);
            DrawButton(sb, pixel, layout.Res64Button, "64", selectedAsset.Resolution == 64, ColorNeonCyan, scale);
        }
        else
        {
            DrawButton(sb, pixel, layout.Res256Button, "256", selectedAsset.Resolution == 256, ColorNeonMagenta, scale);
            DrawButton(sb, pixel, layout.Res512Button, "512", selectedAsset.Resolution == 512, ColorNeonMagenta, scale);
        }

        // Brush Size
        PixelText.Draw(sb, pixel, "BRUSH SIZE", new Vector2(layout.BrushMinusButton.X, layout.BrushMinusButton.Y - (int)(30 * scale)), (int)Math.Max(1, 2 * scale), ColorText);
        DrawButton(sb, pixel, layout.BrushMinusButton, "-", false, ColorText, scale);
        DrawButton(sb, pixel, layout.BrushPlusButton, "+", false, ColorText, scale);
        
        var sizeText = editor.BrushSize.ToString();
        var sizePos = new Vector2(layout.BrushMinusButton.Right + (int)(20 * scale), layout.BrushMinusButton.Y + (int)(15 * scale));
        PixelText.Draw(sb, pixel, sizeText, sizePos, (int)Math.Max(1, 3 * scale), ColorNeonYellow);

        // Tool Selection
        PixelText.Draw(sb, pixel, "TOOL", new Vector2(layout.ToolBrushButton.X, layout.ToolBrushButton.Y - (int)(25 * scale)), (int)Math.Max(1, 2 * scale), ColorText);
        DrawButton(sb, pixel, layout.ToolBrushButton, "BRUSH", editor.SelectedTool == AssetEditorTool.Brush, ColorNeonCyan, scale);
        DrawButton(sb, pixel, layout.ToolBucketButton, "BUCKET", editor.SelectedTool == AssetEditorTool.Bucket, ColorNeonCyan, scale);

        // Custom/Bg Color
        PixelText.Draw(sb, pixel, "COLOR", new Vector2(layout.CustomColorButton.X, layout.CustomColorButton.Y - (int)(25 * scale)), (int)Math.Max(1, 2 * scale), ColorText);
        DrawButton(sb, pixel, layout.CustomColorButton, "PICK", editor.IsUsingCustomColor, editor.CustomColor, scale);
        DrawButton(sb, pixel, layout.BgColorButton, "BG", editor.BackgroundColor.A > 0, editor.BackgroundColor, scale);

        // Social
        PixelText.Draw(sb, pixel, "SOCIAL", new Vector2(layout.TransferButton.X, layout.TransferButton.Y - (int)(25 * scale)), (int)Math.Max(1, 2 * scale), ColorText);
        bool canTransfer = world.TryGetResource<NetworkResource>(out var net) && net is { IsConnected: true };
        DrawButton(sb, pixel, layout.TransferButton, "TRANSFER TO PEER", false, canTransfer ? ColorNeonMagenta : Color.DarkSlateGray, scale);

        // Save / Discard
        DrawButton(sb, pixel, layout.SaveButton, "SAVE", false, ColorNeonGreen, scale);
        DrawButton(sb, pixel, layout.DiscardButton, "BACK", false, ColorNeonRed, scale);
    }

    private static void DrawButton(SpriteBatch sb, Texture2D pixel, Rectangle rect, string text, bool active, Color color, float scale)
    {
        sb.Draw(pixel, rect, active ? color : ColorPanelAccent);
        var tScale = (int)Math.Max(1, 2 * scale);
        var tSize = PixelText.Measure(text, tScale);
        var tPos = new Vector2(rect.Center.X - tSize.X / 2, rect.Center.Y - tSize.Y / 2);
        PixelText.Draw(sb, pixel, text, tPos, tScale, active ? Color.Black : ColorText);
    }

    private static bool TryGetCanvasPixel(Point mouse, EditorLayout layout, EditableAsset asset, out Point pixel)
    {
        pixel = default;
        if (!layout.CanvasRect.Contains(mouse)) return false;
        
        var px = (mouse.X - layout.CanvasRect.X) * asset.Width / layout.CanvasRect.Width;
        var py = (mouse.Y - layout.CanvasRect.Y) * asset.Height / layout.CanvasRect.Height;
        
        if (px < 0 || py < 0 || px >= asset.Width || py >= asset.Height) return false;
        
        pixel = new Point(px, py);
        return true;
    }

    private static bool TryGetCanvasLocal(Point mouse, EditorLayout layout, EditableAsset asset, out Vector2 local)
    {
        local = default;
        if (!layout.CanvasRect.Contains(mouse)) return false;

        var fx = (mouse.X - layout.CanvasRect.X) * (float)asset.Width / layout.CanvasRect.Width;
        var fy = (mouse.Y - layout.CanvasRect.Y) * (float)asset.Height / layout.CanvasRect.Height;
        
        if (fx < 0f || fy < 0f || fx >= asset.Width || fy >= asset.Height) return false;
        
        local = new Vector2(fx, fy);
        return true;
    }

    private static void PaintLineContinuous(Color[] frame, int width, int height, Vector2 from, Vector2 to, int brushSize, Color color)
    {
        var diff = to - from;
        var len = diff.Length();
        var steps = Math.Max(1, (int)(len * 5f));

        for (var i = 0; i <= steps; i++)
        {
            var t = i / (float)steps;
            var point = Vector2.Lerp(from, to, t);
            PaintBrushContinuous(frame, width, height, point, brushSize, color);
        }
    }

    private static void PaintBrushContinuous(Color[] frame, int width, int height, Vector2 center, int brushSize, Color color)
    {
        var radius = brushSize * 0.5f;
        var rSq = radius * radius;
        
        int minX = (int)Math.Max(0, Math.Floor(center.X - radius));
        int maxX = (int)Math.Min(width - 1, Math.Ceiling(center.X + radius));
        int minY = (int)Math.Max(0, Math.Floor(center.Y - radius));
        int maxY = (int)Math.Min(height - 1, Math.Ceiling(center.Y + radius));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                var dx = (x + 0.5f) - center.X;
                var dy = (y + 0.5f) - center.Y;
                if (dx * dx + dy * dy <= rSq + 0.25f)
                {
                    frame[y * width + x] = color;
                }
            }
        }
    }

    private static void PaintBrush(Color[] frame, int width, int height, Point center, int brushSize, Color color)
    {
        var radius = brushSize / 2;
        for (int y = -radius; y <= (brushSize % 2 == 0 ? radius - 1 : radius); y++)
        {
            for (int x = -radius; x <= (brushSize % 2 == 0 ? radius - 1 : radius); x++)
            {
                var px = center.X + x;
                var py = center.Y + y;
                if (px >= 0 && px < width && py >= 0 && py < height)
                {
                    frame[py * width + px] = color;
                }
            }
        }
    }

    private static Rectangle GetPaletteRect(EditorLayout layout, int index)
    {
        var scale = layout.Scale;
        var margin = (int)(25 * scale);
        var size = (int)(50 * scale);
        var gap = (int)(15 * scale);
        var col = index % 3;
        var row = index / 3;
        return new Rectangle(layout.RightPanel.X + margin + col * (size + gap), layout.RightPanel.Y + (int)(60 * scale) + row * (size + gap), size, size);
    }

    private static Rectangle GetAssetRowRect(EditorLayout layout, int index)
    {
        var scale = layout.Scale;
        var h = (int)(80 * scale);
        var margin = (int)(12 * scale);
        var gap = (int)(8 * scale);
        return new Rectangle(layout.LeftPanel.X + margin, layout.LeftPanel.Y + (int)(80 * scale) + index * (h + gap), layout.LeftPanel.Width - margin * 2, h);
    }

    private static Rectangle GetFrameRect(EditorLayout layout, int index, int frameCount)
    {
        var scale = layout.Scale;
        var size = (int)(100 * scale);
        var gap = (int)(20 * scale);
        return new Rectangle(layout.BottomPanel.X + (int)(30 * scale) + index * (size + gap), layout.BottomPanel.Y + (int)(50 * scale), size, size);
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

    private static EditorLayout BuildLayout(int w, int h, float userScale, int resolution)
    {
        float autoScale = Math.Max(1.0f, Math.Min(w / 1024f, h / 768f));
        float scale = userScale * autoScale;

        var leftW = (int)(320 * scale);
        var rightW = (int)(340 * scale);
        var actionH = (int)(100 * scale);
        var bottomH = (int)(180 * scale);
        var headerH = (int)(80 * scale);
        var margin = (int)(20 * scale);

        var leftPanel = new Rectangle(margin, headerH, leftW, h - headerH - margin);
        var actionPanel = new Rectangle(w - rightW - margin, headerH, rightW, actionH);
        var rightPanel = new Rectangle(w - rightW - margin, actionPanel.Bottom + margin, rightW, h - actionPanel.Bottom - margin);
        
        var canvasAreaX = leftPanel.Right + margin;
        var canvasAreaW = w - leftW - rightW - margin * 4;
        var canvasPanel = new Rectangle(canvasAreaX, headerH, canvasAreaW, h - bottomH - headerH - margin * 2);
        var bottomPanel = new Rectangle(canvasAreaX, h - bottomH - margin, canvasAreaW, bottomH);

        var availableCanvasW = canvasPanel.Width - margin * 2;
        var availableCanvasH = canvasPanel.Height - margin * 2;
        var cellSize = Math.Max(1, Math.Min(availableCanvasW, availableCanvasH) / resolution);
        var canvasSize = cellSize * resolution;

        var canvasX = canvasPanel.X + (canvasPanel.Width - canvasSize) / 2;
        var canvasY = canvasPanel.Y + (canvasPanel.Height - canvasSize) / 2;

        var btnW = (rightW - margin * 3) / 2;
        var btnH = (int)(54 * scale);
        var toolStartX = rightPanel.X + margin;

        var confirmW = (int)(600 * scale);
        var confirmH = (int)(300 * scale);

        return new EditorLayout
        {
            Scale = scale,
            LeftPanel = leftPanel,
            RightPanel = rightPanel,
            BottomPanel = bottomPanel,
            CanvasPanel = canvasPanel,
            ActionPanel = actionPanel,
            CanvasRect = new Rectangle(canvasX, canvasY, canvasSize, canvasSize),
            CellSize = cellSize,
            
            ModePixelButton = new Rectangle(toolStartX, rightPanel.Y + (int)(250 * scale), btnW, btnH),
            ModeFreeButton = new Rectangle(toolStartX + btnW + margin, rightPanel.Y + (int)(250 * scale), btnW, btnH),
            
            Res16Button = new Rectangle(toolStartX, rightPanel.Y + (int)(360 * scale), (rightW - margin * 4) / 3, btnH),
            Res32Button = new Rectangle(toolStartX + ((rightW - margin * 4) / 3) + margin/2, rightPanel.Y + (int)(360 * scale), (rightW - margin * 4) / 3, btnH),
            Res64Button = new Rectangle(toolStartX + 2 * (((rightW - margin * 4) / 3) + margin/2), rightPanel.Y + (int)(360 * scale), (rightW - margin * 4) / 3, btnH),

            Res256Button = new Rectangle(toolStartX, rightPanel.Y + (int)(360 * scale), (rightW - margin * 3) / 2, btnH),
            Res512Button = new Rectangle(toolStartX + (rightW - margin * 3) / 2 + margin, rightPanel.Y + (int)(360 * scale), (rightW - margin * 3) / 2, btnH),

            BrushMinusButton = new Rectangle(toolStartX, rightPanel.Y + (int)(460 * scale), (int)(64 * scale), (int)(64 * scale)),
            BrushPlusButton = new Rectangle(toolStartX + (int)(150 * scale), rightPanel.Y + (int)(460 * scale), (int)(64 * scale), (int)(64 * scale)),

            ToolBrushButton = new Rectangle(toolStartX, rightPanel.Y + (int)(550 * scale), btnW, btnH),
            ToolBucketButton = new Rectangle(toolStartX + btnW + margin, rightPanel.Y + (int)(550 * scale), btnW, btnH),

            CustomColorButton = new Rectangle(toolStartX, rightPanel.Y + (int)(640 * scale), btnW, btnH),
            BgColorButton = new Rectangle(toolStartX + btnW + margin, rightPanel.Y + (int)(640 * scale), btnW, btnH),

            TransferButton = new Rectangle(toolStartX, rightPanel.Y + (int)(730 * scale), btnW * 2 + margin, btnH),

            SaveButton = new Rectangle(actionPanel.X + margin, actionPanel.Y + (int)(30 * scale), btnW, btnH),
            DiscardButton = new Rectangle(actionPanel.X + btnW + margin * 2, actionPanel.Y + (int)(30 * scale), btnW, btnH),

            ConfirmPanel = new Rectangle((w - confirmW) / 2, (h - confirmH) / 2, confirmW, confirmH),
            ConfirmYesButton = new Rectangle((w - confirmW) / 2 + (int)(100 * scale), (h - confirmH) / 2 + (int)(200 * scale), (int)(180 * scale), (int)(60 * scale)),
            ConfirmNoButton = new Rectangle((w - confirmW) / 2 + (int)(320 * scale), (h - confirmH) / 2 + (int)(200 * scale), (int)(180 * scale), (int)(60 * scale)),
        };
    }

    private sealed class EditorLayout
    {
        public float Scale { get; init; }
        public Rectangle LeftPanel { get; init; }
        public Rectangle RightPanel { get; init; }
        public Rectangle BottomPanel { get; init; }
        public Rectangle CanvasPanel { get; init; }
        public Rectangle ActionPanel { get; init; }
        public Rectangle CanvasRect { get; init; }
        public int CellSize { get; init; }
        
        public Rectangle ModePixelButton { get; init; }
        public Rectangle ModeFreeButton { get; init; }
        
        public Rectangle Res16Button { get; init; }
        public Rectangle Res32Button { get; init; }
        public Rectangle Res64Button { get; init; }
        public Rectangle Res256Button { get; init; }
        public Rectangle Res512Button { get; init; }

        public Rectangle BrushMinusButton { get; init; }
        public Rectangle BrushPlusButton { get; init; }

        public Rectangle ToolBrushButton { get; init; }
        public Rectangle ToolBucketButton { get; init; }
        public Rectangle CustomColorButton { get; init; }
        public Rectangle BgColorButton { get; init; }
        public Rectangle TransferButton { get; init; }

        public Rectangle SaveButton { get; init; }
        public Rectangle DiscardButton { get; init; }

        public Rectangle ConfirmPanel { get; init; }
        public Rectangle ConfirmYesButton { get; init; }
        public Rectangle ConfirmNoButton { get; init; }
    }

    private const int MinBrushSize = 1;
    private const int MaxBrushSize = 100;
}
