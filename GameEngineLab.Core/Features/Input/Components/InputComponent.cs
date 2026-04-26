using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.Input.Components;

public struct InputComponent : IComponent
{
    /// <summary>
    /// The direction the player wants to move.
    /// </summary>
    public Point DesiredDirection;

    /// <summary>
    /// Action buttons (e.g., bits for different buttons).
    /// </summary>
    public int Buttons;

    public bool IsDown(int buttonBit) => (Buttons & buttonBit) != 0;
}
