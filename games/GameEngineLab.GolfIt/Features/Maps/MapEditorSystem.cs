using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Maps.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Physics.Components;
using GameEngineLab.GolfIt.Features.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace GameEngineLab.GolfIt.Features.Maps;

public enum EditorTool
{
    None,
    Square,
    Circle,
    Triangle,
    SoftCircle,
    Ball,
    Goal
}

public sealed class EditorContextResource
{
    public EditorTool SelectedTool { get; set; } = EditorTool.None;
    public EntityId? DraggedEntity { get; set; }
    public Vector2 DragOffset { get; set; }
    public bool IsPanning { get; set; }
    public Vector2 LastMousePos { get; set; }
}

public struct EditorObjectComponent : GameEngineLab.Core.Features.Ecs.Components.IComponent
{
    public EditorTool ToolType;
}

public struct TemplateComponent : GameEngineLab.Core.Features.Ecs.Components.IComponent { }

public sealed class MapEditorSystem : IGameSystem
{
    public int Order => 0;

    public void Update(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<GameStateResource>(out var gameState) || gameState?.Current != GameState.MapEditor)
            return;

        if (!world.TryGetResource<EditorContextResource>(out var editorContext) || editorContext == null)
        {
            editorContext = new EditorContextResource();
            world.SetResource(editorContext);
        }

        if (!world.TryGetResource<MapEditorStateResource>(out var mapState) || mapState == null) return;
        if (!world.TryGetResource<CameraResource>(out var camera) || camera == null) return;
        if (!world.TryGetResource<GameEngineLab.Core.Features.UI.Resources.UiThemeResource>(out var theme) || theme == null) return;

        var globalScale = theme.GlobalScale;
        var mouseState = frameContext.CurrentMouse;
        var prevMouseState = frameContext.PreviousMouse;
        var mousePos = mouseState.Position.ToVector2();
        var worldMousePos = camera.ScreenToWorld(mousePos, frameContext.Viewport);
        
        var wasLeftClicked = mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released;
        var wasLeftReleased = mouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed;
        var wasRightPressed = mouseState.RightButton == ButtonState.Pressed;

        // SIDEBAR BOUNDS (Scaled Screen Space)
        bool inLeftSidebar = mousePos.X < 240 * globalScale;
        bool inRightSidebar = mousePos.X > (GameEngineLab.Core.Features.Runtime.Resources.GameConstants.DefaultWindowWidth - 180) * globalScale;
        bool inToolbar = mousePos.Y < 60 * globalScale;
        bool inCanvas = !inLeftSidebar && !inRightSidebar && !inToolbar;

        // 1. PANNING (Right click anywhere)
        if (wasRightPressed)
        {
            var delta = mousePos - editorContext.LastMousePos;
            camera.Position -= delta / (camera.Zoom); // camera.Zoom already includes globalScale
        }

        // 2. DRAG START
        if (wasLeftClicked)
        {
            if (inCanvas)
            {
                // Try picking existing object on canvas
                EntityId? picked = PickObject(world, worldMousePos, false, 1.0f); // World space is unscaled
                if (picked.HasValue)
                {
                    editorContext.DraggedEntity = picked;
                    mapState.SelectedEntity = picked;
                    world.TryGetComponent<TransformComponent>(picked.Value, out var t);
                    editorContext.DragOffset = t.Position - worldMousePos;
                }
                else
                {
                    mapState.SelectedEntity = null; // Deselect
                    editorContext.IsPanning = true; // Panning on empty space drag
                }
            }
            else if (inRightSidebar)
            {
                // Try picking template
                // IMPORTANT: Templates are rendered in SCREEN SPACE at GLOBAL SCALE.
                EntityId? templateId = PickObject(world, mousePos, true, globalScale); 
                if (templateId.HasValue)
                {
                    world.TryGetComponent<EditorObjectComponent>(templateId.Value, out var templateObj);
                    world.TryGetComponent<DrawColorComponent>(templateId.Value, out var templateColor);
                    
                    // CLONE TEMPLATE to Canvas
                    var newEntity = CloneToCanvas(world, templateId.Value, worldMousePos);
                    editorContext.DraggedEntity = newEntity;
                    mapState.SelectedEntity = newEntity;
                    editorContext.DragOffset = Vector2.Zero;
                }
            }
        }

