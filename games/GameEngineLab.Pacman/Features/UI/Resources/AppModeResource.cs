namespace GameEngineLab.Pacman.Features.UI.Resources;

public enum AppMode
{
    Menu,
    Gameplay,
    GameSetup,
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
