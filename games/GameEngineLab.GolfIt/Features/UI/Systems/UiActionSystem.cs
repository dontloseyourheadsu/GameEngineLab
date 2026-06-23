using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics;
using GameEngineLab.Core.Features.Physics.Resources;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.Core.Features.UI;
using GameEngineLab.Core.Features.UI.Components;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Runtime.Resources;
using GameEngineLab.Core.Features.Identity.Resources;
using GameEngineLab.Core.Features.Identity.Components;
using GameEngineLab.Core.Features.Online.Resources;
using GameEngineLab.GolfIt.Features.Ball.Components;
using GameEngineLab.GolfIt.Features.Maps;
using GameEngineLab.GolfIt.Features.Physics.Components;
using GameEngineLab.GolfIt.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.GolfIt.Features.UI.Systems;

public sealed class UiActionSystem : IGameSystem
{
    public int Order => 0;

    public void Update(World world, FrameContext frameContext)
    {
        // Handle Button Clicks
        if (frameContext.CurrentMouse.LeftButton == ButtonState.Released && 
            frameContext.PreviousMouse.LeftButton == ButtonState.Pressed)
        {
            foreach (var entityId in world.GetEntitiesWith<UiButtonComponent, UiStateComponent>())
            {
                // Respect Modal
                if (world.GetEntitiesWith<UiModalComponent>().Any() && !world.HasComponent<UiModalComponent>(entityId))
                    continue;

                world.TryGetComponent<UiStateComponent>(entityId, out var uiState);
                if (uiState.State == UiState.Hovered || uiState.State == UiState.Pressed)
                {
                    world.TryGetComponent<UiButtonComponent>(entityId, out var button);
                    HandleAction(world, button.ActionId);
                }
            }
        }

        // Handle Queued Actions (from Zones, etc.)
        if (world.TryGetResource<ActionQueueResource>(out var actionQueue) && actionQueue != null)
        {
            while (actionQueue.TryDequeue(out var actionId))
            {
                if (actionId != null)
                {
                    HandleAction(world, actionId);
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext) { }

    private void HandleAction(World world, string actionId)
    {
        if (!world.TryGetResource<GameStateResource>(out var state) || state == null) return;
        if (!world.TryGetResource<MapEditorStateResource>(out var mapState) || mapState == null) return;

        if (actionId == "play")
        {
            state.Current = GameState.Playing;
            state.Strokes = 0;
            StartPlaying(world);
        }
        else if (actionId == "settings")
        {
            state.Current = GameState.Settings;
        }
        else if (actionId == "multiplayer_lobby")
        {
            state.Current = GameState.MultiplayerLobby;
        }
        else if (actionId == "mp_host")
        {
            if (world.TryGetResource<NetworkResource>(out var net) && net != null)
            {
                net.State = NetworkState.ConnectedAsHost;
                net.Port = 7777;
                
                // Get the discovery service and start broadcasting
                if (world.TryGetResource<DiscoveryService>(out var ds) && ds != null)
                {
                    ds.StartBroadcasting();
                }
            }
        }
        else if (actionId == "mp_join_ip")
        {
            if (world.TryGetResource<NetworkResource>(out var net) && net != null)
            {
                // Find IP text input
                var textInputs = world.GetEntitiesWith<UiTextInputComponent>().ToList();
                string ipAddress = "127.0.0.1";
                foreach (var inputEntity in textInputs)
                {
                    world.TryGetComponent<UiTextInputComponent>(inputEntity, out var textInput);
                    if (world.TryGetResource<UserAccountResource>(out var account) && account != null)
                    {
                        if (textInput.Text != account.Username)
                        {
                            ipAddress = textInput.Text;
                            break;
                        }
                    }
                }

                net.RemoteAddress = ipAddress;
                net.Port = 7777;
                net.State = NetworkState.Connecting;
                
                // Send Handshake
                if (world.TryGetResource<UserAccountResource>(out var accountRes) && accountRes != null)
                {
                    var handshake = new NetworkPacket
                    {
                        Type = PacketType.Handshake,
                        SenderId = accountRes.UserId,
                        Payload = Encoding.UTF8.GetBytes(accountRes.Username)
                    };
                    net.OutgoingPackets.Enqueue(handshake);
                }
            }
        }
        else if (actionId.StartsWith("mp_join_lan:"))
        {
            var hostIp = actionId.Substring("mp_join_lan:".Length);
            if (world.TryGetResource<NetworkResource>(out var net) && net != null)
            {
                net.RemoteAddress = hostIp;
                net.Port = 7777;
                net.State = NetworkState.Connecting;
                
                // Send Handshake
                if (world.TryGetResource<UserAccountResource>(out var accountRes) && accountRes != null)
                {
                    var handshake = new NetworkPacket
                    {
                        Type = PacketType.Handshake,
                        SenderId = accountRes.UserId,
                        Payload = Encoding.UTF8.GetBytes(accountRes.Username)
                    };
                    net.OutgoingPackets.Enqueue(handshake);
                }
            }
        }
        else if (actionId == "mp_start_game")
        {
            if (world.TryGetResource<NetworkResource>(out var net) && net != null)
            {
                if (world.TryGetResource<UserAccountResource>(out var accountRes) && accountRes != null)
                {
                    var mapStateRes = world.GetRequiredResource<MapEditorStateResource>();
                    var mapPath = mapStateRes.SelectedMapPath ?? "";
                    
                    var packet = new NetworkPacket
                    {
                        Type = PacketType.Handshake,
                        SenderId = accountRes.UserId,
                        Payload = Encoding.UTF8.GetBytes($"START_GAME:{mapPath}")
                    };
                    net.OutgoingPackets.Enqueue(packet);
                }
            }
            state.Current = GameState.Playing;
            state.Strokes = 0;
            StartPlaying(world);
        }
        else if (actionId.StartsWith("mp_start_game_trigger"))
        {
            state.Current = GameState.Playing;
            state.Strokes = 0;
            
            // Check if there was a map path in the message
            string? mapPath = null;
            if (actionId.Contains(":"))
            {
                mapPath = actionId.Substring(actionId.IndexOf(':') + 1);
            }
            
            if (!string.IsNullOrEmpty(mapPath) && File.Exists(mapPath))
            {
                ClearEditorObjects(world);
                mapState.SelectedMapPath = mapPath;
                LoadMap(world, mapPath);
                StartPlaying(world);
            }
            else
            {
                StartPlaying(world);
            }
        }
        else if (actionId == "back_to_menu")
        {
            ClearAllGameplayEntities(world);
            // Disconnect if we leave multiplayer
            if (world.TryGetResource<NetworkResource>(out var net) && net != null)
            {
                net.State = NetworkState.Disconnected;
                net.RemotePlayerId = Guid.Empty;
                net.RemotePlayerName = "";
                net.RemotePlayerStrokes = 0;
            }
            state.Current = GameState.Menu;
        }
        else if (actionId == "goal")
        {
            state.Current = GameState.GameOver;
        }
        else if (actionId == "map_editor_list")
        {
            ClearAllGameplayEntities(world);
            RefreshMaps(mapState);
            state.Current = GameState.MapEditorList;
        }
        else if (actionId == "create_new_map")
        {
            ClearEditorObjects(world);
            mapState.SelectedMapPath = null;
            mapState.SelectedEntity = null;
            state.Current = GameState.MapEditor;
        }
        else if (actionId.StartsWith("edit_map:"))
        {
            ClearEditorObjects(world);
            mapState.SelectedMapPath = actionId.Substring("edit_map:".Length);
            mapState.SelectedEntity = null;
            state.Current = GameState.MapEditor;
            LoadMap(world, mapState.SelectedMapPath);
        }
        else if (actionId.StartsWith("delete_map_req:"))
        {
            mapState.MapPathToDelete = actionId.Substring("delete_map_req:".Length);
            UiBuilder.CreatePopup(world, GameConstants.DefaultWindowWidth / 2, GameConstants.DefaultWindowHeight / 2, 
                "DELETE MAP?", "Are you sure you want to delete this map?", "delete_map_confirm", "cancel_popup");
        }
        else if (actionId == "delete_map_confirm")
        {
            if (mapState.MapPathToDelete != null && File.Exists(mapState.MapPathToDelete))
            {
                File.Delete(mapState.MapPathToDelete);
            }
            mapState.MapPathToDelete = null;
            ClearModal(world);
            RefreshMaps(mapState);
            state.Current = GameState.MapEditorList;
        }
        else if (actionId == "cancel_popup")
        {
            ClearModal(world);
        }
        else if (actionId == "save_map")
        {
            SaveCurrentMap(world, mapState);
            state.Current = GameState.MapEditorList;
        }
        else if (actionId == "discard_map_req")
        {
            UiBuilder.CreatePopup(world, GameConstants.DefaultWindowWidth / 2, GameConstants.DefaultWindowHeight / 2, 
                "DISCARD CHANGES?", "Unsaved changes will be lost.", "discard_map_confirm", "cancel_popup");
        }
        else if (actionId == "discard_map_confirm")
        {
            ClearModal(world);
            state.Current = GameState.MapEditorList;
        }
        else if (actionId == "delete_selected")
        {
            if (mapState.SelectedEntity.HasValue && world.IsAlive(mapState.SelectedEntity.Value))
            {
                world.DestroyEntity(mapState.SelectedEntity.Value);
                mapState.SelectedEntity = null;
            }
        }
        else if (actionId.StartsWith("tool_"))
        {
            if (world.TryGetResource<EditorContextResource>(out var editor) && editor != null)
            {
                editor.SelectedTool = actionId switch
                {
                    "tool_square" => EditorTool.Square,
                    "tool_circle" => EditorTool.Circle,
                    "tool_triangle" => EditorTool.Triangle,
                    "tool_softcircle" => EditorTool.SoftCircle,
                    "tool_ball" => EditorTool.Ball,
                    "tool_goal" => EditorTool.Goal,
                    _ => EditorTool.None
                };
            }
        }
    }

    private void StartPlaying(World world)
    {
        var editorObjects = world.GetEntitiesWith<EditorObjectComponent>().Where(e => !world.HasComponent<TemplateComponent>(e)).ToList();
        
        if (editorObjects.Count == 0)
        {
            LoadDemoLevel(world);
        }
        else
        {
            foreach (var editorId in editorObjects)
            {
                world.TryGetComponent<EditorObjectComponent>(editorId, out var obj);
                world.TryGetComponent<TransformComponent>(editorId, out var t);
                world.TryGetComponent<DrawColorComponent>(editorId, out var c);
                world.TryGetComponent<RigidBodyComponent>(editorId, out var b);

                // Hide the editor object during playback
                world.SetComponent(editorId, new HiddenComponent());

                // Create gameplay version
                switch (obj.ToolType)
                {
                    case EditorTool.Ball:
                        var ball = world.CreateEntity();
                        world.SetComponent(ball, new BallComponent());
                        world.SetComponent(ball, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = b.BoundingRadius, Restitution = 0.6f, Friction = 0.99f, Mass = 5.0f });
                        world.SetComponent(ball, new TransformComponent { Position = t.Position });
                        world.SetComponent(ball, new VelocityComponent { Value = Vector2.Zero });
                        world.SetComponent(ball, new DrawColorComponent(c.Value));
                        if (world.TryGetResource<UserAccountResource>(out var account) && account != null)
                        {
                            world.SetComponent(ball, new UserIdentityComponent { UserId = account.UserId, Username = account.Username });
                        }
                        break;
                    case EditorTool.Goal:
                        var goal = world.CreateEntity();
                        world.SetComponent(goal, new TriggerZoneComponent("goal"));
                        world.SetComponent(goal, new TransformComponent { Position = t.Position });
                        world.SetComponent(goal, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = b.BoundingRadius, Mass = 0f });
                        world.SetComponent(goal, new DrawColorComponent(c.Value));
                        break;
                    case EditorTool.Square:
                    case EditorTool.Circle:
                    case EditorTool.Triangle:
                        var obstacle = world.CreateEntity();
                        world.SetComponent(obstacle, new ObstacleComponent());
                        world.SetComponent(obstacle, new TransformComponent { Position = t.Position, Rotation = t.Rotation });
                        world.SetComponent(obstacle, new DrawColorComponent(c.Value));
                        
                        if (obj.ToolType == EditorTool.Triangle)
                        {
                            world.SetComponent(obstacle, new RigidBodyComponent { Shape = RigidBodyShape.Polygon, Mass = 0f, Restitution = 0.5f });
                            float r = b.BoundingRadius;
                            var vertices = new[]
                            {
                                new Vector2(0, -r),
                                new Vector2(r * 0.866f, r * 0.5f),
                                new Vector2(-r * 0.866f, r * 0.5f)
                            };
                            world.SetComponent(obstacle, new PolygonComponent(vertices));
                        }
                        else
                        {
                            world.SetComponent(obstacle, new RigidBodyComponent { Shape = b.Shape, Size = b.Size, BoundingRadius = b.BoundingRadius, Restitution = 0.5f, Mass = 0f });
                        }

                        // Support Auto-Rotation in gameplay
                        if (world.TryGetComponent<AutoRotateComponent>(editorId, out var autoRotate))
                        {
                            world.SetComponent(obstacle, autoRotate);
                        }
                        break;
                    case EditorTool.SoftCircle:
                        // Use segments based on size, min 8, max 16
                        int segments = Math.Clamp((int)(b.BoundingRadius / 5), 8, 16);
                        SoftBodyFactory.CreateCircle(world, t.Position, b.BoundingRadius, segments, 100f, 20f, c.Value);
                        break;
                }
            }
        }
    }

    private void LoadDemoLevel(World world)
    {
        var library = world.GetRequiredResource<PaletteLibraryResource>();
        
        // Ball
        var ball = world.CreateEntity();
        world.SetComponent(ball, new BallComponent());
        world.SetComponent(ball, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 16f, Restitution = 0.6f, Friction = 0.99f, Mass = 5.0f });
        world.SetComponent(ball, new TransformComponent { Position = new Vector2(512, 600) });
        world.SetComponent(ball, new VelocityComponent { Value = Vector2.Zero });
        world.SetComponent(ball, new DrawColorComponent(library.Specific.GetColor(1)));
        if (world.TryGetResource<UserAccountResource>(out var account) && account != null)
        {
            world.SetComponent(ball, new UserIdentityComponent { UserId = account.UserId, Username = account.Username });
        }

        // Rotating Poly
        var poly = world.CreateEntity();
        world.SetComponent(poly, new TransformComponent { Position = new Vector2(300, 300) });
        world.SetComponent(poly, new RigidBodyComponent { Shape = RigidBodyShape.Polygon, Mass = 0, Restitution = 0.8f });
        world.SetComponent(poly, new PolygonComponent(new[] { new Vector2(-50, -50), new Vector2(50, -50), new Vector2(80, 0), new Vector2(50, 50), new Vector2(-50, 50) }));
        world.SetComponent(poly, new DrawColorComponent(library.General.GetColor(2)));

        // SoftBody
        SoftBodyFactory.CreateCircle(world, new Vector2(700, 300), 50f, 8, 80f, 15f, library.General.GetColor(3));

        // Goal
        var goal = world.CreateEntity();
        world.SetComponent(goal, new TriggerZoneComponent("goal"));
        world.SetComponent(goal, new TransformComponent { Position = new Vector2(512, 150) });
        world.SetComponent(goal, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 40f, Mass = 0f });
        world.SetComponent(goal, new DrawColorComponent(Color.Black));

