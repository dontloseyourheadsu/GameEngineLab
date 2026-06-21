using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.ShadowHell.Features.Environment.Components;

public struct LightSourceComponent : IComponent
{
    public Color LightColor;
    public float Radius;
    public float Intensity; // 0 to 1
    
    public LightSourceComponent(Color color, float radius, float intensity = 0.5f)
    {
        LightColor = color;
        Radius = radius;
        Intensity = intensity;
    }
}
