using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Physics.Systems;
using GameEngineLab.Core.Features.Rendering.Components;
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
            PlayArea = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight)
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
        _world.SetResource(new UiThemeResource
        {
            BorderColor = library.Cozy.GetColor(6),
            SurfaceColor = library.Cozy.GetColor(1),
            TextColor = library.Cozy.GetColor(4),
            HighlightColor = library.Cozy.GetColor(5),
            ShadowColor = library.Cozy.GetColor(3)
        });

        // Create the heavy ball with a spring
        var ball = _world.CreateEntity();
        _world.SetComponent(ball, new BallComponent());
        _world.SetComponent(ball, new RigidBodyComponent 
        { 
            Shape = RigidBodyShape.Circle,
            BoundingRadius = 12f, 
            Restitution = 0.6f, 
            Friction = 0.99f,
            Mass = 5.0f // Heavy ball
        });
        _world.SetComponent(ball, new TransformComponent { Position = new Vector2(512, 600) });
        _world.SetComponent(ball, new VelocityComponent { Value = Vector2.Zero });
        _world.SetComponent(ball, new DrawColorComponent(library.Specific.GetColor(1))); // Use a specific color for the ball
        _world.SetComponent(ball, new SpringComponent
        {
            Anchor = new Vector2(512, 600),
            Stiffness = 30f,
            Damping = 2f,
            RestLength = 0f
        });

        // Create some obstacles randomly
        var random = new Random();
        for (int i = 0; i < 15; i++)
        {
            var pos = new Vector2(random.Next(100, 900), random.Next(100, 500));
            var size = new Vector2(random.Next(40, 150), random.Next(20, 60));
            var colorIndex = random.Next(library.General.Colors.Count);
            var color = library.General.GetColor(colorIndex);
            
            CreateObstacle(pos, size, color);
        }

        // Gameplay Systems
        _scheduler.AddSystem(new SlingshotInputSystem());
        _scheduler.AddSystem(new SpringSystem());
        _scheduler.AddSystem(new MovementSystem());
        _scheduler.AddSystem(new CollisionSystem());
        _scheduler.AddSystem(new BoundarySystem());
        _scheduler.AddSystem(new PhysicsFrictionSystem());
        _scheduler.AddSystem(new BallRenderSystem());

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
        var state = _world.GetRequiredResource<GameStateResource>().Current;
        
        if (state == GameState.Menu)
        {
            CreateMainMenuUi();
        }
        else if (state == GameState.Settings)
        {
            CreateSettingsUi();
        }
    }

    private void CreateMainMenuUi()
    {
        var centerX = GraphicsDevice.Viewport.Width / 2;
        UiBuilder.CreateLabel(_world, centerX - 150, 100, "GOLFIN'", "Fonts/Blanka", 2.0f, true);
        
        UiBuilder.CreateButton(_world, centerX - 100, 300, 200, 60, "PLAY", "play", "Fonts/SilkscreenBold");
        UiBuilder.CreateButton(_world, centerX - 100, 400, 200, 60, "SETTINGS", "settings", "Fonts/SilkscreenBold");
    }

    private void CreateSettingsUi()
    {
        var centerX = GraphicsDevice.Viewport.Width / 2;
        UiBuilder.CreateLabel(_world, centerX - 100, 50, "SETTINGS", "Fonts/Blanka", 1.2f, true);

        // Volume Slider
        UiBuilder.CreateLabel(_world, centerX - 150, 150, "VOLUME", "Fonts/SilkscreenBold", 0.6f);
        _volumeSlider = UiBuilder.CreateSlider(_world, centerX - 150, 180, 300, 30, _settings.Volume);

        // Scale Selector
        UiBuilder.CreateLabel(_world, centerX - 150, 250, "RESOLUTION SCALE", "Fonts/SilkscreenBold", 0.6f);
        _scaleSelector = UiBuilder.CreateSelector(_world, centerX - 150, 280, 300, 40, "SCALE", _settings.ScaleOptions, _settings.ResolutionScaleIndex);

        // Fullscreen Checkbox
        _fullscreenCheckbox = UiBuilder.CreateCheckbox(_world, centerX - 150, 360, "FULLSCREEN MODE", _settings.IsFullScreen, 300);

        UiBuilder.CreateButton(_world, centerX - 100, 500, 200, 50, "BACK", "back_to_menu", "Fonts/SilkscreenBold");
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
        else
        {
            _uiScheduler.Update(_world, frameContext);
            
            if (gameStateResource.Current == GameState.Settings)
            {
                UpdateSettingsFromUi();
            }
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

        // Update MapBoundsResource to match new resolution
        _world.SetResource(new MapBoundsResource
        {
            PlayArea = new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight)
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

        _spriteBatch?.Begin();
        
        var gameState = _world.GetRequiredResource<GameStateResource>();
        if (gameState.Current == GameState.Playing)
        {
            _scheduler.Draw(_world, frameContext);
        }
        else
        {
            _uiScheduler.Draw(_world, frameContext);
        }

        _spriteBatch?.End();

        base.Draw(gameTime);
    }
}
