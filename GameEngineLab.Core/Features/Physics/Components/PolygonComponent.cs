using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.Core.Features.Physics.Components;

public struct PolygonComponent : IComponent
{
    public Vector2[] Vertices;
    public Vector2[] TransformedVertices;

    public PolygonComponent(Vector2[] vertices)
    {
        Vertices = vertices;
        TransformedVertices = new Vector2[vertices.Length];
    }
}
