using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.GolfIt.Features.Ball.Components;
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
        var mouse = frameContext.CurrentMouse;
        var prevMouse = frameContext.PreviousMouse;

        foreach (var entityId in world.GetEntitiesWith<BallComponent, TransformComponent>())
        {
            world.TryGetComponent<BallComponent>(entityId, out var ball);
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<VelocityComponent>(entityId, out var velocity);

            var ballPos = transform.Position;
            var mousePos = new Vector2(mouse.X, mouse.Y);

            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                if (Vector2.Distance(mousePos, ballPos) < ball.Radius * 2)
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
                }
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (_isDragging && frameContext.SpriteBatch != null && frameContext.DebugPixel != null)
        {
            var mouse = frameContext.CurrentMouse;
            var mousePos = new Vector2(mouse.X, mouse.Y);
            
            // Draw a simple line using a scaled pixel
            var diff = _dragStart - mousePos;
            var angle = (float)System.Math.Atan2(diff.Y, diff.X);
            var length = diff.Length();

            frameContext.SpriteBatch.Draw(
                frameContext.DebugPixel,
                _dragStart,
                null,
                Color.Red,
                angle,
                Vector2.Zero,
                new Vector2(length, 2f),
                SpriteEffects.None,
                0);
        }
    }
}
