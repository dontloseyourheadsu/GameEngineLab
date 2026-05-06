using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.GolfIt.Features.Ball.Components;
using GameEngineLab.GolfIt.Features.Runtime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.GolfIt.Features.Input.Systems;

public sealed class SlingshotInputSystem : IGameSystem
{
    public int Order => 0;

    private bool _isDragging;
    private Vector2 _dragStart;

    public void Update(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<GameStateResource>(out var gameState) || gameState?.Current != GameState.Playing)
            return;

        if (!world.TryGetResource<CameraResource>(out var camera) || camera == null) return;

        var mouse = frameContext.CurrentMouse;
        var prevMouse = frameContext.PreviousMouse;
        var mousePos = camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y), frameContext.Viewport);

        foreach (var entityId in world.GetEntitiesWith<BallComponent, TransformComponent, RigidBodyComponent>())
        {
            world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);

            var ballPos = transform.Position;

            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                if (Vector2.Distance(mousePos, ballPos) < body.BoundingRadius * 2)
                {
                    _isDragging = true;
                    _dragStart = mousePos;
                }
            }

            if (_isDragging && mouse.LeftButton == ButtonState.Released)
            {
                _isDragging = false;
                var dragEnd = mousePos;
                var direction = _dragStart - dragEnd;
                var strength = direction.Length() * 5f; // Scale factor
                
                if (strength > 0)
                {
                    velocity.Value = Vector2.Normalize(direction) * strength;
                    world.SetComponent(entityId, velocity);

                    // Increment strokes
                    if (gameState != null)
                    {
                        gameState.Strokes++;
                        if (gameState.Strokes >= gameState.StrokeLimit)
                        {
                            gameState.Current = GameState.GameOver;
                        }
                    }
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (!world.TryGetResource<CameraResource>(out var camera) || camera == null) return;

        if (_isDragging && frameContext.SpriteBatch != null && frameContext.DebugPixel != null)
        {
            var mouse = frameContext.CurrentMouse;
            var mousePos = camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y), frameContext.Viewport);
            
            // Draw aiming line
            var diff = _dragStart - mousePos;
            var angle = (float)System.Math.Atan2(diff.Y, diff.X);
            var length = diff.Length();

            frameContext.SpriteBatch.Draw(
                frameContext.DebugPixel,
                _dragStart,
                null,
                Color.Red * 0.5f,
                angle,
                Vector2.Zero,
                new Vector2(length, 1f),
                SpriteEffects.None,
                0);

            // Highlight the ball being dragged
            foreach (var entityId in world.GetEntitiesWith<BallComponent, TransformComponent, RigidBodyComponent>())
            {
                world.TryGetComponent<RigidBodyComponent>(entityId, out var body);
                world.TryGetComponent<TransformComponent>(entityId, out var transform);
                
                if (Vector2.Distance(_dragStart, transform.Position) < body.BoundingRadius * 2)
                {
                    ShapeRenderer.DrawCircleOutline(
                        frameContext.SpriteBatch,
                        frameContext.DebugPixel,
                        transform.Position,
                        body.BoundingRadius + 2,
                        Color.Yellow);
                }
            }
        }
    }
}
