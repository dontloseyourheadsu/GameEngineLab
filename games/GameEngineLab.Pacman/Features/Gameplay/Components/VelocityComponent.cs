using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Pacman.Features.Gameplay.Components;

public struct VelocityComponent : IComponent
{
    public Vector2 Value;
}
