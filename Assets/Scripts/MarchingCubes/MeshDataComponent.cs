using JetBrains.Annotations;
using System;
using Unity.Entities;
using UnityEngine;

[GenerateAuthoringComponent]
public struct MeshDataComponent : ISharedComponentData, IEquatable<MeshDataComponent>
{
    public Material material;

    public bool Equals(MeshDataComponent other)
    {
        if (material == null || other.material == null) return false;
        return material.Equals(other.material);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() * material.GetHashCode();
    }
}