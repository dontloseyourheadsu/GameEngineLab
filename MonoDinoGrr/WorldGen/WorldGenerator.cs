using MonoDinoGrr.Physics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MonoDinoGrr.WorldGen
{
    public class WorldGenerator
    {
        public int Level { get; set; }
        Dictionary<string, Level> gameData;
        public PhysicWorld physicWorld;

        public WorldGenerator()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "WorldGen/levels.json");

            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            gameData = JsonSerializer.Deserialize<Dictionary<string, Level>>(json, options);
            Level = 0;
        }

        public List<LevelPlatform> GetLevelPlatforms()
        {
            return gameData[Level.ToString()].LevelPlatforms.Values.ToList();
        }

        public LevelGoal GetLevelGoal()
        {
            return gameData[Level.ToString()].LevelGoal;
        }

        public LevelPlayer GetLevelPlayer()
        {
            return gameData[Level.ToString()].LevelPlayer;
        }

        public int GetLevelWidth()
        {
            return gameData[Level.ToString()].Width;
        }

        public List<LevelDinosaur> GetLevelDinosaurs()
        {
            return gameData[Level.ToString()].LevelDinosaurs.Values.ToList();
        }

        public int GetLevelCount()
        {
            return gameData.Count;
        }
    }

    public class LevelPlatform
    {
        [JsonPropertyName("X")]
        public int X { get; set; }

        [JsonPropertyName("Y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class LevelGoal
    {
        [JsonPropertyName("X")]
        public int X { get; set; }

        [JsonPropertyName("Y")]
        public int Y { get; set; }
    }

    public class LevelPlayer
    {
        [JsonPropertyName("X")]
        public int X { get; set; }

        [JsonPropertyName("Y")]
        public int Y { get; set; }
    }

    public class LevelDinosaur
    {
        [JsonPropertyName("dino")]
        public string Dino { get; set; }

        [JsonPropertyName("X")]
        public int X { get; set; }

        [JsonPropertyName("Y")]
        public int Y { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class Level
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("levelGoal")]
        public LevelGoal LevelGoal { get; set; }

        [JsonPropertyName("levelPlayer")]
        public LevelPlayer LevelPlayer { get; set; }

        [JsonPropertyName("levelDinosaurs")]
        public Dictionary<string, LevelDinosaur> LevelDinosaurs { get; set; }

        [JsonPropertyName("levelPlatforms")]
        public Dictionary<string, LevelPlatform> LevelPlatforms { get; set; }
    }
}
