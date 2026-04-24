namespace GameEngineLab.Core.Features.Maps.Resources;

public enum MapTextureSourceKind
{
    File,
    Color,
}

public readonly record struct MapTextureSource(MapTextureSourceKind Kind, string Value)
{
    public static MapTextureSource FromRaw(string value)
    {
        return IsColorValue(value)
            ? new MapTextureSource(MapTextureSourceKind.Color, value)
            : new MapTextureSource(MapTextureSourceKind.File, value);
    }

    private static bool IsColorValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        return trimmed.StartsWith("#", System.StringComparison.Ordinal)
               || trimmed.StartsWith("rgb(", System.StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("rgba(", System.StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("hsl(", System.StringComparison.OrdinalIgnoreCase)
               || trimmed.StartsWith("hsla(", System.StringComparison.OrdinalIgnoreCase);
    }
}
