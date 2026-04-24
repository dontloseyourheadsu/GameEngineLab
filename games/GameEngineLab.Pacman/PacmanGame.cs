using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Maps.Systems;
using GameEngineLab.Core.Features.Runtime.Resources;
using GameEngineLab.Pacman.Features.Gameplay.Components;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Gameplay.Systems;
using GameEngineLab.Pacman.Features.Ghosts.Components;
using GameEngineLab.Pacman.Features.Ghosts.Resources;
using GameEngineLab.Pacman.Features.Ghosts.Systems;
using GameEngineLab.Pacman.Features.Map.Resources;
using GameEngineLab.Pacman.Features.Map.Systems;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.Pacman.Resources;
using GameEngineLab.Pacman.Features.Pacman.Systems;
using GameEngineLab.Pacman.Features.UI.Resources;
using GameEngineLab.Pacman.Features.UI.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.Pacman;

public sealed class PacmanGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly World _world = new();
    private readonly SystemScheduler _scheduler = new();

    private SpriteBatch? _spriteBatch;
    private Texture2D? _debugPixel;
    private KeyboardState _previousKeyboard;
    private MouseState _previousMouse;

    public PacmanGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = GameConstants.DefaultWindowWidth;
        _graphics.PreferredBackBufferHeight = GameConstants.DefaultWindowHeight;
    }

    protected override void Initialize()
    {
        var map = Map2DLoader.LoadFromJson(MapPaths.DefaultMap);
        _world.SetResource(new MapStateResource
        {
            Map = map,
        });

        var mapWidth = map.Width * map.TileSize;
        var mapHeight = map.Height * map.TileSize;

        _graphics.PreferredBackBufferWidth = mapWidth;
        _graphics.PreferredBackBufferHeight = mapHeight;
        _graphics.ApplyChanges();

        _world.SetResource(new MapBoundsResource
        {
            PlayArea = new Rectangle(0, 0, mapWidth, mapHeight),
        });
        _world.SetResource(BuildCollectibles(map));
        _world.SetResource(BuildMapEditor(map));
        _world.SetResource(new GameplayStateResource());
        _world.SetResource(new GhostModeResource());
        _world.SetResource(new AppModeResource
        {
            Mode = AppMode.Menu,
        });

        var spawn = map.TryFindSymbolPosition(
            map.Symbols.TryGetValue("pacman", out var pacmanSymbol) ? pacmanSymbol : "P",
            out var position)
            ? position
            : (map.Width / 2, map.Height / 2);

        var pacmanSpawnPosition = new Vector2((spawn.Item1 + 0.5f) * map.TileSize, (spawn.Item2 + 0.5f) * map.TileSize);
        _world.SetResource(new PacmanSpawnResource
        {
            SpawnPosition = pacmanSpawnPosition,
            SpawnTile = new Point(spawn.Item1, spawn.Item2),
        });

        var pacman = _world.CreateEntity();
        _world.SetComponent(pacman, new TransformComponent
        {
            Position = pacmanSpawnPosition,
        });
        _world.SetComponent(pacman, new VelocityComponent
        {
            Value = Vector2.Zero,
        });
        _world.SetComponent(pacman, new PacmanPlayerComponent
        {
            Speed = 240f,
            Radius = 14f,
            GridPosition = new Point(spawn.Item1, spawn.Item2),
            PreviousGridPosition = new Point(spawn.Item1, spawn.Item2),
            CurrentDirection = Point.Zero,
            DesiredDirection = Point.Zero,
            IsMoving = false,
            IsTeleporting = false,
            MoveProgress = 0f,
            MoveIntervalSeconds = 0.2f,
        });

        SpawnGhosts(map);

        _scheduler.AddSystem(new MenuSystem());
        _scheduler.AddSystem(new MapEditorSystem());
        _scheduler.AddSystem(new PacmanInputSystem());
        _scheduler.AddSystem(new PacmanMovementSystem());
        _scheduler.AddSystem(new PacmanCollectibleSystem());
        _scheduler.AddSystem(new GhostModeSystem());
        _scheduler.AddSystem(new GhostMovementSystem());
        _scheduler.AddSystem(new GhostCollisionSystem());
        _scheduler.AddSystem(new MapRenderSystem());
        _scheduler.AddSystem(new GhostRenderSystem());
        _scheduler.AddSystem(new DebugRenderSystem());
        _scheduler.AddSystem(new GameplayOverlaySystem());

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugPixel = new Texture2D(GraphicsDevice, 1, 1);
        _debugPixel.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        var currentKeyboard = Keyboard.GetState();
        var currentMouse = Mouse.GetState();

        var frameContext = new FrameContext(gameTime, currentKeyboard, _previousKeyboard, currentMouse, _previousMouse);
        _scheduler.Update(_world, frameContext);

        if (_world.TryGetResource<AppModeResource>(out var appMode) && appMode is not null)
        {
            Window.Title = appMode.Mode switch
            {
                AppMode.Menu => "GameEngineLab.Pacman | Menu | 1 Play  2 Map Editor  3 Asset Editor  4 Options",
                AppMode.MapEditor => "GameEngineLab.Pacman | Map Editor | LMB paint  RMB erase  1-6 tile  Enter save  Esc menu",
                AppMode.AssetEditor => "GameEngineLab.Pacman | Asset Editor migration pending | Esc menu",
                AppMode.Options => "GameEngineLab.Pacman | Options migration pending | Esc menu",
                _ => BuildGameplayTitle(),
            };
        }

        _previousKeyboard = currentKeyboard;
        _previousMouse = currentMouse;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(12, 14, 24));

        if (_spriteBatch is null || _debugPixel is null)
        {
            base.Draw(gameTime);
            return;
        }

        var currentKeyboard = Keyboard.GetState();
        var currentMouse = Mouse.GetState();
        var frameContext = new FrameContext(gameTime, currentKeyboard, _previousKeyboard, currentMouse, _previousMouse, _spriteBatch, _debugPixel);

        _spriteBatch.Begin();
        _scheduler.Draw(_world, frameContext);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private string BuildGameplayTitle()
    {
        if (!_world.TryGetResource<GameplayStateResource>(out var gameplay) || gameplay is null)
        {
            return "GameEngineLab.Pacman | Gameplay";
        }

        var state = gameplay.IsGameOver ? "GAME OVER" : gameplay.IsWin ? "YOU WIN" : "PLAYING";
        return $"GameEngineLab.Pacman | Score: {gameplay.Score} | Lives: {gameplay.Lives} | {state}";
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

        return new CollectiblesResource
        {
            Food = food,
            Pills = pills,
        };
    }

    private static MapEditorResource BuildMapEditor(Map2DModel map)
    {
        return new MapEditorResource
        {
            Tiles = map.Data.Select(static row => row.ToCharArray()).ToArray(),
            Palette = ['#', '.', 'o', 'S', 'P', ' '],
            SelectedPaletteIndex = 0,
            Dirty = false,
        };
    }

    private void SpawnGhosts(Map2DModel map)
    {
        var spawnerSymbol = map.Symbols.TryGetValue("spawner", out var value) ? value : "S";
        var behaviors = new[]
        {
            GhostBehavior.Blinky,
            GhostBehavior.Pinky,
            GhostBehavior.Inky,
            GhostBehavior.Clyde,
        };

        var index = 0;
        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                var tile = map.GetTile(x, y).ToString();
                if (!string.Equals(tile, spawnerSymbol, System.StringComparison.Ordinal))
                {
                    continue;
                }

                var ghostEntity = _world.CreateEntity();
                var spawnPosition = new Vector2((x + 0.5f) * map.TileSize, (y + 0.5f) * map.TileSize);

                _world.SetComponent(ghostEntity, new TransformComponent
                {
                    Position = spawnPosition,
                });

                _world.SetComponent(ghostEntity, new GhostComponent
                {
                    Behavior = behaviors[index % behaviors.Length],
                    State = GhostState.Scatter,
                    FrightenedTimer = 0f,
                    Speed = 140f,
                    Radius = 12f,
                    GridPosition = new Point(x, y),
                    PreviousGridPosition = new Point(x, y),
                    NextGridPosition = new Point(x, y),
                    CurrentDirection = Point.Zero,
                    IsMoving = false,
                    MoveProgress = 0f,
                    MoveIntervalSeconds = 0.2f,
                    SpawnTile = new Point(x, y),
                    SpawnPosition = spawnPosition,
                });

                index++;
            }
        }
    }
}
