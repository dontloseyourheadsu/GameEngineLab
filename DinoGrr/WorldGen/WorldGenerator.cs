using DinoGrr.Physics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DinoGrr.WorldGen
{
    public class WorldGenerator
    {
        public PhysicWorld CurrentWorld { get; set; }
        public int Level { get; set; }
        Dictionary<string, Level> gameData;

        public WorldGenerator()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "WorldGen/levels.json");

            string json = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            gameData = JsonSerializer.Deserialize<Dictionary<string, Level>>(json, options);
            Level = 0;

            LoadWorld();
        }

        public void LoadWorld()
        {
            if (gameData.ContainsKey(Level.ToString()))
            {
                CurrentWorld = new PhysicWorld(gameData[Level.ToString()].Width, 
                    gameData[Level.ToString()].Height, 
                    gameData[Level.ToString()].LevelGoal,
                    gameData[Level.ToString()].LevelPlayer, 
                    gameData[Level.ToString()].LevelDinosaurs.Values.ToList(),
                    gameData[Level.ToString()].LevelPlatforms.Values.ToList()
                    );
            }
        }

        public void NextLevel()
        {
            Level++;
            LoadWorld();
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

        [JsonPropertyName("height")]
        public int Height { get; set; }

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
