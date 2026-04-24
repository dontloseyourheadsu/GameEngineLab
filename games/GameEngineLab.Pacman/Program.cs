namespace GameEngineLab.Pacman;

internal static class Program
{
    private static void Main()
    {
        using PacmanGame game = new();
        game.Run();
    }
}
