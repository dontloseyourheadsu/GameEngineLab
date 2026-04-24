namespace GameEngineLab.Pacman.Features.UI.Resources;

public enum AppMode
{
    Menu,
    Gameplay,
    MapGroupSelector,
    MapEditor,
    AssetGroupSelector,
    AssetEditor,
    Options,
}

public sealed class AppModeResource
{
    public AppMode Mode { get; set; } = AppMode.Menu;
}
