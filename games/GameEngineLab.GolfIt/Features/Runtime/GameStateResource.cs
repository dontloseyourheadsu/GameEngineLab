namespace GameEngineLab.GolfIt.Features.Runtime;

public enum GameState
{
    Menu,
    Settings,
    Playing
}

public sealed class GameStateResource
{
    public GameState Current { get; set; } = GameState.Menu;
}
