using Boids;
using SpaceMap;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

[UpdateBefore(typeof(PushByForceSystem))]
public class SurroundingForcesSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float viewRange = Settings.Instance.Boid.surroundingsViewRange;
        float separationDistance = Settings.Instance.Boid.separationDistance;
        float cohesionForceStrength = Settings.Instance.Boid.Forces.cohesionForceStrength;
        float alignmentForceStrength = Settings.Instance.Boid.Forces.alignmentForceStrength;
        float avoidanceForceStrength = Settings.Instance.Boid.Forces.sharedAvoidanceForceStrength;
        NativeMultiHashMap<int, BoidData> boidMap = BoidHashMap.BoidMap;
        float cellSize = BoidHashMap.cellSize;

        return Entities.WithAny<BoidComponent>().WithReadOnly(boidMap).ForEach((Entity entity, ref ForceComponent forceComponent, ref PhysicsVelocity velocity, ref LocalToWorld localToWorld, ref Translation translation) =>
            {
                Collector collector = new Collector(separationDistance, translation.Value);

                SpaceMap.Utils.CollectInSphere(boidMap, cellSize, translation.Value, viewRange, ref collector);
                if (collector.NumFlockmates > 0)
                {

                    float3 offsetToFlockCenter = collector.FlockCentre - localToWorld.Position;

                    forceComponent.Force += Utils.SteerTowards(localToWorld.Up, offsetToFlockCenter) * cohesionForceStrength
                        + Utils.SteerTowards(localToWorld.Up, collector.FlockHeading) * alignmentForceStrength
                        + Utils.SteerTowards(localToWorld.Up, collector.SeparationHeading) * avoidanceForceStrength;
                }
            }).Schedule(inputDeps);
    }

    private struct Collector : SpaceMap.ICollector<BoidData>
    {
        private readonly float sqrSeparationDistance;
        private float3 boidPosition;

        private float3 flockHeading;
        private float3 flockCentre;
        private float3 separationHeading;
        private int numFlockmates;

        public Collector(float separationDistance, float3 boidPosition) : this()
        {
            this.sqrSeparationDistance = separationDistance * separationDistance;
            this.boidPosition = boidPosition;
        }

        public float3 FlockHeading { get => flockHeading; }
        public float3 FlockCentre { get => flockCentre / numFlockmates; }
        public float3 SeparationHeading { get => separationHeading; }
        public int NumFlockmates { get => numFlockmates; }

        public void Collect(BoidData item)
        {
            float3 direction = math.normalizesafe(item.velocity);
            flockHeading += direction;
            flockCentre += item.position;
            numFlockmates++;
            float3 offset = boidPosition - item.position; // item --->>> this
            float sqrDist = math.lengthsq(item.position - boidPosition);
            if (sqrDist < sqrSeparationDistance && sqrDist > 0f)
            {
                separationHeading += math.normalize(offset) * Utils.KernelFunction(sqrDist / sqrSeparationDistance, 1);
            }
        }
    }
}