        // Random obstacles
        var random = new Random();
        for (int i = 0; i < 10; i++)
        {
            var pos = new Vector2(random.Next(100, 900), random.Next(200, 500));
            var size = new Vector2(random.Next(60, 150), random.Next(30, 80));
            var obstacle = world.CreateEntity();
            world.SetComponent(obstacle, new ObstacleComponent());
            world.SetComponent(obstacle, new TransformComponent { Position = pos });
            world.SetComponent(obstacle, new DrawColorComponent(library.General.GetColor(random.Next(library.General.Colors.Count))));
            world.SetComponent(obstacle, new RigidBodyComponent { Shape = RigidBodyShape.Rectangle, Size = size, Restitution = 0.5f, Mass = 0f });
        }
    }

    private void ClearAllGameplayEntities(World world)
    {
        foreach (var entity in world.GetEntitiesWith<ObstacleComponent>().ToList()) world.DestroyEntity(entity);
        foreach (var entity in world.GetEntitiesWith<BallComponent>().ToList()) world.DestroyEntity(entity);
        foreach (var entity in world.GetEntitiesWith<TriggerZoneComponent>().ToList()) world.DestroyEntity(entity);
        foreach (var entity in world.GetEntitiesWith<SoftBodyNodeComponent>().ToList()) world.DestroyEntity(entity);
        foreach (var entity in world.GetEntitiesWith<DistanceSpringComponent>().ToList()) world.DestroyEntity(entity);
        
        // Restore editor objects
        foreach (var entity in world.GetEntitiesWith<EditorObjectComponent>().ToList())
        {
            world.RemoveComponent<HiddenComponent>(entity);
        }
    }

    private void ClearEditorObjects(World world)
    {
        foreach (var entity in world.GetEntitiesWith<EditorObjectComponent>().ToList())
        {
            if (!world.HasComponent<TemplateComponent>(entity))
            {
                world.DestroyEntity(entity);
            }
        }
    }

    private void LoadMap(World world, string path)
    {
        if (!File.Exists(path)) return;
        var json = File.ReadAllText(path);
        var mapData = JsonSerializer.Deserialize<MapDataDto>(json);
        if (mapData == null) return;

        if (world.TryGetResource<MapEditorStateResource>(out var mapState) && mapState != null)
        {
            mapState.EnableGlobalLight = mapData.EnableGlobalLight;
        }

        foreach (var obj in mapData.Objects)
        {
            var entityId = world.CreateEntity();
            world.SetComponent(entityId, new EditorObjectComponent { ToolType = obj.Type });
            world.SetComponent(entityId, new TransformComponent { Position = obj.Position });
            
            switch (obj.Type)
            {
                case EditorTool.Square:
                    world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Rectangle, Size = new Vector2(80, 40), Mass = 0 });
                    world.SetComponent(entityId, new DrawColorComponent(Color.Gray));
                    break;
                case EditorTool.Circle:
                    world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 30, Mass = 0 });
                    world.SetComponent(entityId, new DrawColorComponent(Color.Gray));
                    break;
                case EditorTool.Triangle:
                    world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Polygon, BoundingRadius = 30, Mass = 0 });
                    world.SetComponent(entityId, new DrawColorComponent(Color.Gray));
                    // Setup initial polygon vertices for editor preview
                    var triVerts = new[]
                    {
                        new Vector2(0, -30),
                        new Vector2(30 * 0.866f, 30 * 0.5f),
                        new Vector2(-30 * 0.866f, 30 * 0.5f)
                    };
                    world.SetComponent(entityId, new PolygonComponent(triVerts));
                    break;
                case EditorTool.SoftCircle:
                    world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 30, Mass = 0 });
                    world.SetComponent(entityId, new DrawColorComponent(Color.DeepSkyBlue));
                    break;
                case EditorTool.Ball:
                    world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 16, Mass = 5 });
                    world.SetComponent(entityId, new DrawColorComponent(Color.White));
                    break;
                case EditorTool.Goal:
                    world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 40, Mass = 0 });
                    world.SetComponent(entityId, new DrawColorComponent(Color.Black));
                    break;
                case EditorTool.Light:
                    world.SetComponent(entityId, new RigidBodyComponent { Shape = RigidBodyShape.Circle, BoundingRadius = 15, Mass = 0 });
                    world.SetComponent(entityId, new DrawColorComponent(Color.Yellow));
                    break;
            }
        }
    }

    private void SaveCurrentMap(World world, MapEditorStateResource mapState)
    {
        var mapData = new MapDataDto();
        mapData.EnableGlobalLight = mapState.EnableGlobalLight;
        foreach (var entityId in world.GetEntitiesWith<EditorObjectComponent, TransformComponent>())
        {
            if (world.HasComponent<TemplateComponent>(entityId)) continue;

            world.TryGetComponent<EditorObjectComponent>(entityId, out var editorObj);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            mapData.Objects.Add(new MapObjectDto 
            { 
                Type = editorObj.ToolType, 
                Position = transform.Position 
            });
        }

        var fileName = mapState.SelectedMapPath ?? Path.Combine("maps", $"map_{DateTime.Now:yyyyMMddHHmmss}.json");
        var json = JsonSerializer.Serialize(mapData);
        if (!Directory.Exists("maps")) Directory.CreateDirectory("maps");
        File.WriteAllText(fileName, json);
    }

    private void ClearModal(World world)
    {
        foreach (var entity in world.GetEntitiesWith<UiModalComponent>().ToList())
        {
            world.DestroyEntity(entity);
        }
    }

    private void RefreshMaps(MapEditorStateResource mapState)
    {
        mapState.Maps.Clear();
        if (!Directory.Exists("maps")) Directory.CreateDirectory("maps");
        
        var files = Directory.GetFiles("maps", "*.json");
        foreach (var file in files)
        {
            mapState.Maps.Add(new MapMetadata
            {
                Name = Path.GetFileNameWithoutExtension(file),
                FilePath = file,
                LastModified = File.GetLastWriteTime(file)
            });
        }
    }

    private class MapDataDto
    {
        public bool EnableGlobalLight { get; set; } = true;
        public List<MapObjectDto> Objects { get; set; } = new();
    }

    private class MapObjectDto
    {
        public EditorTool Type { get; set; }
        public Vector2 Position { get; set; }
    }
}
