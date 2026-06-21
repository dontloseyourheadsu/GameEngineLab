using GameEngineLab.Core.Features.Ecs.Resources;
using GameEngineLab.Core.Features.Ecs.Systems;
using GameEngineLab.Core.Features.Physics.Components;
using GameEngineLab.Core.Features.Rendering.Components;
using GameEngineLab.Core.Features.Rendering.Resources;
using GameEngineLab.Core.Features.Animation.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GameEngineLab.Core.Features.Animation.Systems;

public sealed class SkeletonSystem : IGameSystem
{
    public int Order => 90; // Runs right before rendering

    public void Update(World world, FrameContext frameContext)
    {
        foreach (var entityId in world.GetEntitiesWith<TransformComponent, SkeletonComponent>())
        {
            world.TryGetComponent<TransformComponent>(entityId, out var transform);
            world.TryGetComponent<SkeletonComponent>(entityId, out var skeleton);

            var bones = skeleton.Bones;
            if (bones == null || bones.Count == 0) continue;

            // Compute bone positions using forward kinematics
            for (int i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];
                
                Vector2 parentStart = (bone.ParentIndex == -1) ? transform.Position : bones[bone.ParentIndex].WorldStart;
                float parentAngle = (bone.ParentIndex == -1) ? transform.Rotation : bones[bone.ParentIndex].WorldAngle;
                
                // Rotate local offset by parent angle
                float cos = (float)Math.Cos(parentAngle);
                float sin = (float)Math.Sin(parentAngle);
                Vector2 rotatedOffset = new Vector2(
                    bone.LocalOffset.X * cos - bone.LocalOffset.Y * sin,
                    bone.LocalOffset.X * sin + bone.LocalOffset.Y * cos
                );

                bone.WorldStart = parentStart + rotatedOffset;
                bone.WorldAngle = parentAngle + bone.LocalAngle;

                // Compute end position
                bone.WorldEnd = bone.WorldStart + new Vector2(
                    (float)Math.Cos(bone.WorldAngle) * bone.Length,
                    (float)Math.Sin(bone.WorldAngle) * bone.Length
                );

                bones[i] = bone; // Save changes back to list (since Bone is a struct)
            }

            world.SetComponent(entityId, skeleton);
        }
    }

    public void Draw(World world, FrameContext frameContext)
    {
        if (frameContext.SpriteBatch == null || frameContext.DebugPixel == null) return;

        foreach (var entityId in world.GetEntitiesWith<SkeletonComponent>())
        {
            if (world.HasComponent<HiddenComponent>(entityId)) continue;

            world.TryGetComponent<SkeletonComponent>(entityId, out var skeleton);
            var bones = skeleton.Bones;
            if (bones == null) continue;

            foreach (var bone in bones)
            {
                if (bone.Shape == BoneShape.None) continue;

                Vector2 shapeCenter = bone.WorldStart + new Vector2(
                    (float)Math.Cos(bone.WorldAngle) * (bone.Length / 2f),
                    (float)Math.Sin(bone.WorldAngle) * (bone.Length / 2f)
                );

                switch (bone.Shape)
                {
                    case BoneShape.Line:
                        ShapeRenderer.DrawLine(
                            frameContext.SpriteBatch, 
                            frameContext.DebugPixel, 
                            bone.WorldStart, 
                            bone.WorldEnd, 
                            bone.Color, 
                            (int)bone.ShapeSize.Y
                        );
                        break;

                    case BoneShape.Circle:
                        // Draw circle centered at start (or center)
                        // If length is 0, draw at start. If length > 0, center it
                        Vector2 circleCenter = bone.Length == 0 ? bone.WorldStart : shapeCenter;
                        ShapeRenderer.DrawCircle(
                            frameContext.SpriteBatch, 
                            frameContext.DebugPixel, 
                            circleCenter, 
                            bone.ShapeSize.X, 
                            bone.Color
                        );
                        break;

                    case BoneShape.Rectangle:
                        ShapeRenderer.DrawRectangle(
                            frameContext.SpriteBatch, 
                            frameContext.DebugPixel, 
                            shapeCenter, 
                            bone.ShapeSize, 
                            bone.WorldAngle, 
                            bone.Color
                        );
                        break;

                    case BoneShape.Ellipse:
                        // ShapeRenderer doesn't support rotated ellipse out of the box,
                        // so we approximate with a rotated rectangle or draw as non-rotated ellipse.
                        // For a silhouette cartoonish style, a rectangle or circles at joints looks great!
                        // We can also draw multiple overlapping circles to create an ellipse-like shape.
                        ShapeRenderer.DrawRectangle(
                            frameContext.SpriteBatch, 
                            frameContext.DebugPixel, 
                            shapeCenter, 
                            bone.ShapeSize, 
                            bone.WorldAngle, 
                            bone.Color
                        );
                        break;
                }
            }
        }
    }
}
