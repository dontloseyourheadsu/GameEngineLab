using System.Collections.Generic;
using GameEngineLab.Core.Features.Ecs.Entities;

namespace GameEngineLab.GolfIt.Features.Maps;

public sealed class MapEditorStateResource
{
    public string? SelectedMapPath { get; set; }
    public string? MapPathToDelete { get; set; }
    public List<MapMetadata> Maps { get; } = new();
    
    // Advanced Editor State
    public EntityId? SelectedEntity { get; set; }
    public Dictionary<EditorTool, EntityId> ToolItems { get; } = new();

    // Property UI IDs
    public EntityId ColorSelectorId { get; set; }
    public EntityId SizeSliderId { get; set; }
    public EntityId RotationSliderId { get; set; }
    public EntityId MapWidthSliderId { get; set; }
    public EntityId MapHeightSliderId { get; set; }
    public EntityId AutoRotateCheckboxId { get; set; }
    public EntityId EnableGlobalLightCheckboxId { get; set; }
    public bool EnableGlobalLight { get; set; } = true;
}
