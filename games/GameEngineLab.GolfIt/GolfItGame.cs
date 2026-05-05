using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Physics.Resources;
using GameEngineLab.Core.Features.Physics.Systems;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.Core.Features.Rendering.Systems;
using GameEngineLab.Core.Features.Runtime.Resources;
using GameEngineLab.Core.Features.UI;
using GameEngineLab.Core.Features.UI.Components;
using GameEngineLab.Core.Features.UI.Resources;
using GameEngineLab.Core.Features.UI.Systems;
using GameEngineLab.GolfIt.Features.Ball.Components;
using GameEngineLab.GolfIt.Features.Input.Systems;
using GameEngineLab.GolfIt.Features.Physics.Components;
using GameEngineLab.GolfIt.Features.Physics.Systems;
using GameEngineLab.GolfIt.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Rendering.Systems;
using GameEngineLab.GolfIt.Features.Runtime;
using GameEngineLab.GolfIt.Features.UI.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace GameEngineLab.GolfIt;

public sealed class GolfItGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly World _world = new();
    private readonly SystemScheduler _scheduler = new();
    private readonly SystemScheduler _uiScheduler = new();

    private SpriteBatch? _spriteBatch;
    private Texture2D? _debugPixel;
    private KeyboardState _previousKeyboard;
    private MouseState _previousMouse;
    private GameState _lastGameState = GameState.Menu;
    private SettingsResource _settings = new();

    // UI Entity IDs for Settings tracking
    private EntityId _volumeSlider;
    private EntityId _scaleSelector;
    private EntityId _fullscreenCheckbox;

    public GolfItGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = false; // Block resizing as requested
        _graphics.PreferredBackBufferWidth = GameConstants.DefaultWindowWidth;
        _graphics.PreferredBackBufferHeight = GameConstants.DefaultWindowHeight;
    }

    protected override void Initialize()
    {
        _world.SetResource(new MapBoundsResource
        {
            PlayArea = new Rectangle(0, 0, GameConstants.DefaultWindowWidth, GameConstants.DefaultWindowHeight)
        });

        var library = new PaletteLibraryResource
        {
            General = TwilioQuestPalette.Create(),
            Specific = SpecificAssetsPalette.Create(),
            Cozy = CozyPalette.Create()
        };
        _world.SetResource(library);
        _world.SetResource(new GameStateResource());
        _world.SetResource(_settings);
        _world.SetResource(new ActionQueueResource());
        _world.SetResource(new UiThemeResource()
        {
            BorderColor = library.Cozy.GetColor(6),
            SurfaceColor = library.Cozy.GetColor(1),
            TextColor = library.Cozy.GetColor(4),
            HighlightColor = library.Cozy.GetColor(5),
            ShadowColor = library.Cozy.GetColor(3),
            GlobalScale = 1.0f
        });
        _world.SetResource(new CameraResource
        {
            Position = new Vector2(GameConstants.DefaultWindowWidth / 2f, GameConstants.DefaultWindowHeight / 2f),
            Zoom = 1.0f
        });

        // Create the heavy ball (NO SPRING)
        var ball = _world.CreateEntity();
        _world.SetComponent(ball, new BallComponent());
        _world.SetComponent(ball, new RigidBodyComponent 
        { 
            Shape = RigidBodyShape.Circle,
            BoundingRadius = 16f, 
            Restitution = 0.6f, 
            Friction = 0.99f,
            Mass = 5.0f // Heavy ball
        });
        _world.SetComponent(ball, new TransformComponent { Position = new Vector2(512, 600) });
        _world.SetComponent(ball, new VelocityComponent { Value = Vector2.Zero });
        _world.SetComponent(ball, new DrawColorComponent(library.Specific.GetColor(1))); // Use a specific color for the ball

        // Create a Rotating Polygon
        var poly = _world.CreateEntity();
        _world.SetComponent(poly, new TransformComponent { Position = new Vector2(300, 300) });
        _world.SetComponent(poly, new RigidBodyComponent { Shape = RigidBodyShape.Polygon, Mass = 0, Restitution = 0.8f });
        _world.SetComponent(poly, new PolygonComponent(new[] 
        {
            new Vector2(-50, -50),
            new Vector2(50, -50),
            new Vector2(80, 0),
            new Vector2(50, 50),
            new Vector2(-50, 50)
        }));
        _world.SetComponent(poly, new DrawColorComponent(library.General.GetColor(2)));

        // Create a SoftBody
        SoftBodyFactory.CreateCircle(_world, new Vector2(700, 300), 50f, 8, 80f, 15f, library.General.GetColor(3));

        // Create the Goal (Circle Zone)
        var goal = _world.CreateEntity();
        _world.SetComponent(goal, new TriggerZoneComponent("goal"));
        var goalPos = new Vector2(512, 150);
        _world.SetComponent(goal, new TransformComponent { Position = goalPos });
        _world.SetComponent(goal, new RigidBodyComponent
        {
            Shape = RigidBodyShape.Circle,
            BoundingRadius = 40f,
            Mass = 0f
        });
        _world.SetComponent(goal, new DrawColorComponent(Color.Black)); // Black circle as requested

        // Track occupied areas to prevent overlaps
        var occupiedAreas = new List<Rectangle>();
        // Ball area
        occupiedAreas.Add(new Rectangle(512 - 40, 600 - 40, 80, 80));
        // Goal area
        occupiedAreas.Add(new Rectangle((int)goalPos.X - 60, (int)goalPos.Y - 60, 120, 120));
        // Rotating Poly area (approximate)
        occupiedAreas.Add(new Rectangle(300 - 100, 300 - 100, 200, 200));
        // Softbody area (approximate)
        occupiedAreas.Add(new Rectangle(700 - 100, 300 - 100, 200, 200));

        // Create some obstacles randomly
        var random = new Random();
        for (int i = 0; i < 12; i++) // Increased count since we filter them
        {
            Vector2 pos = Vector2.Zero;
            Vector2 size = Vector2.Zero;
            bool overlapping = true;
            int attempts = 0;

            while (overlapping && attempts < 20)
            {
                pos = new Vector2(random.Next(100, 900), random.Next(200, 500));
                size = new Vector2(random.Next(60, 150), random.Next(30, 80));
                
                var newRect = new Rectangle((int)(pos.X - size.X / 2), (int)(pos.Y - size.Y / 2), (int)size.X, (int)size.Y);
                overlapping = false;
                foreach (var area in occupiedAreas)
                {
                    if (newRect.Intersects(area))
                    {
                        overlapping = true;
                        break;
                    }
                }
                attempts++;
            }

            if (!overlapping)
            {
                var colorIndex = random.Next(library.General.Colors.Count);
                var color = library.General.GetColor(colorIndex);
                CreateObstacle(pos, size, color);
                occupiedAreas.Add(new Rectangle((int)(pos.X - size.X / 2), (int)(pos.Y - size.Y / 2), (int)size.X, (int)size.Y));
            }
        }

        // Gameplay Systems
        _scheduler.AddSystem(new SlingshotInputSystem());

        var physicsStepper = new PhysicsStepperSystem(order: 5, substeps: 8);
        physicsStepper.AddSystem(new DemoRotationSystem());
        physicsStepper.AddSystem(new SpringSystem());
        physicsStepper.AddSystem(new PolygonUpdateSystem());
        physicsStepper.AddSystem(new MovementSystem());
        physicsStepper.AddSystem(new CollisionSystem());
        physicsStepper.AddSystem(new BoundarySystem());
        physicsStepper.AddSystem(new PhysicsFrictionSystem());
        physicsStepper.AddSystem(new ZoneSystem());

        _scheduler.AddSystem(physicsStepper);
        _scheduler.AddSystem(new ShapeRenderSystem());

        // UI Systems
        _uiScheduler.AddSystem(new UiInteractionSystem());
        _uiScheduler.AddSystem(new UiTextInputSystem());
        _uiScheduler.AddSystem(new UiActionSystem());
        _uiScheduler.AddSystem(new UiRenderSystem());

        base.Initialize();
    }

    private void CreateObstacle(Vector2 position, Vector2 size, Color color)
    {
        var obstacle = _world.CreateEntity();
        _world.SetComponent(obstacle, new ObstacleComponent());
        _world.SetComponent(obstacle, new TransformComponent { Position = position });
        _world.SetComponent(obstacle, new DrawColorComponent(color));
        _world.SetComponent(obstacle, new RigidBodyComponent
        {
            Shape = RigidBodyShape.Rectangle,
            Size = size,
            Restitution = 0.5f,
            Mass = 0f // Static
        });
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _debugPixel = new Texture2D(GraphicsDevice, 1, 1);
        _debugPixel.SetData(new[] { Color.White });

        var uiTheme = _world.GetRequiredResource<UiThemeResource>();
        uiTheme.Fonts["Fonts/Silkscreen"] = Content.Load<SpriteFont>("Fonts/Silkscreen");
        uiTheme.Fonts["Fonts/SilkscreenBold"] = Content.Load<SpriteFont>("Fonts/SilkscreenBold");
        uiTheme.Fonts["Fonts/Blanka"] = Content.Load<SpriteFont>("Fonts/Blanka");

        RebuildUi();
    }

    private void ClearUi()
    {
        foreach (var entity in _world.GetEntitiesWith<UiTransformComponent>().ToList())
        {
            _world.DestroyEntity(entity);
        }
    }

    private void RebuildUi()
    {
        ClearUi();
        var gameStateRes = _world.GetRequiredResource<GameStateResource>();
        var state = gameStateRes.Current;
        
        if (state == GameState.Menu)
        {
            CreateMainMenuUi();
        }
        else if (state == GameState.Settings)
        {
            CreateSettingsUi();
        }
        else if (state == GameState.GameOver)
        {
            CreateGameOverUi();
        }
    }

    private void CreateGameOverUi()
    {
        var centerX = GameConstants.DefaultWindowWidth / 2;
        var gameStateRes = _world.GetRequiredResource<GameStateResource>();
        
        UiBuilder.CreateLabel(_world, centerX - 250, 100, "HOLE FINISHED", "Fonts/Blanka", 2.5f, true);
        
        var strokesText = $"STROKES: {gameStateRes.Strokes}";
        var legendText = gameStateRes.GetStrokeLegend();
        
        UiBuilder.CreateLabel(_world, centerX - 150, 250, strokesText, "Fonts/SilkscreenBold", 1.5f, true);
        UiBuilder.CreateLabel(_world, centerX - 150, 320, legendText, "Fonts/SilkscreenBold", 2.0f, true);
        
        UiBuilder.CreateButton(_world, centerX - 150, 450, 300, 80, "MAIN MENU", "back_to_menu", "Fonts/SilkscreenBold");
    }

    private void CreateMainMenuUi()
    {
        var centerX = GameConstants.DefaultWindowWidth / 2;
        UiBuilder.CreateLabel(_world, centerX - 250, 100, "GOLFIN'", "Fonts/Blanka", 3.0f, true);
        
        UiBuilder.CreateButton(_world, centerX - 150, 350, 300, 80, "PLAY", "play", "Fonts/SilkscreenBold");
        UiBuilder.CreateButton(_world, centerX - 150, 450, 300, 80, "SETTINGS", "settings", "Fonts/SilkscreenBold");
    }

    private void CreateSettingsUi()
    {
        var centerX = GameConstants.DefaultWindowWidth / 2;
        UiBuilder.CreateLabel(_world, centerX - 150, 50, "SETTINGS", "Fonts/Blanka", 1.5f, true);

        // Volume Slider
        UiBuilder.CreateLabel(_world, centerX - 200, 150, "VOLUME", "Fonts/SilkscreenBold", 1.0f);
        _volumeSlider = UiBuilder.CreateSlider(_world, centerX - 200, 190, 400, 40, _settings.Volume);

        // Scale Selector
        UiBuilder.CreateLabel(_world, centerX - 200, 270, "RESOLUTION SCALE", "Fonts/SilkscreenBold", 1.0f);
        _scaleSelector = UiBuilder.CreateSelector(_world, centerX - 200, 310, 400, 50, "SCALE", _settings.ScaleOptions, _settings.ResolutionScaleIndex);

        // Fullscreen Checkbox
        _fullscreenCheckbox = UiBuilder.CreateCheckbox(_world, centerX - 200, 420, "FULLSCREEN MODE", _settings.IsFullScreen, 400);

        UiBuilder.CreateButton(_world, centerX - 150, 550, 300, 70, "BACK", "back_to_menu", "Fonts/SilkscreenBold");
    }

    protected override void Update(GameTime gameTime)
    {
        var currentKeyboard = Keyboard.GetState();
        var currentMouse = Mouse.GetState();

        var frameContext = new FrameContext(
            gameTime,
            currentKeyboard,
            _previousKeyboard,
            currentMouse,
            _previousMouse,
            GraphicsDevice.Viewport,
            _spriteBatch,
            _debugPixel);

        var gameStateResource = _world.GetRequiredResource<GameStateResource>();
        
        // Handle State Transitions
        if (gameStateResource.Current != _lastGameState)
        {
            RebuildUi();
            _lastGameState = gameStateResource.Current;
        }

        if (gameStateResource.Current == GameState.Playing)
        {
            _scheduler.Update(_world, frameContext);
        }
        
        _uiScheduler.Update(_world, frameContext);
        
        if (gameStateResource.Current == GameState.Settings)
        {
            UpdateSettingsFromUi();
        }

        if (_settings.NeedsApply)
        {
            ApplyGraphicsSettings();
        }

        _previousKeyboard = currentKeyboard;
        _previousMouse = currentMouse;

        base.Update(gameTime);
    }

    private void UpdateSettingsFromUi()
    {
        if (_world.TryGetComponent<UiSliderComponent>(_volumeSlider, out var volumeSlider))
        {
            if (Math.Abs(_settings.Volume - volumeSlider.Value) > 0.01f)
            {
                _settings.Volume = volumeSlider.Value;
            }
        }

        if (_world.TryGetComponent<UiSelectorComponent>(_scaleSelector, out var scaleSelector))
        {
            if (_settings.ResolutionScaleIndex != scaleSelector.SelectedIndex)
            {
                _settings.ResolutionScaleIndex = scaleSelector.SelectedIndex;
                _settings.NeedsApply = true;
            }
        }

        if (_world.TryGetComponent<UiCheckboxComponent>(_fullscreenCheckbox, out var fullscreenCheckbox))
        {
            if (_settings.IsFullScreen != fullscreenCheckbox.Checked)
            {
                _settings.IsFullScreen = fullscreenCheckbox.Checked;
                _settings.NeedsApply = true;
            }
        }
    }

    private void ApplyGraphicsSettings()
    {
        var res = _settings.GetResolution();
        _graphics.PreferredBackBufferWidth = res.Width;
        _graphics.PreferredBackBufferHeight = res.Height;
        _graphics.IsFullScreen = _settings.IsFullScreen;
        _graphics.ApplyChanges();

        // Calculate dynamic scales based on base resolution (1024x768)
        float scaleX = (float)res.Width / GameConstants.DefaultWindowWidth;
        float scaleY = (float)res.Height / GameConstants.DefaultWindowHeight;
        float minScale = Math.Min(scaleX, scaleY);

        // Update UI Global Scale
        var uiTheme = _world.GetRequiredResource<UiThemeResource>();
        uiTheme.GlobalScale = minScale;

        // Update Camera Zoom for Gameplay
        var camera = _world.GetRequiredResource<CameraResource>();
        camera.Zoom = minScale;
        camera.Position = new Vector2(GameConstants.DefaultWindowWidth / 2f, GameConstants.DefaultWindowHeight / 2f);

        // Keep PlayArea in base coordinates (Camera handles scaling)
        _world.SetResource(new MapBoundsResource
        {
            PlayArea = new Rectangle(0, 0, GameConstants.DefaultWindowWidth, GameConstants.DefaultWindowHeight)
        });

        _settings.NeedsApply = false;
        
        // Rebuild UI to center things correctly for new resolution
        RebuildUi();
    }

    protected override void Draw(GameTime gameTime)
    {
        var backgroundColor = new Color(0x2d, 0x4a, 0x1b); // Cozy Root BG
        if (_world.TryGetResource<PaletteLibraryResource>(out var library) && library != null)
        {
            backgroundColor = library.Cozy.GetColor(0); 
        }

        GraphicsDevice.Clear(backgroundColor);

        var frameContext = new FrameContext(
            gameTime,
            Keyboard.GetState(),
            _previousKeyboard,
            Mouse.GetState(),
            _previousMouse,
            GraphicsDevice.Viewport,
            _spriteBatch,
            _debugPixel);

        var gameState = _world.GetRequiredResource<GameStateResource>();
        if (gameState.Current == GameState.Playing)
        {
            var camera = _world.GetRequiredResource<CameraResource>();
            _spriteBatch?.Begin(transformMatrix: camera.GetViewMatrix(GraphicsDevice.Viewport));
            _scheduler.Draw(_world, frameContext);
            _spriteBatch?.End();
        }
        else
        {
            // UI is rendered in screen space, but our UI system now handles its own internal scaling
            _spriteBatch?.Begin();
            _uiScheduler.Draw(_world, frameContext);
            _spriteBatch?.End();
        }

        base.Draw(gameTime);
    }
}
