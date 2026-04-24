using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Maps.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Components;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.Pacman.Resources;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.Pacman.Features.Map.Systems;

public sealed class MapEditorSystem : IGameSystem
{
    public int Order => -5;

    public void Update(World world, FrameContext frameContext)
    {
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.MapEditor)
        {
            return;
        }

        var editor = world.GetRequiredResource<MapEditorResource>();
        var mapState = world.GetRequiredResource<MapStateResource>();

        if (IsNewKeyPress(frameContext, Keys.Escape))
        {
            appMode.Mode = AppMode.Menu;
            return;
        }

        for (var i = 0; i < editor.Palette.Length; i++)
        {
            var key = (Keys)((int)Keys.D1 + i);
            if (IsNewKeyPress(frameContext, key))
            {
                editor.SelectedPaletteIndex = i;
            }
        }

        if (IsNewLeftClick(frameContext, out var leftPoint))
        {
            TryPaint(editor, mapState.Map, leftPoint, true);
        }

        if (IsNewRightClick(frameContext, out var rightPoint))
        {
            TryPaint(editor, mapState.Map, rightPoint, false);
        }

        if (IsNewKeyPress(frameContext, Keys.Enter))
        {
            SaveEditor(world, editor, mapState.Map);
            appMode.Mode = AppMode.Menu;
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch is null || frameContext.DebugPixel is null)
        {
            return;
        }

        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.MapEditor)
        {
            return;
        }

        var editor = world.GetRequiredResource<MapEditorResource>();
        var map = world.GetRequiredResource<MapStateResource>().Map;

        var tileSize = map.TileSize;

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = editor.Tiles[y][x];
                var rect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                frameContext.SpriteBatch.Draw(frameContext.DebugPixel, rect, GetColor(tile));
                frameContext.SpriteBatch.Draw(frameContext.DebugPixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), new Color(255, 255, 255, 16));
                frameContext.SpriteBatch.Draw(frameContext.DebugPixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), new Color(255, 255, 255, 16));
            }
        }

        var panel = new Rectangle(10, 10, 260, 150);
        frameContext.SpriteBatch.Draw(frameContext.DebugPixel, panel, new Color(0, 0, 0, 140));

        for (var i = 0; i < editor.Palette.Length; i++)
        {
            var colorRect = new Rectangle(20 + (i * 36), 40, 28, 28);
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, colorRect, GetColor(editor.Palette[i]));
            if (i == editor.SelectedPaletteIndex)
            {
                frameContext.SpriteBatch.Draw(frameContext.DebugPixel, new Rectangle(colorRect.X - 2, colorRect.Y - 2, colorRect.Width + 4, 2), Color.White);
                frameContext.SpriteBatch.Draw(frameContext.DebugPixel, new Rectangle(colorRect.X - 2, colorRect.Bottom, colorRect.Width + 4, 2), Color.White);
            }
        }

        if (TryGetTileUnderMouse(frameContext.CurrentMouse.Position, map, out var cell))
        {
            var highlight = new Rectangle(cell.X * tileSize, cell.Y * tileSize, tileSize, tileSize);
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, new Rectangle(highlight.X, highlight.Y, highlight.Width, 2), new Color(255, 255, 255, 180));
            frameContext.SpriteBatch.Draw(frameContext.DebugPixel, new Rectangle(highlight.X, highlight.Bottom - 2, highlight.Width, 2), new Color(255, 255, 255, 180));
        }
    }

    private static void SaveEditor(World world, MapEditorResource editor, Map2DModel original)
    {
        var lines = editor.Tiles.Select(chars => new string(chars)).ToArray();
        var dto = new
        {
            symbols = original.Symbols,
            textures_path = original.TexturesPath,
            textures = original.Textures.ToDictionary(p => p.Key, p => p.Value.Value),
            tile_size = original.TileSize,
            map_data = string.Join("\n", lines),
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(MapPaths.DefaultMap, json);

        var loaded = Map2DLoader.LoadFromJson(MapPaths.DefaultMap);
        world.SetResource(new MapStateResource { Map = loaded });
        world.SetResource(BuildCollectibles(loaded));

        RebuildEntitiesForMap(world, loaded);

        editor.Dirty = false;
    }

    private static void RebuildEntitiesForMap(World world, Map2DModel map)
    {
        foreach (var entity in world.GetEntitiesWith<GhostComponent>().ToList())
        {
            world.DestroyEntity(entity);
        }

        if (TryGetPacmanEntity(world, out var pacEntity))
        {
            var spawn = map.TryFindSymbolPosition(map.Symbols.TryGetValue("pacman", out var p) ? p : "P", out var pos)
                ? pos
                : (map.Width / 2, map.Height / 2);

            var spawnTile = new Point(spawn.Item1, spawn.Item2);
            var spawnPos = TileCenter(spawnTile, map.TileSize);
            world.SetResource(new PacmanSpawnResource { SpawnTile = spawnTile, SpawnPosition = spawnPos });

            if (world.TryGetComponent<TransformComponent>(pacEntity, out var transform))
            {
                transform.Position = spawnPos;
                world.SetComponent(pacEntity, transform);
            }

            if (world.TryGetComponent<PacmanPlayerComponent>(pacEntity, out var pacman))
            {
                pacman.GridPosition = spawnTile;
                pacman.PreviousGridPosition = spawnTile;
                pacman.CurrentDirection = Point.Zero;
                pacman.DesiredDirection = Point.Zero;
                pacman.IsMoving = false;
                pacman.MoveProgress = 0f;
                world.SetComponent(pacEntity, pacman);
            }

            if (world.TryGetComponent<VelocityComponent>(pacEntity, out var velocity))
            {
                velocity.Value = Vector2.Zero;
                world.SetComponent(pacEntity, velocity);
            }
        }

        SpawnGhosts(world, map);
    }

    private static void SpawnGhosts(World world, Map2DModel map)
    {
        var symbol = map.Symbols.TryGetValue("spawner", out var value) ? value : "S";
        var behaviors = new[] { GhostBehavior.Blinky, GhostBehavior.Pinky, GhostBehavior.Inky, GhostBehavior.Clyde };
        var index = 0;

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                if (!string.Equals(map.GetTile(x, y).ToString(), symbol, System.StringComparison.Ordinal))
                {
                    continue;
                }

                var entity = world.CreateEntity();
                var tile = new Point(x, y);
                var spawnPos = TileCenter(tile, map.TileSize);

                world.SetComponent(entity, new TransformComponent { Position = spawnPos });
                world.SetComponent(entity, new GhostComponent
                {
                    Behavior = behaviors[index % behaviors.Length],
                    State = GhostState.Scatter,
                    FrightenedTimer = 0f,
                    Speed = 140f,
                    Radius = 12f,
                    GridPosition = tile,
                    PreviousGridPosition = tile,
                    NextGridPosition = tile,
                    CurrentDirection = Point.Zero,
                    IsMoving = false,
                    MoveProgress = 0f,
                    MoveIntervalSeconds = 0.2f,
                    SpawnTile = tile,
                    SpawnPosition = spawnPos,
                });

                index++;
            }
        }
    }

    private static bool TryGetPacmanEntity(World world, out EntityId entityId)
    {
        foreach (var entity in world.GetEntitiesWith<PacmanPlayerComponent>())
        {
            entityId = entity;
            return true;
        }

        entityId = default;
        return false;
    }

    private static CollectiblesResource BuildCollectibles(Map2DModel map)
    {
        var food = new HashSet<Point>();
        var pills = new HashSet<Point>();

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.GetTile(x, y);
                if (tile == '.')
                {
                    food.Add(new Point(x, y));
                }
                else if (tile == 'o')
                {
                    pills.Add(new Point(x, y));
                }
            }
        }

        return new CollectiblesResource { Food = food, Pills = pills };
    }

    private static void TryPaint(MapEditorResource editor, Map2DModel map, Point mouse, bool paint)
    {
        if (!TryGetTileUnderMouse(mouse, map, out var tile))
        {
            return;
        }

        var newChar = paint ? editor.Palette[editor.SelectedPaletteIndex] : ' ';
        editor.Tiles[tile.Y][tile.X] = newChar;
        editor.Dirty = true;
    }

    private static bool TryGetTileUnderMouse(Point mousePosition, Map2DModel map, out Point tile)
    {
        tile = default;
        var x = mousePosition.X / map.TileSize;
        var y = mousePosition.Y / map.TileSize;
        if (x < 0 || y < 0 || x >= map.Width || y >= map.Height)
        {
            return false;
        }

        tile = new Point(x, y);
        return true;
    }

    private static Vector2 TileCenter(Point tile, int tileSize)
    {
        return new Vector2((tile.X + 0.5f) * tileSize, (tile.Y + 0.5f) * tileSize);
    }

    private static bool IsNewKeyPress(FrameContext frameContext, Keys key)
    {
        return frameContext.CurrentKeyboard.IsKeyDown(key) && frameContext.PreviousKeyboard.IsKeyUp(key);
    }

    private static bool IsNewLeftClick(FrameContext frameContext, out Point point)
    {
        point = frameContext.CurrentMouse.Position;
        return frameContext.CurrentMouse.LeftButton == ButtonState.Pressed
               && frameContext.PreviousMouse.LeftButton == ButtonState.Released;
    }

    private static bool IsNewRightClick(FrameContext frameContext, out Point point)
    {
        point = frameContext.CurrentMouse.Position;
        return frameContext.CurrentMouse.RightButton == ButtonState.Pressed
               && frameContext.PreviousMouse.RightButton == ButtonState.Released;
    }

    private static Color GetColor(char tile)
    {
        return tile switch
        {
            '#' => new Color(38, 70, 140),
            '.' => new Color(46, 52, 92),
            'o' => new Color(66, 72, 112),
            'S' => new Color(72, 56, 100),
            'P' => new Color(196, 164, 34),
            _ => new Color(22, 26, 46),
        };
    }
}
