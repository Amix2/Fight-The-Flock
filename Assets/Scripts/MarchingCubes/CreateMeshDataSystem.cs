
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

class CreateMeshDataSystem : JobComponentSystem
{
    private EntityCommandBufferSystem EndSimulationCBS;

    private NativeArray<int> Triangles;
    private NativeArray<int> CornerIndexAFromEdge;
    private NativeArray<int> CornerIndexBFromEdge;


    protected override void OnCreate()
    {
        base.OnCreate();
        EndSimulationCBS = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        Triangles = new NativeArray<int>(Table.Triangles1dArray.Length, Allocator.Persistent);
        CornerIndexAFromEdge = new NativeArray<int>(Table.CornerIndexAFromEdge.Length, Allocator.Persistent);
        CornerIndexBFromEdge = new NativeArray<int>(Table.CornerIndexBFromEdge.Length, Allocator.Persistent);
        Triangles.CopyFrom(Table.Triangles1dArray);
        CornerIndexAFromEdge.CopyFrom(Table.CornerIndexAFromEdge);
        CornerIndexBFromEdge.CopyFrom(Table.CornerIndexBFromEdge);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Triangles.Dispose();
        CornerIndexAFromEdge.Dispose();
        CornerIndexBFromEdge.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer.ParallelWriter commandBufferPR = EndSimulationCBS.CreateCommandBuffer().AsParallelWriter();


        NativeArray<int> Triangles = this.Triangles;
        NativeArray<int> CornerIndexAFromEdge = this.CornerIndexAFromEdge;
        NativeArray<int> CornerIndexBFromEdge = this.CornerIndexBFromEdge;
        //, ref DynamicBuffer<Normals> normalsBuf, ref DynamicBuffer<Index> trianglesBuf
        JobHandle job = Entities.WithAll<BuffersPresentTag>().WithNone<NoiseBufferTag>().ForEach((Entity thisEntity, int entityInQueryIndex, ref NoiseOrderComponent noiseOrder
            , ref DynamicBuffer<Vertices> verticesBuf, ref DynamicBuffer<Normals> normalsBuf, ref DynamicBuffer<Index> trianglesBuf) =>
        {
            commandBufferPR.AddComponent(entityInQueryIndex, thisEntity, new NoiseBufferTag());

            DynamicBuffer<float3> vertices = verticesBuf.Reinterpret<float3>();
            DynamicBuffer<float3> normals = normalsBuf.Reinterpret<float3>();
            DynamicBuffer<int> triangles = trianglesBuf.Reinterpret<int>();

            float3 sampleDist = noiseOrder.size / noiseOrder.sampleCount;
            int3 samples = (int3)(noiseOrder.size / sampleDist) + 1;
            int numOfSamples = samples.x * samples.y * samples.z;
            NativeArray<NoiseVal> noiseValues = new NativeArray<NoiseVal>(numOfSamples, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            float3 maxPoint = noiseOrder.minPoint + noiseOrder.size;



            for (int3 iter = default; iter.z < samples.z; iter.z++)
                for (iter.y = 0; iter.y < samples.y; iter.y++)
                    for (iter.x = 0; iter.x < samples.x; iter.x++)
                    {
                        int index = GetIndex(samples, iter);
                        float3 pos = noiseOrder.minPoint + iter * sampleDist;
                        float4 noise = Noise.snoise_grad(pos * noiseOrder.noiseFactor);
                        noiseValues[index] = new NoiseVal(pos, noise);// new float4(pos, (noise.w+1f)*0.5f);

                    }

            NativeHashMap<float3, int> newLayerPosIdDict = new NativeHashMap<float3, int>(noiseOrder.sampleCount.x * noiseOrder.sampleCount.y * 2, Allocator.Temp);
            NativeHashMap<float3, int> oldLayerPosIdDict = new NativeHashMap<float3, int>(noiseOrder.sampleCount.x * noiseOrder.sampleCount.y * 2, Allocator.Temp);
            for (int3 id = default; id.z < samples.z - 1; id.z++)
            {
                NativeHashMap<float3, int> temp = oldLayerPosIdDict;
                oldLayerPosIdDict = newLayerPosIdDict;
                newLayerPosIdDict = temp;
                newLayerPosIdDict.Clear();
                for (id.y = 0; id.y < samples.y - 1; id.y++)
                {
                    for (id.x = 0; id.x < samples.x - 1; id.x++)
                    {
                        // 8 corners of the current cube
                        NativeArray<NoiseVal> cubeCorners = new NativeArray<NoiseVal>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

                        cubeCorners[0] = noiseValues[GetIndex(samples, new int3(id.x, id.y, id.z))];
                        cubeCorners[1] = noiseValues[GetIndex(samples, new int3(id.x + 1, id.y, id.z))];
                        cubeCorners[2] = noiseValues[GetIndex(samples, new int3(id.x + 1, id.y, id.z + 1))];
                        cubeCorners[3] = noiseValues[GetIndex(samples, new int3(id.x, id.y, id.z + 1))];
                        cubeCorners[4] = noiseValues[GetIndex(samples, new int3(id.x, id.y + 1, id.z))];
                        cubeCorners[5] = noiseValues[GetIndex(samples, new int3(id.x + 1, id.y + 1, id.z))];
                        cubeCorners[6] = noiseValues[GetIndex(samples, new int3(id.x + 1, id.y + 1, id.z + 1))];
                        cubeCorners[7] = noiseValues[GetIndex(samples, new int3(id.x, id.y + 1, id.z + 1))];

                        float3 topPoint = noiseValues[GetIndex(samples, new int3(id.x + 1, id.y + 1, id.z + 1))].Pos;


                        // Calculate unique index for each cube configuration.
                        // There are 256 possible values
                        // A value of 0 means cube is entirely inside surface; 255 entirely outside.
                        // The value is used to look up the edge table, which indicates which edges of the cube are cut by the isosurface.
                        int cubeIndex = 0;
                        if (cubeCorners[0].Noise < noiseOrder.isoSurfaceLevel) cubeIndex |= 1;
                        if (cubeCorners[1].Noise < noiseOrder.isoSurfaceLevel) cubeIndex |= 2;
                        if (cubeCorners[2].Noise < noiseOrder.isoSurfaceLevel) cubeIndex |= 4;
                        if (cubeCorners[3].Noise < noiseOrder.isoSurfaceLevel) cubeIndex |= 8;
                        if (cubeCorners[4].Noise < noiseOrder.isoSurfaceLevel) cubeIndex |= 16;
                        if (cubeCorners[5].Noise < noiseOrder.isoSurfaceLevel) cubeIndex |= 32;
                        if (cubeCorners[6].Noise < noiseOrder.isoSurfaceLevel) cubeIndex |= 64;
                        if (cubeCorners[7].Noise < noiseOrder.isoSurfaceLevel) cubeIndex |= 128;

                        // Create triangles for current cube configuration
                        for (int i = 0; Triangles[16 * cubeIndex + i] != -1; i += 3)
                        {
                            // Get indices of corner points A and B for each of the three edges
                            // of the cube that need to be joined to form the triangle.
                            int a0 = CornerIndexAFromEdge[Triangles[16 * cubeIndex + i]];
                            int b0 = CornerIndexBFromEdge[Triangles[16 * cubeIndex + i]];

                            int a1 = CornerIndexAFromEdge[Triangles[16 * cubeIndex + i + 1]];
                            int b1 = CornerIndexBFromEdge[Triangles[16 * cubeIndex + i + 1]];

                            int a2 = CornerIndexAFromEdge[Triangles[16 * cubeIndex + i + 2]];
                            int b2 = CornerIndexBFromEdge[Triangles[16 * cubeIndex + i + 2]];

                            VertexNormal vertexA = interpolateVerts(noiseOrder.isoSurfaceLevel, cubeCorners[a0], cubeCorners[b0]);
                            VertexNormal vertexB = interpolateVerts(noiseOrder.isoSurfaceLevel, cubeCorners[a1], cubeCorners[b1]);
                            VertexNormal vertexC = interpolateVerts(noiseOrder.isoSurfaceLevel, cubeCorners[a2], cubeCorners[b2]);

                            int len = vertices.Length;

                            int vertCId = GetVertId(ref vertices, ref normals, oldLayerPosIdDict, newLayerPosIdDict, vertexC, topPoint, ref len);

                            int vertBId = GetVertId(ref vertices, ref normals, oldLayerPosIdDict, newLayerPosIdDict, vertexB, topPoint, ref len);

                            int vertAId = GetVertId(ref vertices, ref normals, oldLayerPosIdDict, newLayerPosIdDict, vertexA, topPoint, ref len);

                            //vertices.Add(vertexC.position);
                            //vertices.Add(vertexB.position);
                            //vertices.Add(vertexA.position);

                            //normals.Add(vertexC.normal);
                            //normals.Add(vertexB.normal);
                            //normals.Add(vertexA.normal);

                            triangles.Add(vertCId);
                            triangles.Add(vertBId);
                            triangles.Add(vertAId);
                        }

                    }
                }

            }

        }).Schedule(inputDeps);
            EndSimulationCBS.AddJobHandleForProducer(job);
            return job;
    }

    private static int GetVertId(ref DynamicBuffer<float3> vertices, ref DynamicBuffer<float3> normals, NativeHashMap<float3, int> oldLayerPosIdDict, NativeHashMap<float3, int> newLayerPosIdDict, VertexNormal vertex, float3 topPoint, ref int len)
    {

        int vertId;
        if (oldLayerPosIdDict.TryGetValue(vertex.position, out vertId))
        {
            //vertId = positionIdDict.TryGetValue;
        }
        else
        {
            vertices.Add(vertex.position);
            normals.Add(vertex.normal);
            vertId = len;
            if (math.any(vertex.position == topPoint))
            {
                oldLayerPosIdDict.Add(vertex.position, vertId);
            }
            if (vertex.position.z == topPoint.z)
            {
                newLayerPosIdDict.Add(vertex.position, vertId);
            }
            len++;
        }

        return vertId;
    }

    private static VertexNormal interpolateVerts(float isoLevel, NoiseVal v1, NoiseVal v2)
    {
        float t = (isoLevel - v1.Noise) / (v2.Noise - v1.Noise);
        float3 normal = math.lerp(v1.Grad, v2.Grad, t);
        return new VertexNormal { position = v1.Pos + t * (v2.Pos - v1.Pos), normal = -normal };
    }

    private static int GetIndex(int3 sampleSize, int3 iter)
    {
        return iter.z * sampleSize.x * sampleSize.y + iter.y * sampleSize.x + iter.x;
    }

}

struct VertexNormal
{
    public float3 position;
    public float3 normal;
}

[InternalBufferCapacity(0)]
public struct Vertices : IBufferElementData
{
    public float3 position;
}

[InternalBufferCapacity(0)]
public struct Normals : IBufferElementData
{
    public float3 vector;
}

[InternalBufferCapacity(0)]
public struct Index : IBufferElementData
{
    public int id;
}

