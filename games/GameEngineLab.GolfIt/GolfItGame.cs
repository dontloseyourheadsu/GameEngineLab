using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Physics.Systems;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Runtime.Resources;
using GameEngineLab.Core.Features.UI;
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

    public GolfItGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
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

        // Create Menu UI based on HTML design
        UiBuilder.CreateLabel(_world, 350, 40, "GOLF IT", "Fonts/Blanka", 1.2f, true);
        UiBuilder.CreateLabel(_world, 360, 90, "UI COMPONENT SHOWCASE - COZY TONES", "Fonts/Silkscreen", 0.6f);

        // Grid Layout simulation using Cards (Panels)
        
        // TYPOGRAPHY CARD
        UiBuilder.CreatePanel(_world, 50, 150, 300, 200);
        UiBuilder.CreateLabel(_world, 60, 160, "TYPOGRAPHY", "Fonts/SilkscreenBold", 0.6f);
        UiBuilder.CreateLabel(_world, 60, 190, "GOLF IT", "Fonts/SilkscreenBold", 1.1f, true);
        UiBuilder.CreateLabel(_world, 60, 230, "COURSE SELECT", "Fonts/SilkscreenBold", 0.8f);
        UiBuilder.CreateLabel(_world, 60, 260, "HOLE 7 / 18", "Fonts/Silkscreen", 0.7f);

        // BUTTONS CARD
        UiBuilder.CreatePanel(_world, 365, 150, 300, 200);
        UiBuilder.CreateLabel(_world, 375, 160, "TEXT BUTTONS", "Fonts/SilkscreenBold", 0.6f);
        UiBuilder.CreateButton(_world, 375, 190, 200, 50, "PLAY GAME", "play", "Fonts/SilkscreenBold");
        UiBuilder.CreateButton(_world, 375, 260, 120, 40, "CONFIRM", "none");
        UiBuilder.CreateButton(_world, 505, 260, 120, 40, "CANCEL", "none");

        // CHECKBOXES CARD
        UiBuilder.CreatePanel(_world, 680, 150, 300, 200);
        UiBuilder.CreateLabel(_world, 690, 160, "CHECKBOXES", "Fonts/SilkscreenBold", 0.6f);
        UiBuilder.CreateCheckbox(_world, 690, 190, "ENABLE SHADOWS", true, 250);
        UiBuilder.CreateCheckbox(_world, 690, 230, "SHOW TRAJECTORY", true, 250);
        UiBuilder.CreateCheckbox(_world, 690, 270, "WIND EFFECTS", false, 250);

        // INPUTS CARD
        UiBuilder.CreatePanel(_world, 50, 380, 300, 200);
        UiBuilder.CreateLabel(_world, 60, 390, "INPUTS", "Fonts/SilkscreenBold", 0.6f);
        UiBuilder.CreateLabel(_world, 60, 420, "PLAYER NAME", "Fonts/Silkscreen", 0.6f);
        UiBuilder.CreateTextInput(_world, 60, 440, 250, 40, "PLAYER 1");
        UiBuilder.CreateLabel(_world, 60, 490, "COURSE CODE", "Fonts/Silkscreen", 0.6f);
        UiBuilder.CreateTextInput(_world, 60, 510, 250, 40, "XXXX-XXXX");

        // SLIDERS CARD
        UiBuilder.CreatePanel(_world, 365, 380, 300, 200);
        UiBuilder.CreateLabel(_world, 375, 390, "SLIDERS", "Fonts/SilkscreenBold", 0.6f);
        UiBuilder.CreateLabel(_world, 375, 420, "WIND SPEED", "Fonts/Silkscreen", 0.6f);
        UiBuilder.CreateSlider(_world, 375, 440, 250, 20, 0.4f);
        UiBuilder.CreateLabel(_world, 375, 490, "SHOT POWER", "Fonts/Silkscreen", 0.6f);
        UiBuilder.CreateSlider(_world, 375, 510, 250, 20, 0.75f);
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

        var gameState = _world.GetRequiredResource<GameStateResource>();

        if (gameState.Current == GameState.Playing)
        {
            _scheduler.Update(_world, frameContext);
        }
        else
        {
            _uiScheduler.Update(_world, frameContext);
        }

        _previousKeyboard = currentKeyboard;
        _previousMouse = currentMouse;

        base.Update(gameTime);
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
