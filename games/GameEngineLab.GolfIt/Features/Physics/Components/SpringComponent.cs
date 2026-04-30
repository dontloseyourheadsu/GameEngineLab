using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.GolfIt.Features.Physics.Components;

public struct SpringComponent : IComponent
{
    public Vector2 Anchor;
    public float Stiffness;
    public float Damping;
    public float RestLength;

    public SpringComponent()
    {
        Anchor = Vector2.Zero;
        Stiffness = 50f;
        Damping = 5f;
        RestLength = 0f;
    }
}
