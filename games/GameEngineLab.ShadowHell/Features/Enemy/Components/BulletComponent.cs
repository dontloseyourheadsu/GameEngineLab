using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.ShadowHell.Features.Enemy.Components;

public struct BulletComponent : IComponent
{
    public Vector2 Velocity;
    public float Damage;
    public float Lifetime;

    public BulletComponent(Vector2 velocity, float damage = 0.5f, float lifetime = 4.0f)
    {
        Velocity = velocity;
        Damage = damage;
        Lifetime = lifetime;
    }
}
