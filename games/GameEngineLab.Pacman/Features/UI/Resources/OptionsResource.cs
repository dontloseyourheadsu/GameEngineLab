namespace GameEngineLab.Pacman.Features.UI.Resources;

public sealed class OptionsResource
{
    public float MusicVolume { get; set; } = 0.5f;

    public float SfxVolume { get; set; } = 0.5f;

    public float UiScale { get; set; } = 1.0f;

    public float PendingMusicVolume { get; set; } = 0.5f;

    public float PendingSfxVolume { get; set; } = 0.5f;

    public float PendingUiScale { get; set; } = 1.0f;

    public bool DraggingMusic { get; set; }

    public bool DraggingSfx { get; set; }

    public bool DraggingScale { get; set; }

    public void SyncPendingFromCurrent()
    {
        PendingMusicVolume = MusicVolume;
        PendingSfxVolume = SfxVolume;
        PendingUiScale = UiScale;
    }

    public void ApplyPending()
    {
        MusicVolume = PendingMusicVolume;
        SfxVolume = PendingSfxVolume;
        UiScale = PendingUiScale;
    }
}
