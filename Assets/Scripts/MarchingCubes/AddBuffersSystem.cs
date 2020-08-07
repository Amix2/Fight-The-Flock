
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;

[UpdateBefore(typeof(CreateMeshDataSystem))]
class AddBuffersSystem : ComponentSystem
{
    private EntityCommandBufferSystem EndSimulationCBS;

    protected override void OnCreate()
    {
        base.OnCreate();
        EndSimulationCBS = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    protected override void OnUpdate()
    {
        //EntityCommandBuffer.ParallelWriter commandBufferPR = EndSimulationCBS.CreateCommandBuffer().AsParallelWriter();
        // int entityInQueryIndex,
        Entities.WithNone<NoiseBufferTag>().ForEach((Entity thisEntity, ref NoiseOrderComponent noiseOrder) =>
        {
            DynamicBuffer<Normals> normalsBuf = EntityManager.AddBuffer<Normals>(thisEntity);// commandBufferPR.AddBuffer<Normals>(entityInQueryIndex, thisEntity);
            DynamicBuffer<Vertices> verticesBuf = EntityManager.AddBuffer<Vertices>(thisEntity);// commandBufferPR.AddBuffer<Vertices>(entityInQueryIndex, thisEntity);
            DynamicBuffer<Index> trianglesBuf = EntityManager.AddBuffer<Index>(thisEntity);//  commandBufferPR.AddBuffer<Index>(entityInQueryIndex, thisEntity);
            EntityManager.AddComponent<BuffersPresentTag>(thisEntity);//  commandBufferPR.AddComponent<BuffersPresentTag>(entityInQueryIndex, thisEntity);
            //verticesBuf.EnsureCapacity(2);
            //trianglesBuf.EnsureCapacity(3);

        });//.Schedule(inputDeps);

        //EndSimulationCBS.AddJobHandleForProducer(job);

        //return inputDeps;
    }
}

public struct BuffersPresentTag : IComponentData { };

