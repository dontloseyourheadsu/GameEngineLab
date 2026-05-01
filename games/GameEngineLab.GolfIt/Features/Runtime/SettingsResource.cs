namespace GameEngineLab.GolfIt.Features.Runtime;

public sealed class SettingsResource
{
    public float Volume { get; set; } = 1.0f;
    
    public int ResolutionScaleIndex { get; set; } = 1; // 1x, 2x, etc.
    
    public bool IsFullScreen { get; set; } = false;

    public bool NeedsApply { get; set; } = false;

    public string[] ScaleOptions => new[] { "1x (1024x768)", "1.5x (1536x1152)", "2x (2048x1536)" };

    public (int Width, int Height) GetResolution()
    {
        return ResolutionScaleIndex switch
        {
            0 => (1024, 768),
            1 => (1536, 1152),
            2 => (2048, 1536),
            _ => (1024, 768)
        };
    }
}
