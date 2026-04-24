using System;
using System.Collections.Generic;
using System.Linq;
using GameEngineLab.Core.Features.Ecs.Resources;

namespace GameEngineLab.Core.Features.Ecs.Systems;

public sealed class SystemScheduler
{
    private readonly List<IGameSystem> _systems = new();

    public void AddSystem(IGameSystem system)
    {
        ArgumentNullException.ThrowIfNull(system);
        _systems.Add(system);
        _systems.Sort((left, right) => left.Order.CompareTo(right.Order));
    }

    public IReadOnlyList<IGameSystem> Systems => _systems;

    public void Update(World world, FrameContext frameContext)
    {
        foreach (IGameSystem system in _systems)
        {
            system.Update(world, frameContext);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        foreach (IGameSystem system in _systems)
        {
            system.Draw(world, frameContext);
        }
    }
}
