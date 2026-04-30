using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Physics.Systems;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Runtime.Resources;
using GameEngineLab.GolfIt.Features.Ball.Components;
using GameEngineLab.GolfIt.Features.Input.Systems;
using GameEngineLab.GolfIt.Features.Physics.Components;
using GameEngineLab.GolfIt.Features.Physics.Systems;
using GameEngineLab.GolfIt.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Rendering.Systems;
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
            Specific = SpecificAssetsPalette.Create()
        };
        _world.SetResource(library);

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

        _scheduler.AddSystem(new SlingshotInputSystem());
        _scheduler.AddSystem(new SpringSystem());
        _scheduler.AddSystem(new MovementSystem());
        _scheduler.AddSystem(new CollisionSystem());
        _scheduler.AddSystem(new BoundarySystem());
        _scheduler.AddSystem(new PhysicsFrictionSystem());
        _scheduler.AddSystem(new BallRenderSystem());

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

        _scheduler.Update(_world, frameContext);

        _previousKeyboard = currentKeyboard;
        _previousMouse = currentMouse;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var backgroundColor = Color.CornflowerBlue;
        if (_world.TryGetResource<PaletteLibraryResource>(out var library) && library != null)
        {
            backgroundColor = library.Specific.GetColor(7); // The darkest color from the specific palette
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
        _scheduler.Draw(_world, frameContext);
        _spriteBatch?.End();

        base.Draw(gameTime);
    }
}
