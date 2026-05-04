using GameEngineLab.Core.Features.Ecs.Components;

namespace GameEngineLab.Core.Features.Physics.Components;

public struct TriggerZoneComponent : IComponent
{
    public string ActionId { get; set; }

    public TriggerZoneComponent(string actionId)
    {
        ActionId = actionId;
    }
}
