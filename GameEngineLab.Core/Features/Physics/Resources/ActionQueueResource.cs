using GameEngineLab.Core.Features.Ecs.Resources;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GameEngineLab.Core.Features.Physics.Resources;

public sealed class ActionQueueResource
{
    private readonly Queue<string> _pendingActions = new();

    public void Enqueue(string actionId)
    {
        _pendingActions.Enqueue(actionId);
    }

    public bool TryDequeue(out string? actionId)
    {
        return _pendingActions.TryDequeue(out actionId);
    }
}
