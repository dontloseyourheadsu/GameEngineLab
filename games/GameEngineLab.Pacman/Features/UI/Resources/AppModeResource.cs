namespace GameEngineLab.Pacman.Features.UI.Resources;

public enum AppMode
{
    Menu,
    Gameplay,
    MapEditor,
    AssetEditor,
    Options,
}

public sealed class AppModeResource
{
    public AppMode Mode { get; set; } = AppMode.Menu;
}
