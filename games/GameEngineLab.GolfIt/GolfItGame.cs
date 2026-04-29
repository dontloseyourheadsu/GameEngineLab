using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Runtime.Resources;
using GameEngineLab.GolfIt.Features.Ball.Components;
using GameEngineLab.GolfIt.Features.Input.Systems;
using GameEngineLab.GolfIt.Features.Physics.Systems;
using GameEngineLab.GolfIt.Features.Rendering.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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

        var ball = _world.CreateEntity();
        _world.SetComponent(ball, new BallComponent());
        _world.SetComponent(ball, new TransformComponent { Position = new Vector2(400, 300) });
        _world.SetComponent(ball, new VelocityComponent { Value = Vector2.Zero });

        _scheduler.AddSystem(new SlingshotInputSystem());
        _scheduler.AddSystem(new MovementSystem());
        _scheduler.AddSystem(new BounceSystem());
        _scheduler.AddSystem(new FrictionSystem());
        _scheduler.AddSystem(new BallRenderSystem());

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
        GraphicsDevice.Clear(Color.CornflowerBlue);

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
