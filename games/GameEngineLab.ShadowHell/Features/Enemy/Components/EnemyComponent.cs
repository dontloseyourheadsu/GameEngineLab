using GameEngineLab.Core.Features.Ecs.Components;

namespace GameEngineLab.ShadowHell.Features.Enemy.Components;

public struct EnemyComponent : IComponent
{
    public float Speed;
    public float AnimTime;
    
    public EnemyComponent()
    {
        Speed = 65f; // slower than player
        AnimTime = 0f;
    }
}
