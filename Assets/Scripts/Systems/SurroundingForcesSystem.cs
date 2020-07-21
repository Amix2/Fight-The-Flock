
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class SurroundingForcesSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float viewRange = Settings.Instance.boidSurroundingsViewRange;
        float separationDistance = Settings.Instance.boidSeparationDistance;
        float cohesionForceStrength = Settings.Instance.cohesionForceStrength;
        float alignmentForceStrength = Settings.Instance.alignmentForceStrength;
        float avoidanceForceStrength = Settings.Instance.sharedAvoidanceForceStrength;
        Octree octree = SpaceTree.BoidTree;
        NativeHashMap<ushort, Entity> hashMap = SpaceTree.BoidItemMap;
        ComponentDataFromEntity<LocalToWorld> componentData = GetComponentDataFromEntity<LocalToWorld>(true);

        return Entities.WithAny<BoidComponent>().WithReadOnly(hashMap).WithReadOnly(octree).WithReadOnly(componentData)
            .ForEach((Entity entity, ref ForceComponent forceComponent, ref PhysicsVelocity velocity, ref LocalToWorld localToWorld, ref Translation translation) =>
        {
            Collector collector = new Collector(componentData, hashMap, separationDistance, localToWorld.Position);
            octree.VisitInSphere(localToWorld.Position, viewRange, ref collector);

            float3 offsetToFlockCenter = collector.FlockCentre - localToWorld.Position;

            forceComponent.Force += Utils.SteerTowards(localToWorld.Up, offsetToFlockCenter) * cohesionForceStrength 
                + Utils.SteerTowards(localToWorld.Up, collector.FlockHeading) * alignmentForceStrength 
                + Utils.SteerTowards(localToWorld.Up, collector.SeparationHeading) * avoidanceForceStrength * Utils.KernelFunction(math.lengthsq(collector.SeparationHeading) / (avoidanceForceStrength* avoidanceForceStrength));
        }).Schedule(inputDeps);
    }

    private struct Collector : ICollector
    {
        ComponentDataFromEntity<LocalToWorld> componentData;
        NativeHashMap<ushort, Entity> hashMap;
        readonly float sqrSeparationDistance;
        float3 boidPosition;

        float3 flockHeading;
        float3 flockCentre;
        float3 separationHeading;
        int numFlockmates;

        public Collector(ComponentDataFromEntity<LocalToWorld> componentData, NativeHashMap<ushort, Entity> hashMap, float separationDistance, float3 boidPosition) : this()
        {
            this.componentData = componentData;
            this.hashMap = hashMap;
            this.sqrSeparationDistance = separationDistance * separationDistance;
            this.boidPosition = boidPosition;
        }

        public float3 FlockHeading { get => flockHeading; }
        public float3 FlockCentre { get => flockCentre / numFlockmates;  }
        public float3 SeparationHeading { get => separationHeading; }
        public int NumFlockmates { get => numFlockmates; }

        public void Collect(OctreeItem item)
        {
            Entity entity = hashMap[item.id];
            if(componentData.HasComponent(entity))
            {
                LocalToWorld localToWorld = componentData[entity];
                float3 direction = localToWorld.Up;
                flockHeading += direction;
                flockCentre += localToWorld.Position;
                numFlockmates++;
                float3 offset = boidPosition - localToWorld.Position; // item --->>> this
                float sqrDist = math.lengthsq(localToWorld.Position - boidPosition);
                if(sqrDist < sqrSeparationDistance)
                {
                    separationHeading += offset;
                }
            }
        }

    }
}

