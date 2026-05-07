using GameEngineLab.Core.Features.Ecs.Components;

namespace GameEngineLab.GolfIt.Features.Physics.Components;

public struct AutoRotateComponent : IComponent
{
    public float Speed { get; set; }
    public bool IsEnabled { get; set; }

    public AutoRotateComponent(float speed = 1.0f, bool enabled = true)
    {
        Speed = speed;
        IsEnabled = enabled;
    }
}
