using System;
using System.IO;
using System.Text.Json;

namespace GameEngineLab.Pacman.Features.UI.Resources;

public static class OptionsStorage
{
    public static OptionsResource LoadOrDefault(string path)
    {
        if (!File.Exists(path))
        {
            return new OptionsResource();
        }

        try
        {
            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<OptionsDto>(json);
            if (dto is null)
            {
                return new OptionsResource();
            }

            var options = new OptionsResource
            {
                MusicVolume = Clamp01(dto.MusicVolume),
                SfxVolume = Clamp01(dto.SfxVolume),
                UiScale = Math.Clamp(dto.UiScale, 0.25f, 2.0f),
            };
            options.SyncPendingFromCurrent();
            return options;
        }
        catch
        {
            return new OptionsResource();
        }
    }

    public static void Save(string path, OptionsResource options)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var dto = new OptionsDto
        {
            MusicVolume = Clamp01(options.MusicVolume),
            SfxVolume = Clamp01(options.SfxVolume),
            UiScale = Math.Clamp(options.UiScale, 0.25f, 2.0f),
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    private static float Clamp01(float value)
    {
        return Math.Clamp(value, 0f, 1f);
    }

    private sealed class OptionsDto
    {
        public float MusicVolume { get; set; } = 0.5f;

        public float SfxVolume { get; set; } = 0.5f;

        public float UiScale { get; set; } = 1.0f;
    }
}
