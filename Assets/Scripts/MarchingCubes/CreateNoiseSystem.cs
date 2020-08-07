
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct NoiseBufferTag : IComponentData
{

}

//public class CreateNoiseSystem : JobComponentSystem
//{
//    private EntityCommandBufferSystem EndSimulationCBS;

//    protected override void OnCreate()
//    {
//        base.OnCreate();
//        EndSimulationCBS = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        //PerformanceMonitor.DEBUG_BeginSample(inputDeps, "CreateNoiseSystem");
//        EntityCommandBuffer.ParallelWriter commandBufferPR = EndSimulationCBS.CreateCommandBuffer().AsParallelWriter();

//        JobHandle job =  Entities.WithoutBurst().WithNone<NoiseBufferTag>().ForEach((Entity thisEntity, int entityInQueryIndex, ref NoiseOrderComponent noiseOrder) =>
//        {
//            DynamicBuffer<NoiseVal> noiseVals = commandBufferPR.AddBuffer<NoiseVal>(entityInQueryIndex, thisEntity);
//            commandBufferPR.AddComponent(entityInQueryIndex, thisEntity, new NoiseBufferTag());
//            float3 sampleDist = noiseOrder.size / noiseOrder.sampleCount;
//            int3 samples = (int3)(noiseOrder.size / sampleDist) + 1;
//            int numOfSamples = samples.x * samples.y * samples.z;
//            float3 maxPoint = noiseOrder.minPoint + noiseOrder.size;

//            noiseVals.ResizeUninitialized(numOfSamples);
//            for (int3 iter = default; iter.z < samples.z; iter.z++)
//                for (iter.y = 0; iter.y < samples.y; iter.y++)
//                    for (iter.x = 0; iter.x < samples.x; iter.x++)
//                    {
//                        int index = GetIndex(samples, iter);
//                        float3 pos = noiseOrder.minPoint + iter * sampleDist;
//                        float4 noise = Noise.snoise_grad(pos * noiseOrder.noiseFactor);
//                        noiseVals[index] = new NoiseVal(pos, noise);// new float4(pos, (noise.w+1f)*0.5f);
//                    }
//        }).Schedule(inputDeps);
//        job.Complete();

//        EndSimulationCBS.AddJobHandleForProducer(job);
//        //PerformanceMonitor.DEBUG_EndSample(job);
//        return job;
//    }

//    private static int GetIndex(int3 sampleSize, int3 iter)
//    {
//        return iter.z * sampleSize.x * sampleSize.y + iter.y * sampleSize.x + iter.x;
//    }
//}
