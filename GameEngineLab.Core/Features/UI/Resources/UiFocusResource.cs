using GameEngineLab.Core.Features.Ecs.Entities;

namespace GameEngineLab.Core.Features.UI.Resources;

public sealed class UiFocusResource
{
    public EntityId FocusedEntity { get; set; } = EntityId.Invalid;
}
