using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.Rendering.Components;

public struct DrawColorComponent : IComponent
{
    public Color Value;

    public DrawColorComponent(Color color)
    {
        Value = color;
    }

    public static implicit operator Color(DrawColorComponent component) => component.Value;
    public static implicit operator DrawColorComponent(Color color) => new(color);
}
