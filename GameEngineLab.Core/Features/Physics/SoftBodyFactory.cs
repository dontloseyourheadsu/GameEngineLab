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

        // Connect nodes to maintain form (All-to-all for maximum rigidity)
        for (int i = 0; i < segments; i++)
        {
            // Perimeter spring
            var n1 = nodes[i];
            var n2 = nodes[(i + 1) % segments];
            var distP = Vector2.Distance(world.GetRequiredComponent<TransformComponent>(n1).Position, 
                                        world.GetRequiredComponent<TransformComponent>(n2).Position);
            
            var perimeterSpring = world.CreateEntity();
            world.SetComponent(perimeterSpring, new DistanceSpringComponent(n1, n2, stiffness, damping, distP));

            // Radial spring to center
            var distR = Vector2.Distance(world.GetRequiredComponent<TransformComponent>(n1).Position, center);
            var radialSpring = world.CreateEntity();
            world.SetComponent(radialSpring, new DistanceSpringComponent(n1, centerNode, stiffness, damping, distR));

            // Internal Cross-springs (invisible trusses)
            for (int j = i + 2; j < segments; j++)
            {
                // Avoid adjacent nodes (already handled by perimeter) and ensure we don't double up
                if (i == 0 && j == segments - 1) continue; 

                var nOther = nodes[j];
                var distC = Vector2.Distance(world.GetRequiredComponent<TransformComponent>(n1).Position, 
                                            world.GetRequiredComponent<TransformComponent>(nOther).Position);
                
                var crossSpring = world.CreateEntity();
                world.SetComponent(crossSpring, new DistanceSpringComponent(n1, nOther, stiffness * 0.5f, damping, distC));
            }
        }
    }
}
