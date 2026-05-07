using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.GolfIt.Features.Maps;
using GameEngineLab.GolfIt.Features.Physics.Components;
using Microsoft.Xna.Framework;

namespace GameEngineLab.GolfIt.Features.Physics.Systems;

public sealed class DemoRotationSystem : IGameSystem
{
    public int Order => 0;

    public void Update(World world, FrameContext frameContext)
    {
        world.TryGetResource<EditorContextResource>(out var editorContext);

        foreach (var entityId in world.GetEntitiesWith<AutoRotateComponent, TransformComponent>())
        {
            world.TryGetComponent<AutoRotateComponent>(entityId, out var autoRotate);
            if (!autoRotate.IsEnabled) continue;

            // Pause if being dragged
            if (editorContext?.DraggedEntity == entityId) continue;

            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            transform.Rotation += autoRotate.Speed * frameContext.DeltaSeconds;
            world.SetComponent(entityId, transform);
        }
    }

    public void Draw(World world, FrameContext frameContext) { }
}
