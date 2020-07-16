using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BoidSpawnerComponent : IComponentData
{
    public Entity Entity;
    public float3 Center;
    public float3 Offsets;
    public int SpawnNumber;
}
