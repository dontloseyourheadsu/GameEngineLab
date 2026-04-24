using System;
using System.Collections.Generic;

namespace GameEngineLab.Core.Features.Maps.Resources;

public sealed class Map2DModel
{
    public required byte TileSize { get; init; }

    public required IReadOnlyList<string> Data { get; init; }

    public required IReadOnlyDictionary<string, string> Symbols { get; init; }

    public required string TexturesPath { get; init; }

    public required IReadOnlyDictionary<string, MapTextureSource> Textures { get; init; }

    public int Width => Data.Count == 0 ? 0 : Data[0].Length;

    public int Height => Data.Count;

    public bool TryFindSymbolPosition(string symbol, out (int X, int Y) position)
    {
        position = default;

        if (string.IsNullOrEmpty(symbol))
        {
            return false;
        }

        for (var y = 0; y < Data.Count; y++)
        {
            var x = Data[y].IndexOf(symbol, StringComparison.Ordinal);
            if (x >= 0)
            {
                position = (x, y);
                return true;
            }
        }

        return false;
    }

    public char GetTile(int x, int y)
    {
        if (y < 0 || y >= Data.Count)
        {
            return '#';
        }

        var row = Data[y];
        if (x < 0 || x >= row.Length)
        {
            return '#';
        }

        return row[x];
    }
}
