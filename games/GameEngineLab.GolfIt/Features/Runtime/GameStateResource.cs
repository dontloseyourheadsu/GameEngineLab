namespace GameEngineLab.GolfIt.Features.Runtime;

public enum GameState
{
    Menu,
    Playing,
    Settings,
    GameOver,
    MapEditorList,
    MapEditor,
    MultiplayerLobby
}

public sealed class GameStateResource
{
    public GameState Current { get; set; } = GameState.Menu;

    // Scoring
    public int Strokes { get; set; }
    public int Par { get; set; } = 3;
    public int StrokeLimit { get; set; } = 15;

    public string GetStrokeLegend()
    {
        if (Strokes == 1) return "HOLE IN ONE";

        int diff = Strokes - Par;
        return diff switch
        {
            -3 => "ALBATROSS",
            -2 => "EAGLE",
            -1 => "BIRDIE",
            0 => "PAR",
            1 => "BOGEY",
            2 => "DOUBLE BOGEY",
            3 => "TRIPLE BOGEY",
            _ => diff < 0 ? "UNDER PAR" : "OVER BOGEY"
        };
    }
}
