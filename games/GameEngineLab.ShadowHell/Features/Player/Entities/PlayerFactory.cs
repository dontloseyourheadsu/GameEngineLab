using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Animation.Components;
using GameEngineLab.ShadowHell.Features.Player.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

using GameEngineLab.ShadowHell.Features.Environment.Components;

namespace GameEngineLab.ShadowHell.Features.Player.Entities;

public static class PlayerFactory
{
    public static EntityId CreatePlayer(World world, Vector2 startPosition)
    {
        var player = world.CreateEntity();

        // 1. Core ECS and Physics Components
        world.SetComponent(player, new PlayerComponent());
        world.SetComponent(player, new TransformComponent 
        { 
            Position = startPosition, 
            Rotation = 0f 
        });
        world.SetComponent(player, new VelocityComponent 
        { 
            Value = Vector2.Zero 
        });
        
        // RigidBody for physical interactions with room boundaries and obstacles
        world.SetComponent(player, new RigidBodyComponent
        {
            Shape = RigidBodyShape.Circle,
            BoundingRadius = 18f,
            Mass = 1.0f,
            Restitution = 0.2f,
            Friction = 0.95f
        });

        // Add purple/violet glowing light source centered on player (smaller and less bright)
        world.SetComponent(player, new LightSourceComponent(new Color(177, 0, 255), 220f, 0.4f));

        // Skip default ShapeRenderSystem; PlayerRendererSystem handles drawing instead.
        world.SetComponent(player, new HiddenComponent());

        return player;
    }
}
