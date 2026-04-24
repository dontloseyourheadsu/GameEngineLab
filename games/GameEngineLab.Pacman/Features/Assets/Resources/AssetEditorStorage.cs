using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Assets.Resources;

public static class AssetEditorStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static AssetLibraryResource LoadLibraryOrDefault(string path)
    {
        if (!File.Exists(path))
        {
            var lib = new AssetLibraryResource();
            lib.Groups.Add(CreateDefaultGroup("Default Pack"));
            return lib;
        }

        try
        {
            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<AssetLibraryDto>(json);
            
            var lib = new AssetLibraryResource();
            if (dto?.Groups == null) return lib;

            foreach (var gDto in dto.Groups)
            {
                var group = new AssetGroup { Name = gDto.Name, IsDone = gDto.IsDone };
                foreach (var aDto in gDto.Assets)
                {
                    var asset = new EditableAsset
                    {
                        Name = aDto.Name,
                        Width = aDto.Width,
                        Height = aDto.Height,
                        Resolution = aDto.Resolution,
                        Mode = aDto.Mode
                    };

                    foreach (var frameBytes in aDto.Frames)
                    {
                        var pixels = new Color[asset.Width * asset.Height];
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            int baseIdx = i * 4;
                            pixels[i] = new Color(frameBytes[baseIdx], frameBytes[baseIdx+1], frameBytes[baseIdx+2], frameBytes[baseIdx+3]);
                        }
                        asset.Frames.Add(pixels);
                    }
                    group.Assets.Add(asset);
                }
                lib.Groups.Add(group);
            }
            return lib;
        }
        catch
        {
            var lib = new AssetLibraryResource();
            lib.Groups.Add(CreateDefaultGroup("Default Pack"));
            return lib;
        }
    }

    public static void SaveLibrary(string path, AssetLibraryResource library)
    {
        var dto = new AssetLibraryDto
        {
            Groups = library.Groups.Select(g => new AssetGroupDto
            {
                Name = g.Name,
                IsDone = g.IsDone,
                Assets = g.Assets.Select(a => new AssetDto
                {
                    Name = a.Name,
                    Width = a.Width,
                    Height = a.Height,
                    Resolution = a.Resolution,
                    Mode = a.Mode,
                    Frames = a.Frames.Select(f => {
                        var bytes = new byte[f.Length * 4];
                        for (int i = 0; i < f.Length; i++) {
                            bytes[i*4] = f[i].R;
                            bytes[i*4+1] = f[i].G;
                            bytes[i*4+2] = f[i].B;
                            bytes[i*4+3] = f[i].A;
                        }
                        return bytes;
                    }).ToList()
                }).ToList()
            }).ToList()
        };

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(dto, JsonOptions));
    }

    public static AssetGroup CreateDefaultGroup(string name)
    {
        var group = new AssetGroup { Name = name };
        var definitions = new (string Name, int Frames)[]
        {
            ("Pacman", 4), ("Ghost", 1), ("Wall", 1), ("Food", 1), ("Pill", 1),
        };

        foreach (var def in definitions)
        {
            var asset = new EditableAsset
            {
                Name = def.Name,
                Width = 32,
                Height = 32,
                Resolution = 32,
                Mode = AssetCanvasMode.Pixel
            };
            for (int i = 0; i < def.Frames; i++)
            {
                var pixels = new Color[32 * 32];
                Array.Fill(pixels, Color.Transparent);
                asset.Frames.Add(pixels);
            }
            group.Assets.Add(asset);
        }
        return group;
    }

    public static Color[] BuildPalette()
    {
        return [
            new Color(220, 40, 55), new Color(235, 127, 49), new Color(250, 214, 68),
            new Color(78, 170, 64), new Color(57, 111, 214), new Color(121, 82, 196),
            new Color(222, 222, 222), new Color(30, 30, 30), Color.Transparent
        ];
    }

    private sealed class AssetLibraryDto
    {
        public List<AssetGroupDto> Groups { get; set; } = new();
    }

    private sealed class AssetGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public List<AssetDto> Assets { get; set; } = new();
        public bool IsDone { get; set; }
    }

    private sealed class AssetDto
    {
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public int Resolution { get; set; }
        public AssetCanvasMode Mode { get; set; }
        public List<byte[]> Frames { get; set; } = new();
    }
}
