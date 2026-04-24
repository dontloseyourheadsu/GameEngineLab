using GameEngineLab.Core.Features.Ecs.Resources;

namespace GameEngineLab.Core.Features.Ecs.Systems;

public interface IGameSystem
{
    int Order { get; }

    void Update(World world, FrameContext frameContext);

    void Draw(World world, FrameContext frameContext);
}
