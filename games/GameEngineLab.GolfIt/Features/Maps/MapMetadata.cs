using System;

namespace GameEngineLab.GolfIt.Features.Maps;

public sealed class MapMetadata
{
    public string Name { get; set; } = "New Map";
    public string FilePath { get; set; } = string.Empty;
    public DateTime LastModified { get; set; } = DateTime.Now;
}
