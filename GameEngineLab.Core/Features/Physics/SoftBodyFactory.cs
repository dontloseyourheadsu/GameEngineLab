using GameEngineLab.Core.Features.Ecs.Components;
using GameEngineLab.Core.Features.Ecs.Entities;
using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using Microsoft.Xna.Framework;
using System;

namespace GameEngineLab.Core.Features.Physics;

public struct SoftBodyNodeComponent : IComponent { }

public static class SoftBodyFactory
{
    public static void CreateCircle(World world, Vector2 center, float radius, int segments, float stiffness, float damping, Color color)
    {
        var nodes = new EntityId[segments];
        for (int i = 0; i < segments; i++)
        {
            float angle = (float)(i * Math.PI * 2 / segments);
            var pos = center + new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius);
            
            var node = world.CreateEntity();
            world.SetComponent(node, new TransformComponent { Position = pos });
            world.SetComponent(node, new VelocityComponent { Value = Vector2.Zero });
            world.SetComponent(node, new RigidBodyComponent 
            { 
                Shape = RigidBodyShape.Circle,
                BoundingRadius = radius * (MathHelper.Pi / segments), // Approximate node size
                Mass = 0.5f,
                Restitution = 0.1f,
                CollisionGroup = 1, // Softbody group
                CollisionMask = ~ (1 << 1) // Collide with everything EXCEPT group 1
            });
            world.SetComponent(node, new DrawColorComponent(color));
            world.SetComponent(node, new SoftBodyNodeComponent());
            nodes[i] = node;
        }

        // Center node
        var centerNode = world.CreateEntity();
        world.SetComponent(centerNode, new TransformComponent { Position = center });
        world.SetComponent(centerNode, new VelocityComponent { Value = Vector2.Zero });
        world.SetComponent(centerNode, new RigidBodyComponent 
        { 
            Shape = RigidBodyShape.Circle,
            BoundingRadius = radius * 0.2f,
            Mass = 2.0f,
            Restitution = 0.1f,
            CollisionGroup = 1,
            CollisionMask = ~ (1 << 1)
        });
        world.SetComponent(centerNode, new DrawColorComponent(color));
        world.SetComponent(centerNode, new SoftBodyNodeComponent());

        // Connect nodes in a circle and to center
        for (int i = 0; i < segments; i++)
        {
            var n1 = nodes[i];
            var n2 = nodes[(i + 1) % segments];
            
            // Perimeter spring
            var perimeterSpring = world.CreateEntity();
            world.SetComponent(perimeterSpring, new DistanceSpringComponent(n1, n2, stiffness, damping));

            // Radial spring
            var radialSpring = world.CreateEntity();
            world.SetComponent(radialSpring, new DistanceSpringComponent(n1, centerNode, stiffness, damping));
        }
    }
}
