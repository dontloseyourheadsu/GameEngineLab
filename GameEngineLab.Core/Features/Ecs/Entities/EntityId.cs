namespace GameEngineLab.Core.Features.Ecs.Entities;

public readonly record struct EntityId(int Value)
{
    public static EntityId Invalid => new(-1);

    public override string ToString() => Value.ToString();
}
