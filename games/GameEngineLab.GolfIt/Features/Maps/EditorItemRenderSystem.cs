using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameEngineLab.GolfIt.Features.Maps;

public sealed class EditorItemRenderSystem : IGameSystem
{
    public int Order => 210; // Draw after UI panels

    public void Update(World world, FrameContext frameContext) { }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;
        if (!world.TryGetResource<GameEngineLab.Core.Features.UI.Resources.UiThemeResource>(out var theme) || theme == null) return;

        var globalScale = theme.GlobalScale;

        // This system draws only Template items in screen space (sidebar)
        foreach (var entityId in world.GetEntitiesWith<TemplateComponent>())
        {
            if (!world.TryGetComponent<TransformComponent>(entityId, out var transform)) continue;
            if (!world.TryGetComponent<RigidBodyComponent>(entityId, out var body)) continue;
            
            Color color = Color.White;
            if (world.TryGetComponent<DrawColorComponent>(entityId, out var drawColor))
            {
                color = drawColor.Value;
            }

            // Draw in screen space with global scale
            if (body.Shape == RigidBodyShape.Circle)
            {
                ShapeRenderer.DrawCircle(frameContext.SpriteBatch, frameContext.DebugPixel, transform.Position * globalScale, body.BoundingRadius * globalScale, color);
            }
            else if (body.Shape == RigidBodyShape.Rectangle)
            {
                ShapeRenderer.DrawRectangle(frameContext.SpriteBatch, frameContext.DebugPixel, transform.Position * globalScale, body.Size * globalScale, 0.0f, color);
            }
        }
    }
}
