using GameEngineLab.Core.Features.Ecs.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GameEngineLab.Core.Features.Animation.Components;

public enum BoneShape
{
    None,
    Line,
    Circle,
    Rectangle,
    Ellipse
}

public struct Bone
{
    public string Name;
    public int ParentIndex; // -1 for root
    
    // Relative transformation in local space of parent
    public Vector2 LocalOffset; 
    public float LocalAngle; // relative rotation angle in radians
    public float Length;     // length of the bone
    
    // Rendering parameters
    public BoneShape Shape;
    public Vector2 ShapeSize; // width and height for rectangle/ellipse, or diameter/radius for circle
    public Color Color;
    
    // Computed world-space transform (computed during animation update)
    public Vector2 WorldStart;
    public Vector2 WorldEnd;
    public float WorldAngle;
}

public struct SkeletonComponent : IComponent
{
    public List<Bone> Bones { get; set; }
    
    public SkeletonComponent()
    {
        Bones = new List<Bone>();
    }
}
