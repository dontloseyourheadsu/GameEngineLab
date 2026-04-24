namespace GameEngineLab.Pacman.Features.Gameplay.Resources;

public sealed class GameplayStateResource
{
    public int Score { get; set; }

    public int Lives { get; set; } = 3;

    public bool IsGameOver { get; set; }

    public bool IsWin { get; set; }
}
