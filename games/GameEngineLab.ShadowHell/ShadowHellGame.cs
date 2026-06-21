using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Physics.Systems;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.Core.Features.Rendering.Systems;
using GameEngineLab.Core.Features.Animation.Systems;
using GameEngineLab.ShadowHell.Features.Player.Entities;
using GameEngineLab.ShadowHell.Features.Player.Systems;
using GameEngineLab.ShadowHell.Features.Environment.Systems;
using GameEngineLab.ShadowHell.Features.Environment.Components;
using GameEngineLab.ShadowHell.Features.Enemy.Systems;
using GameEngineLab.ShadowHell.Features.Enemy.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace GameEngineLab.ShadowHell;

public sealed class ShadowHellGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly World _world = new();
    private readonly SystemScheduler _scheduler = new();

    private SpriteBatch? _spriteBatch;
    private Texture2D? _debugPixel;
    private SpriteFont? _font;
    
    private KeyboardState _previousKeyboard;
    private MouseState _previousMouse;
    
    private EntityId _playerEntity;

    private const int WindowWidth = 1024;
    private const int WindowHeight = 768;
    private const float WorldWidth = 2048f;
    private const float WorldHeight = 1536f;

    public ShadowHellGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = "ShadowHell - Android Prototype Testbed";
        
        _graphics.PreferredBackBufferWidth = WindowWidth;
        _graphics.PreferredBackBufferHeight = WindowHeight;
    }

    protected override void Initialize()
    {
        // 1. Initialize Core Resources
        _world.SetResource(new MapBoundsResource
        {
            PlayArea = new Rectangle(0, 0, (int)WorldWidth, (int)WorldHeight)
        });

        _world.SetResource(new CameraResource
        {
            Position = new Vector2(WorldWidth / 2f, WorldHeight / 2f),
            Zoom = 1.0f
        });

        // 2. Setup ECS Schedulers
        // Floor Renderer (runs first under everything)
        _scheduler.AddSystem(new FloorRendererSystem());

        // Shadow Projection and Color Lighting System
        _scheduler.AddSystem(new ShadowSystem());

        // Player input and movement translations
        _scheduler.AddSystem(new PlayerInputSystem());
        _scheduler.AddSystem(new PlayerRendererSystem());

        // Physics Sub-stepping (Movement & Collision physics)
        var physicsStepper = new PhysicsStepperSystem(order: 10, substeps: 8);
        physicsStepper.AddSystem(new MovementSystem());
        physicsStepper.AddSystem(new CollisionSystem());
        physicsStepper.AddSystem(new BoundarySystem());
        physicsStepper.AddSystem(new PhysicsFrictionSystem());
        _scheduler.AddSystem(physicsStepper);

        // Core Skeletal Solver and Render System (kept for core compatibility)
        _scheduler.AddSystem(new SkeletonSystem());

        // Shape renderer for solid walls/caverns/pillars (pure black silhouette foreground)
        _scheduler.AddSystem(new ShapeRenderSystem());

        // Fortress brick highlights and boundary cracks
        _scheduler.AddSystem(new FortressRendererSystem());

        // Enemy steering AI and crawl rendering
        _scheduler.AddSystem(new EnemySystem());

        // 3. Generate Level (Cavern boundary & rocky pillars)
        GenerateCavernLevel();

        base.Initialize();
    }

    private void GenerateCavernLevel()
    {
        // Place Player in the center of the world map
        _playerEntity = PlayerFactory.CreatePlayer(_world, new Vector2(WorldWidth / 2f, WorldHeight / 2f));

        var random = new Random();

        // Create jagged borders (overlapping black circle rocks)
        // Top and Bottom rocky boundaries
        for (float x = 0; x <= WorldWidth + 50f; x += 60f)
        {
            // Top rock
            var topRock = _world.CreateEntity();
            float topRadius = random.Next(40, 80);
            float topY = random.Next(-20, 20);
            _world.SetComponent(topRock, new TransformComponent { Position = new Vector2(x, topY) });
            _world.SetComponent(topRock, new DrawColorComponent(new Color(78, 30, 32))); // Netherrack red
            _world.SetComponent(topRock, new RigidBodyComponent
            {
                Shape = RigidBodyShape.Circle,
                BoundingRadius = topRadius,
                Mass = 0f, // Static
                CollisionGroup = 1 // Cavern boundaries
            });

            // Bottom rock
            var botRock = _world.CreateEntity();
            float botRadius = random.Next(40, 80);
            float botY = WorldHeight + random.Next(-20, 20);
            _world.SetComponent(botRock, new TransformComponent { Position = new Vector2(x, botY) });
            _world.SetComponent(botRock, new DrawColorComponent(new Color(78, 30, 32)));
            _world.SetComponent(botRock, new RigidBodyComponent
            {
                Shape = RigidBodyShape.Circle,
                BoundingRadius = botRadius,
                Mass = 0f,
                CollisionGroup = 1
            });
        }

        // Left and Right rocky boundaries
        for (float y = 0; y <= WorldHeight + 50f; y += 60f)
        {
            // Left rock
            var leftRock = _world.CreateEntity();
            float leftRadius = random.Next(40, 80);
            float leftX = random.Next(-20, 20);
            _world.SetComponent(leftRock, new TransformComponent { Position = new Vector2(leftX, y) });
            _world.SetComponent(leftRock, new DrawColorComponent(new Color(78, 30, 32))); // Netherrack red
            _world.SetComponent(leftRock, new RigidBodyComponent
            {
                Shape = RigidBodyShape.Circle,
                BoundingRadius = leftRadius,
                Mass = 0f,
                CollisionGroup = 1
            });

            // Right rock
            var rightRock = _world.CreateEntity();
            float rightRadius = random.Next(40, 80);
            float rightX = WorldWidth + random.Next(-20, 20);
            _world.SetComponent(rightRock, new TransformComponent { Position = new Vector2(rightX, y) });
            _world.SetComponent(rightRock, new DrawColorComponent(new Color(78, 30, 32)));
            _world.SetComponent(rightRock, new RigidBodyComponent
            {
                Shape = RigidBodyShape.Circle,
                BoundingRadius = rightRadius,
                Mass = 0f,
                CollisionGroup = 1
            });
        }

        // Place random rocky pillars inside the room (foreground objects)
        for (int i = 0; i < 15; i++)
        {
            Vector2 position;
            bool ok;
            int attempts = 0;

            do
            {
                position = new Vector2(random.Next(200, (int)WorldWidth - 200), random.Next(200, (int)WorldHeight - 200));
                // Ensure away from player starting center
                ok = Vector2.Distance(position, new Vector2(WorldWidth / 2f, WorldHeight / 2f)) > 250f;
                attempts++;
            } while (!ok && attempts < 100);

            if (ok)
            {
                var pillar = _world.CreateEntity();
                float radius = random.Next(45, 90);
                
                _world.SetComponent(pillar, new TransformComponent { Position = position });
                _world.SetComponent(pillar, new DrawColorComponent(new Color(61, 33, 38))); // Nether Fortress Brick purple-red
                
                // 50% chance circle, 50% rectangle pillar
                if (random.Next(0, 2) == 0)
                {
                    _world.SetComponent(pillar, new RigidBodyComponent
                    {
                        Shape = RigidBodyShape.Circle,
                        BoundingRadius = radius,
                        Mass = 0f, // Static
                        CollisionGroup = 8 // Cavern interior pillars (casts shadows!)
                    });
                }
                else
                {
                    float width = random.Next(80, 160);
                    float height = random.Next(80, 160);
                    _world.SetComponent(pillar, new RigidBodyComponent
                    {
                        Shape = RigidBodyShape.Rectangle,
                        Size = new Vector2(width, height),
                        Mass = 0f,
                        CollisionGroup = 8
                    });
                }
            }
        }

        // Spawn shadow enemies
        for (int i = 0; i < 6; i++)
        {
            Vector2 position;
            bool ok;
            int attempts = 0;
            do
            {
                position = new Vector2(random.Next(150, (int)WorldWidth - 150), random.Next(150, (int)WorldHeight - 150));
                ok = Vector2.Distance(position, new Vector2(WorldWidth / 2f, WorldHeight / 2f)) > 300f;
                attempts++;
            } while (!ok && attempts < 100);

            var enemy = _world.CreateEntity();
            _world.SetComponent(enemy, new EnemyComponent());
            _world.SetComponent(enemy, new TransformComponent { Position = position });
            _world.SetComponent(enemy, new VelocityComponent { Value = Vector2.Zero });
            _world.SetComponent(enemy, new DrawColorComponent(Color.Black));
            _world.SetComponent(enemy, new RigidBodyComponent
            {
                Shape = RigidBodyShape.Circle,
                BoundingRadius = 20f,
                Mass = 1.2f,
                Restitution = 0.3f,
                Friction = 0.9f,
                CollisionGroup = 2, // Shadow enemies (casts shadows!)
                CollisionMask = 1 | 4 // collides with walls and player
            });
        }

        // Spawn 3 static warm light sources corresponding to the lava pools
        // Pool 1: (500, 1000)
        var lavaLight1 = _world.CreateEntity();
        _world.SetComponent(lavaLight1, new TransformComponent { Position = new Vector2(500, 1000) });
        _world.SetComponent(lavaLight1, new LightSourceComponent(new Color(255, 90, 0), 250f, 0.8f));

        // Pool 2: (1500, 600)
        var lavaLight2 = _world.CreateEntity();
        _world.SetComponent(lavaLight2, new TransformComponent { Position = new Vector2(1500, 600) });
        _world.SetComponent(lavaLight2, new LightSourceComponent(new Color(255, 120, 0), 280f, 0.8f));

        // Pool 3: (1000, 1200)
        var lavaLight3 = _world.CreateEntity();
        _world.SetComponent(lavaLight3, new TransformComponent { Position = new Vector2(1000, 1200) });
        _world.SetComponent(lavaLight3, new LightSourceComponent(new Color(255, 75, 0), 300f, 0.75f));
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Single pixel white texture used by shape drawers
        _debugPixel = new Texture2D(GraphicsDevice, 1, 1);
        _debugPixel.SetData(new[] { Color.White });

        // Load built-in font from Content
        _font = Content.Load<SpriteFont>("Fonts/Silkscreen");
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

        // Update systems
        _scheduler.Update(_world, frameContext);

        // Camera follow player smoothly
        if (_world.TryGetResource<CameraResource>(out var camera) && camera != null)
        {
            if (_world.TryGetComponent<TransformComponent>(_playerEntity, out var playerTransform))
            {
                // Smooth interpolation to center player in screen
                Vector2 targetCamPos = playerTransform.Position;
                // Clamp camera within world bounds
                targetCamPos.X = MathHelper.Clamp(targetCamPos.X, WindowWidth / 2f, WorldWidth - WindowWidth / 2f);
                targetCamPos.Y = MathHelper.Clamp(targetCamPos.Y, WindowHeight / 2f, WorldHeight - WindowHeight / 2f);

                camera.Position = Vector2.Lerp(camera.Position, targetCamPos, 0.1f);
            }
        }

        _previousKeyboard = currentKeyboard;
        _previousMouse = currentMouse;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Hell oppressive dark background tint (Very dark ash brown-black)
        GraphicsDevice.Clear(new Color(24, 12, 14));

        var frameContext = new FrameContext(
            gameTime,
            Keyboard.GetState(),
            _previousKeyboard,
            Mouse.GetState(),
            _previousMouse,
            GraphicsDevice.Viewport,
            _spriteBatch,
            _debugPixel);

        // Render World Space objects using camera matrix
        if (_world.TryGetResource<CameraResource>(out var camera) && camera != null && _spriteBatch != null)
        {
            // Begin with camera view matrix
            _spriteBatch.Begin(
                SpriteSortMode.Deferred, 
                BlendState.AlphaBlend, 
                SamplerState.PointClamp, 
                null, 
                null, 
                null, 
                camera.GetViewMatrix(GraphicsDevice.Viewport));
            
            _scheduler.Draw(_world, frameContext);
            
            _spriteBatch.End();
        }

        // Render UI / HUD Overlay (Screen Space)
        if (_spriteBatch != null && _debugPixel != null)
        {
            _spriteBatch.Begin();

            /*
            // 1. Celeste-style screen overlay filter (semi-transparent glowing heat wash - optimized to be subtle)
            ShapeRenderer.DrawRectangle(
                _spriteBatch, 
                _debugPixel, 
                new Vector2(WindowWidth / 2f, WindowHeight / 2f), 
                new Vector2(WindowWidth, WindowHeight), 
                new Color(255, 60, 0, 8) // subtle warm glow (approx 3% opacity)
            );

            // 2. Rising heat gradient fog at the bottom of the screen (magma glow - faint and clean)
            ShapeRenderer.DrawRectangle(_spriteBatch, _debugPixel, new Vector2(WindowWidth / 2f, WindowHeight - 25), new Vector2(WindowWidth, 50), new Color(255, 80, 0, 25));
            ShapeRenderer.DrawRectangle(_spriteBatch, _debugPixel, new Vector2(WindowWidth / 2f, WindowHeight - 65), new Vector2(WindowWidth, 30), new Color(255, 60, 0, 15));
            ShapeRenderer.DrawRectangle(_spriteBatch, _debugPixel, new Vector2(WindowWidth / 2f, WindowHeight - 100), new Vector2(WindowWidth, 40), new Color(255, 45, 0, 8));
            ShapeRenderer.DrawRectangle(_spriteBatch, _debugPixel, new Vector2(WindowWidth / 2f, WindowHeight - 140), new Vector2(WindowWidth, 40), new Color(255, 30, 0, 4));
            */

            // Draw HUD Title
            if (_font != null)
            {
                // Title shadow
                _spriteBatch.DrawString(_font, "SHADOWHELL", new Vector2(32, 22), Color.DarkRed * 0.5f, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);
                _spriteBatch.DrawString(_font, "SHADOWHELL", new Vector2(30, 20), Color.Red, 0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);

                // Subtitle
                _spriteBatch.DrawString(_font, "Android Roguelike Techbed - testing build", new Vector2(30, 60), Color.Gray, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);

                // Instructions
                string controls = "CONTROLS:\nWASD / Arrows - Move\nSpace - Procedural Jump\nLeft Shift - Dodge Roll";
                _spriteBatch.DrawString(_font, controls, new Vector2(30, WindowHeight - 100), Color.LightGray * 0.8f, 0f, Vector2.Zero, 0.8f, SpriteEffects.None, 0f);
            }

            _spriteBatch.End();
        }

        base.Draw(gameTime);
    }
}