        // 3. DRAGGING
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            if (editorContext.DraggedEntity.HasValue)
            {
                var entityId = editorContext.DraggedEntity.Value;
                if (world.IsAlive(entityId))
                {
                    world.TryGetComponent<TransformComponent>(entityId, out var transform);
                    var newPos = worldMousePos + editorContext.DragOffset;
                    
                    // CLAMP TO MAP BOUNDS
                    if (world.TryGetResource<MapBoundsResource>(out var bounds))
                    {
                        var area = bounds.PlayArea;
                        newPos.X = Math.Clamp(newPos.X, area.Left, area.Right);
                        newPos.Y = Math.Clamp(newPos.Y, area.Top, area.Bottom);
                    }

                    transform.Position = newPos;
                    world.SetComponent(entityId, transform);
                }
            }
            else if (editorContext.IsPanning && inCanvas)
            {
                var delta = mousePos - editorContext.LastMousePos;
                camera.Position -= delta / (camera.Zoom);
            }
        }

        if (wasLeftReleased)
        {
            editorContext.DraggedEntity = null;
            editorContext.IsPanning = false;
        }

        editorContext.LastMousePos = mousePos;
    }

    public void Draw(World world, FrameContext frameContext) { }

    private EntityId? PickObject(World world, Vector2 point, bool templates, float scale)
    {
        foreach (var entityId in world.GetEntitiesWith<EditorObjectComponent, TransformComponent>())
        {
            bool isTemplate = world.HasComponent<TemplateComponent>(entityId);
            if (templates != isTemplate) continue;

            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            
            if (IsPointInObject(point, transform, body, scale))
            {
                return entityId;
            }
        }
        return null;
    }

    private bool IsPointInObject(Vector2 point, TransformComponent t, RigidBodyComponent b, float scale)
    {
        var pos = t.Position * scale;
        if (b.Shape == RigidBodyShape.Circle || b.Shape == RigidBodyShape.Polygon)
        {
            return Vector2.Distance(point, pos) <= b.BoundingRadius * scale;
        }
        else if (b.Shape == RigidBodyShape.Rectangle)
        {
            var halfSize = (b.Size * scale) / 2f;
            var rect = new Rectangle((int)(pos.X - halfSize.X), (int)(pos.Y - halfSize.Y), (int)(b.Size.X * scale), (int)(b.Size.Y * scale));
            return rect.Contains(point);
        }
        return false;
    }

    private EntityId CloneToCanvas(World world, EntityId templateId, Vector2 worldPos)
    {
        world.TryGetComponent<EditorObjectComponent>(templateId, out var templateObj);
        world.TryGetComponent<DrawColorComponent>(templateId, out var templateColor);
        world.TryGetComponent<RigidBodyComponent>(templateId, out var templateBody);

        var newEntity = world.CreateEntity();
        world.SetComponent(newEntity, new EditorObjectComponent { ToolType = templateObj.ToolType });
        world.SetComponent(newEntity, new TransformComponent { Position = worldPos });
        world.SetComponent(newEntity, new DrawColorComponent(templateColor.Value));
        world.SetComponent(newEntity, templateBody);

        // CLONE POLYGON
        if (world.TryGetComponent<PolygonComponent>(templateId, out var poly))
        {
            world.SetComponent(newEntity, new PolygonComponent(poly.Vertices.ToArray()));
        }

        // CLONE AUTO-ROTATE
        if (world.TryGetComponent<AutoRotateComponent>(templateId, out var rot))
        {
            world.SetComponent(newEntity, rot);
        }

        // Special logic for unique items
        if (templateObj.ToolType == EditorTool.Ball || templateObj.ToolType == EditorTool.Goal)
        {
             foreach (var e in world.GetEntitiesWith<EditorObjectComponent>().ToList())
             {
                 if (e == newEntity) continue;
                 if (world.HasComponent<TemplateComponent>(e)) continue;
                 if (world.TryGetComponent<EditorObjectComponent>(e, out var c) && c.ToolType == templateObj.ToolType)
                     world.DestroyEntity(e);
             }
        }

        return newEntity;
    }
}
