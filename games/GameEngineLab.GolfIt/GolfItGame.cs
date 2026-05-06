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
using GameEngineLab.GolfIt.Features.Maps;
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
    private EntityId? _lastSelectedEntity;
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
        _world.SetResource(new MapEditorStateResource());
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

        // Gameplay Systems
        _scheduler.AddSystem(new SlingshotInputSystem());
        _scheduler.AddSystem(new MapEditorSystem());

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
        _uiScheduler.AddSystem(new EditorItemRenderSystem());

        base.Initialize();
    }

    public void LoadDemoLevel()
    {
        var library = _world.GetRequiredResource<PaletteLibraryResource>();
        
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
    }

    public void ClearGameplayEntities()
    {
        // Clear obstacles, balls, goals, etc.
        foreach (var entity in _world.GetEntitiesWith<ObstacleComponent>().ToList()) _world.DestroyEntity(entity);
        foreach (var entity in _world.GetEntitiesWith<BallComponent>().ToList()) _world.DestroyEntity(entity);
        foreach (var entity in _world.GetEntitiesWith<TriggerZoneComponent>().ToList()) _world.DestroyEntity(entity);
        foreach (var entity in _world.GetEntitiesWith<EditorObjectComponent>().ToList())
        {
            if (!_world.HasComponent<TemplateComponent>(entity))
                _world.DestroyEntity(entity);
        }
        foreach (var entity in _world.GetEntitiesWith<SoftBodyNodeComponent>().ToList()) _world.DestroyEntity(entity);
        foreach (var entity in _world.GetEntitiesWith<DistanceSpringComponent>().ToList()) _world.DestroyEntity(entity);
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
        foreach (var entity in _world.GetEntitiesWith<TemplateComponent>().ToList())
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
        else if (state == GameState.MapEditorList)
        {
            CreateMapEditorListUi();
        }
        else if (state == GameState.MapEditor)
        {
            CreateMapEditorUi();
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
        
        UiBuilder.CreateButton(_world, centerX - 150, 300, 300, 80, "PLAY", "play", "Fonts/SilkscreenBold");
        UiBuilder.CreateButton(_world, centerX - 150, 400, 300, 80, "MAP EDITOR", "map_editor_list", "Fonts/SilkscreenBold");
        UiBuilder.CreateButton(_world, centerX - 150, 500, 300, 80, "SETTINGS", "settings", "Fonts/SilkscreenBold");
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
        var mapStateResource = _world.GetRequiredResource<MapEditorStateResource>();
        
        // Handle State Transitions or Selection changes
        if (gameStateResource.Current != _lastGameState || mapStateResource.SelectedEntity != _lastSelectedEntity)
        {
            RebuildUi();
            _lastGameState = gameStateResource.Current;
            _lastSelectedEntity = mapStateResource.SelectedEntity;
        }

        if (gameStateResource.Current == GameState.Playing || gameStateResource.Current == GameState.MapEditor)
        {
            _scheduler.Update(_world, frameContext);
        }
        
        _uiScheduler.Update(_world, frameContext);
        
        if (gameStateResource.Current == GameState.Settings)
        {
            UpdateSettingsFromUi();
        }
        
        if (gameStateResource.Current == GameState.MapEditorList)
        {
            UpdateMapEditorListFromUi();
        }

        if (gameStateResource.Current == GameState.MapEditor)
        {
            UpdateMapEditorPropertiesFromUi();
        }

        if (_settings.NeedsApply)
        {
            ApplyGraphicsSettings();
        }

        _previousKeyboard = currentKeyboard;
        _previousMouse = currentMouse;

        base.Update(gameTime);
    }

    private void UpdateMapEditorPropertiesFromUi()
    {
        var mapState = _world.GetRequiredResource<MapEditorStateResource>();
        if (!mapState.SelectedEntity.HasValue || !_world.IsAlive(mapState.SelectedEntity.Value)) return;

        var selected = mapState.SelectedEntity.Value;

        // Apply COLOR
        if (_world.TryGetComponent<UiSelectorComponent>(mapState.ColorSelectorId, out var colorSelector))
        {
            var colors = new[] { Color.White, Color.Gray, Color.Red, Color.Green, Color.Blue, Color.Black };
            if (colorSelector.SelectedIndex >= 0 && colorSelector.SelectedIndex < colors.Length)
            {
                _world.SetComponent(selected, new DrawColorComponent(colors[colorSelector.SelectedIndex]));
            }
        }

        // Apply SIZE/SCALE
        if (_world.TryGetComponent<UiSliderComponent>(mapState.SizeSliderId, out var sizeSlider))
        {
            if (_world.TryGetComponent<RigidBodyComponent>(selected, out var body))
            {
                var baseScale = 0.5f + sizeSlider.Value; // 0.5 to 1.5
                if (body.Shape == RigidBodyShape.Circle)
                {
                    _world.TryGetComponent<EditorObjectComponent>(selected, out var editorObj);
                    var baseRadius = editorObj.ToolType == EditorTool.Ball ? 16f : (editorObj.ToolType == EditorTool.Goal ? 40f : 30f);
                    body.BoundingRadius = baseRadius * baseScale;
                }
                else if (body.Shape == RigidBodyShape.Rectangle)
                {
                    body.Size = new Vector2(80, 40) * baseScale;
                }
                _world.SetComponent(selected, body);
            }
        }

        // Apply ROTATION
        if (_world.TryGetComponent<UiSliderComponent>(mapState.RotationSliderId, out var rotSlider))
        {
            if (_world.TryGetComponent<TransformComponent>(selected, out var transform))
            {
                transform.Rotation = rotSlider.Value * MathHelper.TwoPi;
                _world.SetComponent(selected, transform);
            }
        }
    }

    private void UpdateMapEditorListFromUi()
    {
        // Handle scrollbar updates if we had multiple pages/scrolling
    }

    private void CreateMapEditorListUi()
    {
        var centerX = GameConstants.DefaultWindowWidth / 2;
        var mapState = _world.GetRequiredResource<MapEditorStateResource>();
        
        UiBuilder.CreateLabel(_world, centerX - 150, 50, "MAP LIST", "Fonts/Blanka", 1.5f, true);
        
        UiBuilder.CreateButton(_world, centerX + 100, 110, 300, 60, "CREATE NEW", "create_new_map", "Fonts/SilkscreenBold");
        
        var listX = centerX - 450;
        var listY = 180;
        var listWidth = 900;
        var listHeight = 480;
        
        UiBuilder.CreatePanel(_world, listX, listY, listWidth, listHeight);
        
        // Show up to 4 maps for now (simplified scrolling)
        int startIdx = 0; 
        int count = Math.Min(4, mapState.Maps.Count);
        
        for (int i = 0; i < count; i++)
        {
            var map = mapState.Maps[startIdx + i];
            var itemY = listY + 20 + i * 110;
            
            // Item Background
            UiBuilder.CreatePanel(_world, listX + 20, itemY, listWidth - 40, 100);
            
            // Placeholder Preview
            var preview = UiBuilder.CreatePanel(_world, listX + 40, itemY + 10, 80, 80);
            _world.SetComponent(preview, new DrawColorComponent(Color.DarkGreen));

            // Map Name
            UiBuilder.CreateLabel(_world, listX + 140, itemY + 35, map.Name, "Fonts/SilkscreenBold", 1.2f);
            
            // Buttons
            UiBuilder.CreateButton(_world, listX + 550, itemY + 20, 150, 60, "EDIT", $"edit_map:{map.FilePath}", "Fonts/SilkscreenBold");
            UiBuilder.CreateButton(_world, listX + 720, itemY + 20, 150, 60, "DELETE", $"delete_map_req:{map.FilePath}", "Fonts/SilkscreenBold");
        }

        if (mapState.Maps.Count == 0)
        {
            UiBuilder.CreateLabel(_world, centerX - 100, listY + 200, "NO MAPS FOUND", "Fonts/Silkscreen", 1.0f);
        }

        UiBuilder.CreateButton(_world, centerX - 150, 680, 300, 60, "BACK", "back_to_menu", "Fonts/SilkscreenBold");
    }

    private void CreateMapEditorUi()
    {
        var mapState = _world.GetRequiredResource<MapEditorStateResource>();

        // 1. TOP TOOLBAR
        UiBuilder.CreatePanel(_world, 0, 0, GameConstants.DefaultWindowWidth, 60);
        UiBuilder.CreateLabel(_world, 20, 15, "MAP EDITOR", "Fonts/SilkscreenBold", 1.2f);
        UiBuilder.CreateButton(_world, 750, 5, 120, 50, "SAVE", "save_map", "Fonts/SilkscreenBold");
        UiBuilder.CreateButton(_world, 880, 5, 130, 50, "DISCARD", "discard_map_req", "Fonts/SilkscreenBold");

        // 2. LEFT SIDEBAR: PROPERTIES
        var leftWidth = 240;
        UiBuilder.CreatePanel(_world, 0, 60, leftWidth, GameConstants.DefaultWindowHeight - 60);
        UiBuilder.CreateLabel(_world, 20, 80, "PROPERTIES", "Fonts/SilkscreenBold", 1.0f);

        if (mapState.SelectedEntity.HasValue && _world.IsAlive(mapState.SelectedEntity.Value))
        {
            var selected = mapState.SelectedEntity.Value;
            _world.TryGetComponent<EditorObjectComponent>(selected, out var editorObj);
            _world.TryGetComponent<TransformComponent>(selected, out var transform);
            _world.TryGetComponent<RigidBodyComponent>(selected, out var body);
            _world.TryGetComponent<DrawColorComponent>(selected, out var drawColor);

            UiBuilder.CreateLabel(_world, 20, 120, $"TYPE: {editorObj.ToolType}", "Fonts/Silkscreen", 0.8f);

            // COLOR PICKER
            UiBuilder.CreateLabel(_world, 20, 160, "COLOR", "Fonts/Silkscreen", 0.8f);
            var colorOptions = new[] { "WHITE", "GRAY", "RED", "GREEN", "BLUE", "BLACK" };
            var colors = new[] { Color.White, Color.Gray, Color.Red, Color.Green, Color.Blue, Color.Black };
            int colorIdx = Array.IndexOf(colors, drawColor.Value);
            if (colorIdx < 0) colorIdx = 0;
            mapState.ColorSelectorId = UiBuilder.CreateSelector(_world, 20, 185, 200, 40, "COLOR", colorOptions, colorIdx);

            // SCALE/SIZE SLIDER
            UiBuilder.CreateLabel(_world, 20, 250, "SIZE", "Fonts/Silkscreen", 0.8f);
            var baseRadius = editorObj.ToolType == EditorTool.Ball ? 16f : (editorObj.ToolType == EditorTool.Goal ? 40f : 30f);
            var currentSize = body.Shape == RigidBodyShape.Circle ? body.BoundingRadius : body.Size.X;
            var baseRef = body.Shape == RigidBodyShape.Circle ? baseRadius : 80f;
            var initialScale = Math.Clamp((currentSize / baseRef) - 0.5f, 0, 1);
            mapState.SizeSliderId = UiBuilder.CreateSlider(_world, 20, 275, 200, 30, initialScale);

            // ROTATION SLIDER
            UiBuilder.CreateLabel(_world, 20, 330, "ROTATION", "Fonts/Silkscreen", 0.8f);
            mapState.RotationSliderId = UiBuilder.CreateSlider(_world, 20, 355, 200, 30, transform.Rotation / MathHelper.TwoPi);
            
            UiBuilder.CreateButton(_world, 20, 450, 200, 50, "DELETE", "delete_selected", "Fonts/SilkscreenBold");
        }
        else
        {
            UiBuilder.CreateLabel(_world, 20, 150, "SELECT AN ITEM\nTO EDIT", "Fonts/Silkscreen", 0.8f);
        }

        // 3. RIGHT SIDEBAR: DRAG & DROP ITEMS
        var rightWidth = 180;
        var rightX = GameConstants.DefaultWindowWidth - rightWidth;
        UiBuilder.CreatePanel(_world, rightX, 60, rightWidth, GameConstants.DefaultWindowHeight - 60);
        UiBuilder.CreateLabel(_world, rightX + 20, 80, "ITEMS", "Fonts/SilkscreenBold", 1.0f);

        // Render "Live" Items for dragging
        UiBuilder.CreateLabel(_world, rightX + 20, 110, "RECTANGLE", "Fonts/Silkscreen", 0.7f);
        CreateTemplate(rightX + 90, 160, EditorTool.Square, Color.Gray);
        
        UiBuilder.CreateLabel(_world, rightX + 20, 230, "SPHERE", "Fonts/Silkscreen", 0.7f);
        CreateTemplate(rightX + 90, 280, EditorTool.Circle, Color.Gray);
        
        UiBuilder.CreateLabel(_world, rightX + 20, 350, "BALL", "Fonts/Silkscreen", 0.7f);
        CreateTemplate(rightX + 90, 400, EditorTool.Ball, Color.White);
        
        UiBuilder.CreateLabel(_world, rightX + 20, 470, "GOAL", "Fonts/Silkscreen", 0.7f);
        CreateTemplate(rightX + 90, 520, EditorTool.Goal, Color.Black);
    }

    private void CreateTemplate(int x, int y, EditorTool tool, Color color)
    {
        var entityId = _world.CreateEntity();
        _world.SetComponent(entityId, new TemplateComponent());
        _world.SetComponent(entityId, new EditorObjectComponent { ToolType = tool });
        _world.SetComponent(entityId, new TransformComponent { Position = new Vector2(x, y) });
        _world.SetComponent(entityId, new DrawColorComponent(color));

        switch (tool)
        {
            case EditorTool.Square:
                _world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Rectangle, Size = new Vector2(60, 30), Mass = 0 });
                break;
            case EditorTool.Circle:
                _world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 25, Mass = 0 });
                break;
            case EditorTool.Ball:
                _world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 16, Mass = 5 });
                break;
            case EditorTool.Goal:
                _world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 30, Mass = 0 });
                break;
        }
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
        var mapState = _world.GetRequiredResource<MapEditorStateResource>();
        
        if (gameState.Current == GameState.Playing || gameState.Current == GameState.MapEditor)
        {
            var camera = _world.GetRequiredResource<CameraResource>();
            _spriteBatch?.Begin(transformMatrix: camera.GetViewMatrix(GraphicsDevice.Viewport));
            _scheduler.Draw(_world, frameContext);
            _spriteBatch?.End();
        }
        
        // UI is always screen space
        _spriteBatch?.Begin();
        _uiScheduler.Draw(_world, frameContext);
        _spriteBatch?.End();

        base.Draw(gameTime);
    }
}
