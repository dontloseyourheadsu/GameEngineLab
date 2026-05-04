using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;

namespace GameEngineLab.Core.Features.Physics.Systems;

public sealed class PhysicsStepperSystem : IGameSystem
{
    private readonly List<IGameSystem> _subSystems = new();
    public int Order { get; }
    public int Substeps { get; set; } = 8;

    public PhysicsStepperSystem(int order, int substeps = 8)
    {
        Order = order;
        Substeps = substeps;
    }

    public void AddSystem(IGameSystem system)
    {
        _subSystems.Add(system);
        _subSystems.Sort((left, right) => left.Order.CompareTo(right.Order));
    }

    public void Update(World world, FrameContext frameContext)
    {
        if (frameContext.DeltaSeconds <= 0) return;

        float subDt = frameContext.DeltaSeconds / Substeps;

        for (int i = 0; i < Substeps; i++)
        {
            // We use a dummy GameTime that has the sub-step duration as its elapsed time
            var subGameTime = new GameTime(
                frameContext.GameTime.TotalGameTime, 
                TimeSpan.FromTicks((long)(subDt * TimeSpan.TicksPerSecond)));

            var subContext = new FrameContext(
                subGameTime,
                frameContext.CurrentKeyboard,
                frameContext.PreviousKeyboard,
                frameContext.CurrentMouse,
                frameContext.PreviousMouse,
                frameContext.Viewport,
                frameContext.SpriteBatch,
                frameContext.DebugPixel);

            foreach (var system in _subSystems)
            {
                system.Update(world, subContext);
            }
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        foreach (var system in _subSystems)
        {
            system.Draw(world, frameContext);
        }
    }
}
