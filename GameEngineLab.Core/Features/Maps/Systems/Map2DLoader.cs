using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameEngineLab.Core.Features.Maps.Resources;

namespace GameEngineLab.Core.Features.Maps.Systems;

public static class Map2DLoader
{
    public static Map2DModel LoadFromJson(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Map path is required.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Map file was not found.", path);
        }

        var json = File.ReadAllText(path);
        var model = JsonSerializer.Deserialize<MapJsonDto>(json, JsonOptions)
            ?? throw new InvalidDataException("Map JSON could not be parsed.");

        var data = model.MapData
            .Split('\n', StringSplitOptions.None)
            .Select(static line => line.TrimEnd('\r'))
            .ToArray();

        var textures = model.Textures.ToDictionary(
            static pair => pair.Key,
            static pair => MapTextureSource.FromRaw(pair.Value),
            StringComparer.OrdinalIgnoreCase);

        return new Map2DModel
        {
            TileSize = model.TileSize,
            Data = data,
            Symbols = new Dictionary<string, string>(model.Symbols, StringComparer.OrdinalIgnoreCase),
            TexturesPath = model.TexturesPath,
            Textures = textures,
        };
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private sealed class MapJsonDto
    {
        [JsonPropertyName("tile_size")]
        public byte TileSize { get; init; }

        [JsonPropertyName("symbols")]
        public Dictionary<string, string> Symbols { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("textures_path")]
        public string TexturesPath { get; init; } = string.Empty;

        [JsonPropertyName("textures")]
        public Dictionary<string, string> Textures { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("map_data")]
        public string MapData { get; init; } = string.Empty;
    }
}
