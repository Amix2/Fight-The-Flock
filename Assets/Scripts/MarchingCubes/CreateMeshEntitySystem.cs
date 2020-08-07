using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(CreateMeshDataSystem))]
public class CreateMeshEntitySystem : JobComponentSystem
{
    private EntityCommandBufferSystem EndSimulationCBS;

    protected override void OnCreate()
    {
        base.OnCreate();
        EndSimulationCBS = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        EntityCommandBuffer.ParallelWriter commandBufferPR = EndSimulationCBS.CreateCommandBuffer().AsParallelWriter();
        // shared component - single thread
        Entities.WithoutBurst().WithStructuralChanges()
            .WithAll<NoiseBufferTag>().ForEach((Entity thisEntity, ref NoiseOrderComponent noiseOrder
            , ref DynamicBuffer<Vertices> verticesBuf, ref DynamicBuffer<Normals> normalsBuf, ref DynamicBuffer<Index> trianglesBuf
            , in MeshDataComponent meshData) =>
        {
            Debug.Log(verticesBuf[0].position);
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[verticesBuf.Length];
            Vector3[] normals = new Vector3[normalsBuf.Length];
            int[] triangles = new int[trianglesBuf.Length];
            verticesBuf.Reinterpret<Vector3>().AsNativeArray().CopyTo(vertices);
            normalsBuf.Reinterpret<Vector3>().AsNativeArray().CopyTo(normals);
            trianglesBuf.Reinterpret<int>().AsNativeArray().CopyTo(triangles);
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            Entity meshEntity = EntityManager.CreateEntity();
            RenderMesh renderMesh = new RenderMesh { castShadows = UnityEngine.Rendering.ShadowCastingMode.On, layer = 0, receiveShadows = true, needMotionVectorPass = true, material = meshData.material, mesh = mesh };
            EntityManager.AddSharedComponentData(meshEntity, renderMesh);
            EntityManager.AddComponentData(meshEntity, new Translation { Value = noiseOrder.minPoint });
            EntityManager.AddComponentData(meshEntity, new Rotation { Value = quaternion.identity });
            EntityManager.AddComponent<PhysicsCollider>(meshEntity);
            EntityManager.AddComponentData(meshEntity, new RenderBounds { Value = mesh.bounds.ToAABB() });
            EntityManager.AddComponent<LocalToWorld>(meshEntity);
            EntityManager.AddComponent<Rotation>(meshEntity);
            EntityManager.DestroyEntity(thisEntity);
        }).Run();

        return inputDeps;
    }
}