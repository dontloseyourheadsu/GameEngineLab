namespace GameEngineLab.Pacman.Features.Ghosts.Resources;

public sealed class GhostModeResource
{
    public float Timer { get; set; }

    public bool IsScatterMode { get; set; } = true;

    public float ScatterDuration { get; init; } = 7f;

    public float ChaseDuration { get; init; } = 20f;
}
