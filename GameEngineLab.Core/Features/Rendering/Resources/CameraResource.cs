using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngineLab.Core.Features.Rendering.Resources;

public sealed class CameraResource
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public float Zoom { get; set; } = 1.0f;
    public float Rotation { get; set; } = 0f;

    public Matrix GetViewMatrix(Viewport viewport)
    {
        return Matrix.CreateTranslation(new Vector3(-Position, 0)) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
               Matrix.CreateTranslation(new Vector3(viewport.Width * 0.5f, viewport.Height * 0.5f, 0));
    }

    public Vector2 ScreenToWorld(Vector2 screenPos, Viewport viewport)
    {
        return Vector2.Transform(screenPos, Matrix.Invert(GetViewMatrix(viewport)));
    }
}
