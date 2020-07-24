using Octrees;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SpawnBoidsSystem : JobComponentSystem
{
    private EndInitializationEntityCommandBufferSystem beginSimulationEntityCBS;
    private Random random;

    protected override void OnCreate()
    {
        base.OnCreate();
        random = new Random(52);
        GizmosDrawer.OnDrawGizmosAction += OnGiznos;
        beginSimulationEntityCBS = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GizmosDrawer.OnDrawGizmosAction -= OnGiznos;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Random random = new Random((uint)(Time.ElapsedTime * 100000));
            //EntityCommandBuffer commandBuffer = beginSimulationEntityCBS.CreateCommandBuffer();
            //EntityCommandBuffer.Concurrent ecb = commandBuffer.ToConcurrent();
            Entities.WithStructuralChanges().ForEach((int entityInQueryIndex, ref BoidSpawnerComponent prefabComponent) =>
            {
                float3 spawnOffset = prefabComponent.Offsets;
                for (int i = 0; i < prefabComponent.SpawnNumber; i++)
                {
                    float3 offset = new float3(
                        spawnOffset.x * (2 * random.NextFloat() - 1),
                        spawnOffset.y * (2 * random.NextFloat() - 1),
                        spawnOffset.z * (2 * random.NextFloat() - 1)
                        );
                    //Entity entity = ecb.Instantiate(entityInQueryIndex, prefabComponent.Entity);
                    Entity entity = EntityManager.Instantiate(prefabComponent.Entity);

                    float3 position = prefabComponent.Center + offset;
                    //ecb.SetComponent(entityInQueryIndex, entity, new Translation { Value = position });
                    EntityManager.AddComponentData(entity, new Translation { Value = position });
                    OctreeCreator.AddItem(SpaceTree.BoidTreeID, entity, position, entity, EntityManager);
                }
            }).Run();//.Schedule(inputDeps);
                     //commandBuffer.Playback(EntityManager);
                     // beginSimulationEntityCBS.AddJobHandleForProducer(inputDeps);
        }
        return inputDeps;
    }

    private void OnGiznos()
    {
        Entities.WithoutBurst().ForEach((ref BoidSpawnerComponent prefabComponent) =>
        {
            Gizmos.DrawWireCube(prefabComponent.Center, 2 * prefabComponent.Offsets);
        }).Run();
    }
}