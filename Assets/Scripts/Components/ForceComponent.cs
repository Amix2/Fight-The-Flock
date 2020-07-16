using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ForceComponent : IComponentData
{
    public float3 Force;
}

