using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Input.Components;
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
            Point desiredDirection = Point.Zero;

            if (frameContext.CurrentKeyboard.IsKeyDown(Keys.Left) || frameContext.CurrentKeyboard.IsKeyDown(Keys.A))
            {
                desiredDirection = new Point(-1, 0);
            }
            else if (frameContext.CurrentKeyboard.IsKeyDown(Keys.Right) || frameContext.CurrentKeyboard.IsKeyDown(Keys.D))
            {
                desiredDirection = new Point(1, 0);
            }
            else if (frameContext.CurrentKeyboard.IsKeyDown(Keys.Up) || frameContext.CurrentKeyboard.IsKeyDown(Keys.W))
            {
                desiredDirection = new Point(0, -1);
            }
            else if (frameContext.CurrentKeyboard.IsKeyDown(Keys.Down) || frameContext.CurrentKeyboard.IsKeyDown(Keys.S))
            {
                desiredDirection = new Point(0, 1);
            }

            if (!world.HasComponent<InputComponent>(entity))
            {
                world.SetComponent(entity, new InputComponent());
            }

            if (world.TryGetComponent<InputComponent>(entity, out var input))
            {
                input.DesiredDirection = desiredDirection;
                world.SetComponent(entity, input);
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
    }
}
