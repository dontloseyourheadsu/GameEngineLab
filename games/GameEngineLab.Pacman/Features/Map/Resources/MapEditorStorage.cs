using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Maps.Systems;

namespace GameEngineLab.Pacman.Features.Map.Resources;

public static class MapEditorStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static MapLibraryResource LoadLibraryOrDefault(string path)
    {
        if (!File.Exists(path))
        {
            var lib = new MapLibraryResource();
            lib.Projects.Add(CreateDefaultProject("Classic Map", 20, 20));
            return lib;
        }

        try
        {
            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<MapLibraryDto>(json);
            
            var lib = new MapLibraryResource();
            if (dto?.Projects == null) return lib;

            foreach (var pDto in dto.Projects)
            {
                var project = new MapProject
                {
                    Name = pDto.Name,
                    Width = pDto.Width,
                    Height = pDto.Height,
                    IsDone = pDto.IsDone,
                    Tiles = pDto.Tiles.Select(row => row.ToCharArray()).ToArray()
                };
                lib.Projects.Add(project);
            }
            return lib;
        }
        catch
        {
            var lib = new MapLibraryResource();
            lib.Projects.Add(CreateDefaultProject("Classic Map", 20, 20));
            return lib;
        }
    }

    public static void SaveLibrary(string path, MapLibraryResource library)
    {
        var dto = new MapLibraryDto
        {
            Projects = library.Projects.Select(p => new MapProjectDto
            {
                Name = p.Name,
                Width = p.Width,
                Height = p.Height,
                IsDone = p.IsDone,
                Tiles = p.Tiles.Select(row => new string(row)).ToList()
            }).ToList()
        };

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(path, JsonSerializer.Serialize(dto, JsonOptions));
    }

    public static MapProject CreateDefaultProject(string name, int w, int h)
    {
        var project = new MapProject { Name = name, Width = w, Height = h };
        project.Tiles = new char[h][];
        for (int y = 0; y < h; y++)
        {
            project.Tiles[y] = new char[w];
            Array.Fill(project.Tiles[y], ' ');
        }
        
        // Add some basic walls and a pacman to make it non-empty
        for (int i = 0; i < w; i++) { project.Tiles[0][i] = '#'; project.Tiles[h - 1][i] = '#'; }
        for (int i = 0; i < h; i++) { project.Tiles[i][0] = '#'; project.Tiles[i][w - 1] = '#'; }
        
        if (w > 2 && h > 2) project.Tiles[h / 2][w / 2] = 'P';
        
        return project;
    }

    public static bool ValidateMap(MapProject project, out List<string> errors)
    {
        errors = new List<string>();
        int pacmanCount = 0;
        int ghostCount = 0;
        int foodCount = 0;

        foreach (var row in project.Tiles)
        {
            foreach (var tile in row)
            {
                if (tile == 'P') pacmanCount++;
                if (tile == 'S') ghostCount++;
                if (tile == '.' || tile == 'o') foodCount++;
            }
        }

        if (pacmanCount != 1) errors.Add("Place exactly 1 Pacman.");
        if (ghostCount == 0) errors.Add("Place at least 1 Ghost spawner.");
        if (foodCount == 0) errors.Add("Place some food or pills.");

        return errors.Count == 0;
    }

    private sealed class MapLibraryDto
    {
        public List<MapProjectDto> Projects { get; set; } = new();
    }

    private sealed class MapProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public List<string> Tiles { get; set; } = new();
        public bool IsDone { get; set; }
    }
}
