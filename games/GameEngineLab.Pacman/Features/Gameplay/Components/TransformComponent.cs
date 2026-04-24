using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Gameplay.Components;

public struct TransformComponent : IComponent
{
    public Vector2 Position;
}
