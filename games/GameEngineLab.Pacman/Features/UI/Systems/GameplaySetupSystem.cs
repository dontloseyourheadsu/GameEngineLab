using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Pacman.Features.Assets.Resources;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Gameplay.Components;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.Pacman.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngineLab.Pacman.Features.UI.Systems;

public sealed class GameplaySetupSystem : IGameSystem
{
    public int Order => -15;

    private static readonly Color ColorBg = new(8, 8, 16);
    private static readonly Color ColorPanel = new(16, 16, 32);
    private static readonly Color ColorPanelAccent = new(24, 24, 48);
    private static readonly Color ColorNeonCyan = new(0, 255, 255);
    private static readonly Color ColorNeonGreen = new(0, 255, 128);
    private static readonly Color ColorNeonMagenta = new(255, 0, 255);
    private static readonly Color ColorNeonYellow = new(255, 255, 0);
    private static readonly Color ColorText = new(220, 230, 255);

    public void Update(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.GameSetup) return;

        var mapLib = world.GetRequiredResource<MapLibraryResource>();
        var assetLib = world.GetRequiredResource<AssetLibraryResource>();
        var options = world.GetRequiredResource<OptionsResource>();
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        float scale = options.UiScale * Math.Max(1.0f, Math.Min(sw / 1024f, sh / 768f));

        if (IsNewKeyPress(frameContext, Keys.Escape)) { appMode.Mode = AppMode.Menu; return; }

