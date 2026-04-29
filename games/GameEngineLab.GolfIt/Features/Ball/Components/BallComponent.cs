using GameEngineLab.Core.Features.Ecs.Components;

namespace GameEngineLab.GolfIt.Features.Ball.Components;

public struct BallComponent : IComponent
{
    public float Radius { get; set; }

    public BallComponent()
    {
        Radius = 10f;
    }
}
