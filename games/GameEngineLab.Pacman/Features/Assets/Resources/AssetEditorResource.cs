using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Assets.Resources;

public enum AssetCanvasMode
{
    Pixel,
    Free,
}

public sealed class EditableAsset
{
    public string Name { get; set; } = string.Empty;

    public int Width { get; set; }

    public int Height { get; set; }

    public int Resolution { get; set; } = 32;

    public AssetCanvasMode Mode { get; set; } = AssetCanvasMode.Pixel;

    public List<Color[]> Frames { get; set; } = new();
}

public sealed class AssetGroup
{
    public string Name { get; set; } = "New Group";
    public List<EditableAsset> Assets { get; set; } = new();
    public bool IsDone { get; set; }
}

public sealed class AssetLibraryResource
{
    public List<AssetGroup> Groups { get; set; } = new();
    public int SelectedGroupIndex { get; set; } = -1;
}

public enum AssetEditorTool
{
    Brush,
    Bucket,
}

public sealed class AssetEditorResource
{
    // The active group being edited
    public AssetGroup? ActiveGroup { get; set; }

    public Color[] Palette { get; set; } = [];

    public int SelectedAssetIndex { get; set; }

    public int SelectedFrameIndex { get; set; }

    public int SelectedColorIndex { get; set; }

    public Color CustomColor { get; set; } = Color.White;

    public bool IsUsingCustomColor { get; set; }

    public Color BackgroundColor { get; set; } = Color.Transparent;

    public AssetEditorTool SelectedTool { get; set; } = AssetEditorTool.Brush;

    public int BrushSize { get; set; } = 1;

    public Vector2? LastStrokePoint { get; set; }

    public bool Dirty { get; set; }

    // Confirmation State
    public bool IsConfirmingModeSwitch { get; set; }
    public AssetCanvasMode PendingCanvasMode { get; set; }

    public Color GetActiveColor()
    {
        if (IsUsingCustomColor) return CustomColor;
        if (SelectedColorIndex >= 0 && SelectedColorIndex < Palette.Length) return Palette[SelectedColorIndex];
        return Color.White;
    }
}