        if (IsNewLeftClick(frameContext, out var mouse))
        {
            // Start Game
            var startBtn = GetStartBtnRect(sw, sh, scale);
            if (startBtn.Contains(mouse) && mapLib.SelectedProjectIndex >= 0 && assetLib.SelectedGroupIndex >= 0)
            {
                // Access GraphicsDevice from the world's FrameContext or via SpriteBatch
                if (frameContext.SpriteBatch != null)
                {
                    StartGame(world, frameContext.SpriteBatch.GraphicsDevice);
                    appMode.Mode = AppMode.Gameplay;
                }
                return;
            }

            // Map selection
            for (int i = 0; i < mapLib.Projects.Count; i++)
            {
                if (GetMapRect(i, sw, sh, scale).Contains(mouse))
                {
                    mapLib.SelectedProjectIndex = i;
                    break;
                }
            }

            // Asset selection
            for (int i = 0; i < assetLib.Groups.Count; i++)
            {
                if (GetAssetRect(i, sw, sh, scale).Contains(mouse))
                {
                    assetLib.SelectedGroupIndex = i;
                    break;
                }
            }
        }
    }

    private void StartGame(World world, GraphicsDevice gd)
    {
        var mapLib = world.GetRequiredResource<MapLibraryResource>();
        var assetLib = world.GetRequiredResource<AssetLibraryResource>();
        var mapProj = mapLib.Projects[mapLib.SelectedProjectIndex];
        var assetGroup = assetLib.Groups[assetLib.SelectedGroupIndex];

        // 1. Rebuild MapStateResource
        var mapModel = ConvertToModel(mapProj);
        world.SetResource(new MapStateResource { Map = mapModel });
        
        // 2. Rebuild Collectibles
        world.SetResource(BuildCollectibles(mapModel));

        // 3. Generate Gameplay Textures
        var gpAssets = world.GetRequiredResource<GameplayAssetsResource>();
        gpAssets.Dispose();
        
        gpAssets.PacmanFrames = assetGroup.Assets.First(a => a.Name == "Pacman").Frames
            .Select(f => CreateTexture(gd, f, assetGroup.Assets.First(a => a.Name == "Pacman").Resolution)).ToArray();
        gpAssets.Ghost = CreateTexture(gd, assetGroup.Assets.First(a => a.Name == "Ghost").Frames[0], assetGroup.Assets.First(a => a.Name == "Ghost").Resolution);
        gpAssets.Wall = CreateTexture(gd, assetGroup.Assets.First(a => a.Name == "Wall").Frames[0], assetGroup.Assets.First(a => a.Name == "Wall").Resolution);
        gpAssets.Food = CreateTexture(gd, assetGroup.Assets.First(a => a.Name == "Food").Frames[0], assetGroup.Assets.First(a => a.Name == "Food").Resolution);
        gpAssets.Pill = CreateTexture(gd, assetGroup.Assets.First(a => a.Name == "Pill").Frames[0], assetGroup.Assets.First(a => a.Name == "Pill").Resolution);

        // 4. Reset Game State
        var gs = world.GetRequiredResource<GameplayStateResource>();
        gs.Score = 0;
        gs.Lives = 3;
        gs.IsGameOver = false;
        gs.IsWin = false;

        // 5. Respawn Entities
        RebuildEntities(world, mapModel);
    }

    private Texture2D CreateTexture(GraphicsDevice gd, Color[] data, int res)
    {
        var tex = new Texture2D(gd, res, res);
        tex.SetData(data);
        return tex;
    }

    private Map2DModel ConvertToModel(MapProject proj)
    {
        return new Map2DModel
        {
            TileSize = 32, // Fixed for now
            Data = proj.Tiles.Select(r => new string(r)).ToList(),
            Symbols = new Dictionary<string, string> { ["pacman"] = "P", ["spawner"] = "S", ["wall"] = "#" },
            TexturesPath = "",
            Textures = new Dictionary<string, MapTextureSource>()
        };
    }

    private CollectiblesResource BuildCollectibles(Map2DModel map)
    {
        var food = new HashSet<Point>();
        var pills = new HashSet<Point>();
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                var t = map.GetTile(x, y);
                if (t == '.') food.Add(new Point(x, y));
                else if (t == 'o') pills.Add(new Point(x, y));
            }
        }
        return new CollectiblesResource { Food = food, Pills = pills };
    }

    private void RebuildEntities(World world, Map2DModel map)
    {
        // Destroy existing
        foreach (var e in world.GetEntitiesWith<GhostComponent>().ToList()) world.DestroyEntity(e);
        foreach (var e in world.GetEntitiesWith<PacmanPlayerComponent>().ToList()) world.DestroyEntity(e);

        // Spawn Pacman
        var spawn = map.TryFindSymbolPosition("P", out var pos) ? pos : (map.Width/2, map.Height/2);
        var pacSpawn = new Vector2((spawn.Item1 + 0.5f) * 32, (spawn.Item2 + 0.5f) * 32);
        world.SetResource(new PacmanSpawnResource { SpawnTile = new Point(spawn.Item1, spawn.Item2), SpawnPosition = pacSpawn });

        var pac = world.CreateEntity();
        world.SetComponent(pac, new TransformComponent { Position = pacSpawn });
        world.SetComponent(pac, new VelocityComponent { Value = Vector2.Zero });
        world.SetComponent(pac, new PacmanPlayerComponent {
            Speed = 240f, Radius = 14f, GridPosition = new Point(spawn.Item1, spawn.Item2),
            PreviousGridPosition = new Point(spawn.Item1, spawn.Item2), MoveIntervalSeconds = 0.2f
        });

        // Spawn Ghosts
        var behaviors = new[] { GhostBehavior.Blinky, GhostBehavior.Pinky, GhostBehavior.Inky, GhostBehavior.Clyde };
        int gIdx = 0;
        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                if (map.GetTile(x, y) == 'S')
                {
                    var g = world.CreateEntity();
                    var gPos = new Vector2((x + 0.5f) * 32, (y + 0.5f) * 32);
                    world.SetComponent(g, new TransformComponent { Position = gPos });
                    world.SetComponent(g, new GhostComponent {
                        Behavior = behaviors[gIdx % 4], State = GhostState.Scatter, Speed = 140f, Radius = 12f,
                        GridPosition = new Point(x, y), PreviousGridPosition = new Point(x, y), NextGridPosition = new Point(x, y),
                        MoveIntervalSeconds = 0.2f, SpawnTile = new Point(x, y), SpawnPosition = gPos
                    });
                    gIdx++;
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.GameSetup) return;

        var mapLib = world.GetRequiredResource<MapLibraryResource>();
        var assetLib = world.GetRequiredResource<AssetLibraryResource>();
        var options = world.GetRequiredResource<OptionsResource>();
        var sb = frameContext.SpriteBatch!;
        var pixel = frameContext.DebugPixel!;
        var sw = frameContext.Viewport.Width;
        var sh = frameContext.Viewport.Height;
        float scale = options.UiScale * Math.Max(1.0f, Math.Min(sw / 1024f, sh / 768f));

        sb.Draw(pixel, new Rectangle(0, 0, sw, sh), ColorBg);

        PixelText.Draw(sb, pixel, "GAME SETUP", new Vector2(sw / 2 - (int)(100 * scale), 30 * scale), (int)(3 * scale), ColorNeonCyan);

        // Map Section
        PixelText.Draw(sb, pixel, "SELECT MAP", new Vector2(100 * scale, 100 * scale), (int)(2 * scale), ColorNeonMagenta);
        for (int i = 0; i < mapLib.Projects.Count; i++)
        {
            var rect = GetMapRect(i, sw, sh, scale);
            bool sel = mapLib.SelectedProjectIndex == i;
            sb.Draw(pixel, rect, sel ? ColorPanelAccent : ColorPanel);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), sel ? ColorNeonCyan : Color.Transparent);
            PixelText.Draw(sb, pixel, mapLib.Projects[i].Name, new Vector2(rect.X + 10, rect.Y + 10), (int)(1 * scale), ColorText);
        }

        // Asset Section
        PixelText.Draw(sb, pixel, "SELECT ASSETS", new Vector2(sw / 2 + 50 * scale, 100 * scale), (int)(2 * scale), ColorNeonMagenta);
        for (int i = 0; i < assetLib.Groups.Count; i++)
        {
            var rect = GetAssetRect(i, sw, sh, scale);
            bool sel = assetLib.SelectedGroupIndex == i;
            sb.Draw(pixel, rect, sel ? ColorPanelAccent : ColorPanel);
            sb.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 2), sel ? ColorNeonCyan : Color.Transparent);
            PixelText.Draw(sb, pixel, assetLib.Groups[i].Name, new Vector2(rect.X + 10, rect.Y + 10), (int)(1 * scale), ColorText);
        }

        // Start Button
        var startBtn = GetStartBtnRect(sw, sh, scale);
        bool canStart = mapLib.SelectedProjectIndex >= 0 && assetLib.SelectedGroupIndex >= 0;
        sb.Draw(pixel, startBtn, canStart ? ColorNeonGreen : Color.DarkSlateGray);
        var sSize = PixelText.Measure("START GAME", (int)(2 * scale));
        PixelText.Draw(sb, pixel, "START GAME", new Vector2(startBtn.Center.X - sSize.X / 2, startBtn.Center.Y - sSize.Y / 2), (int)(2 * scale), canStart ? Color.Black : Color.Gray);
    }

    private Rectangle GetMapRect(int i, int sw, int sh, float scale) => new((int)(100 * scale), (int)(150 * scale) + i * (int)(60 * scale), (int)(300 * scale), (int)(50 * scale));
    private Rectangle GetAssetRect(int i, int sw, int sh, float scale) => new(sw / 2 + (int)(50 * scale), (int)(150 * scale) + i * (int)(60 * scale), (int)(300 * scale), (int)(50 * scale));
    private Rectangle GetStartBtnRect(int sw, int sh, float scale) => new(sw / 2 - (int)(150 * scale), sh - (int)(120 * scale), (int)(300 * scale), (int)(70 * scale));

    private static bool IsNewKeyPress(FrameContext frameContext, Keys key) => frameContext.CurrentKeyboard.IsKeyDown(key) && frameContext.PreviousKeyboard.IsKeyUp(key);
    private static bool IsNewLeftClick(FrameContext frameContext, out Point point)
    {
        point = frameContext.CurrentMouse.Position;
        return frameContext.CurrentMouse.LeftButton == ButtonState.Pressed && frameContext.PreviousMouse.LeftButton == ButtonState.Released;
    }
}
