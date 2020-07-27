using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace SpaceMap
{
    public struct BoidData : ISpaceMapValue
    {
        public Entity entity;
        public float3 position;
        public float3 velocity;

        public float3 Position { get => position; }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class BoidHashMap : JobComponentSystem
    {
        public static NativeMultiHashMap<int, BoidData> BoidMap;
        public static float cellSize = 2f;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            PerformanceMonitor.DEBUG_BeginSample(inputDeps, "BoidHashMap");
            BoidHashMap.cellSize = Settings.Instance.boidSurroundingsViewRange;
            //EntityArchetype archetype = EntityManager.CreateArchetype(typeof(BoidComponent), typeof(Translation), typeof(PhysicsVelocity));
            EntityQuery query = EntityManager.CreateEntityQuery(typeof(BoidComponent), typeof(Translation), typeof(PhysicsVelocity));
            BoidMap.Clear();
            int queryCount = query.CalculateEntityCount();
            if (queryCount > BoidMap.Capacity)
            {
                BoidMap.Capacity = queryCount;
            }

            NativeMultiHashMap<int, BoidData>.ParallelWriter parallelWriter = BoidMap.AsParallelWriter();
            float cellSize = BoidHashMap.cellSize;

            inputDeps =  Entities.WithAny<BoidComponent>().ForEach((Entity entity, ref Translation translation, ref PhysicsVelocity velocity) =>
            {
                parallelWriter.Add(Utils.GetHash(translation.Value, cellSize), new BoidData { entity = entity, position = translation.Value, velocity = velocity.Linear });
            }).Schedule(inputDeps);

            PerformanceMonitor.DEBUG_EndSample(inputDeps);

            return inputDeps;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            BoidMap = new NativeMultiHashMap<int, BoidData>(0, Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            BoidMap.Dispose();
        }

    }
}