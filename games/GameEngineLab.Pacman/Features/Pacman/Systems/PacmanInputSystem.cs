using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Pacman.Features.Gameplay.Resources;
using GameEngineLab.Pacman.Features.Pacman.Components;
using GameEngineLab.Pacman.Features.UI.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GameEngineLab.Pacman.Features.Pacman.Systems;

public sealed class PacmanInputSystem : IGameSystem
{
    public int Order => 0;

    public void Update(World world, FrameContext frameContext)
    {
        var gameplay = world.GetRequiredResource<GameplayStateResource>();
        var appMode = world.GetRequiredResource<AppModeResource>();
        if (appMode.Mode != AppMode.Gameplay || gameplay.IsGameOver)
        {
            return;
        }

        foreach (var entity in world.GetEntitiesWith<PacmanPlayerComponent>())
        {
            Vector2 direction = Vector2.Zero;

            if (frameContext.CurrentKeyboard.IsKeyDown(Keys.Left) || frameContext.CurrentKeyboard.IsKeyDown(Keys.A))
            {
                direction.X -= 1f;
            }

            if (frameContext.CurrentKeyboard.IsKeyDown(Keys.Right) || frameContext.CurrentKeyboard.IsKeyDown(Keys.D))
            {
                direction.X += 1f;
            }

            if (frameContext.CurrentKeyboard.IsKeyDown(Keys.Up) || frameContext.CurrentKeyboard.IsKeyDown(Keys.W))
            {
                direction.Y -= 1f;
            }

            if (frameContext.CurrentKeyboard.IsKeyDown(Keys.Down) || frameContext.CurrentKeyboard.IsKeyDown(Keys.S))
            {
                direction.Y += 1f;
            }

            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }

            if (!world.TryGetComponent<PacmanPlayerComponent>(entity, out var pacman))
            {
                continue;
            }

            pacman.DesiredDirection = new Point((int)direction.X, (int)direction.Y);
            world.SetComponent(entity, pacman);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
    }
}
