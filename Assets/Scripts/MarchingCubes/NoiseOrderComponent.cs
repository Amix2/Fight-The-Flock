using System;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
[Serializable]
public struct NoiseOrderComponent : IComponentData
{
    public float3 minPoint;
    public float3 size;
    public int3 sampleCount;
    public float noiseFactor;
    public float isoSurfaceLevel;
}