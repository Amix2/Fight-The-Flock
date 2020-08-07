using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ChunkSpawnOrderer : MonoBehaviour
{

    [Range(0.0f, 1.0f)]
    public float surfaceLevel;
    [Range(0.0000001f, 0.5f)]
    public float noiseFactor = 1f;

    public float3 size;
    public int3 sampleCount;
    public int3 count;

    public Material material;
    // Start is called before the first frame update
    void Start()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        for (int3 iter = default; iter.z < count.z; iter.z++)
            for (iter.y = 0; iter.y < count.y; iter.y++)
                for (iter.x = 0; iter.x < count.x; iter.x++)
                {
                    NoiseOrderComponent order = new NoiseOrderComponent { isoSurfaceLevel = surfaceLevel, minPoint = size * iter, size = size, noiseFactor = noiseFactor, sampleCount = sampleCount };
                    Entity entity = entityManager.CreateEntity(typeof(NoiseOrderComponent), typeof(MeshDataComponent));
                    entityManager.SetComponentData(entity, order);
                    entityManager.SetSharedComponentData(entity, new MeshDataComponent { material = this.material });
                }

    }
}
