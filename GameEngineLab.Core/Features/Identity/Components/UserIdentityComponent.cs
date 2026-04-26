using System;
using GameEngineLab.Core.Features.Ecs.Components;

namespace GameEngineLab.Core.Features.Identity.Components;

public struct UserIdentityComponent : IComponent
{
    public Guid UserId;
    public string Username;
}
