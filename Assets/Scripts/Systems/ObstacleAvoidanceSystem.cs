using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace Boids
{
    [UpdateAfter(typeof(ExportPhysicsWorld))]
    [UpdateBefore(typeof(EndFramePhysicsSystem))]
    [UpdateBefore(typeof(PushByForceSystem))]
    public class ObstacleAvoidanceSystem : JobComponentSystem
    {
        private NativeArray<float3> sphereDirections;
        private readonly int numOfDirections = 100;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<float3> sphereDirections = this.sphereDirections;
            int numOfDirections = this.numOfDirections;
            float maxBoidObstacleAvoidance = Settings.Instance.maxBoidObstacleAvoidance;
            float minBoidObstacleDist = Settings.Instance.minBoidObstacleDist;
            float forceStrenght = Settings.Instance.wallAvoidanceForceStrength;
            float boidObstacleProximityPush = Settings.Instance.boidObstacleProximityPush;
            uint mask = Settings.Instance.boidObstacleMask;
            float cosAngle = Settings.Instance.boidObstacleMaxAngle;

            BuildPhysicsWorld physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
            PhysicsWorld physicsWorld = physicsWorldSystem.PhysicsWorld;

            inputDeps = Entities.WithoutBurst().WithAny<BoidComponent>().ForEach((ref ForceComponent forceComponent, in Translation translation, in LocalToWorld localToWorld) =>
               {
                   if (!PhysicUtils.Raycast(translation.Value, translation.Value + localToWorld.Up * maxBoidObstacleAvoidance*0.5f, mask, 2, ref physicsWorld, out RaycastHit raycastHitTmp))
                   {
                       return;
                   }
                   float3 force = default;
                   float closestHitDistance = float.MaxValue;
                   float bestHitAngle = float.MaxValue;
                   float3 bestDir = localToWorld.Up;
                   bool hitGot = false;
                   float maxPushFactor = 0;
                   for (int i = 0; i < numOfDirections; i++)
                   {
                       float currentAngle = 1f - math.dot(sphereDirections[i], localToWorld.Up);
                       if (currentAngle > bestHitAngle) continue;

                       bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance, mask, 2, ref physicsWorld, out RaycastHit raycastHit);
                       Debug.DrawLine(translation.Value, translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance, Color.blue);
                       if (!hit)
                       {
                           bestHitAngle = currentAngle;
                           bestDir = sphereDirections[i];
                       }
                       else
                       {
                           float hitDistance = math.length(translation.Value - raycastHit.Position);
                           if (closestHitDistance > hitDistance) closestHitDistance = hitDistance;
                           hitGot = true;
                           Debug.DrawLine(translation.Value, raycastHit.Position, Color.green);
                       }
                       {
                           //if (hit)
                           //{
                           //    float hitDistance = math.length(translation.Value - raycastHit.Position);

                           //    if (hitDistance > maxBoidObstacleAvoidance)
                           //    {
                           //        continue;    // Raycast is not 100% accurate, sometimes it finds hits outsice of given radius, we just ignore them
                           //    }
                           //    float pushFactor;
                           //    if (hitDistance > minBoidObstacleDist)
                           //    {
                           //        pushFactor = Utils.KernelFunction((hitDistance - minBoidObstacleDist) / (maxBoidObstacleAvoidance - minBoidObstacleDist));
                           //        //Debug.DrawLine(translation.Value, raycastHit.Position, Color.yellow);
                           //    }
                           //    else
                           //    {
                           //        pushFactor = boidObstacleProximityPush * Utils.KernelFunction(hitDistance / minBoidObstacleDist) + 1;
                           //        //Debug.DrawLine(translation.Value, raycastHit.Position, Color.red);

                           //    }
                           //    force += pushFactor * -sphereDirections[i];
                           //    if (maxPushFactor < pushFactor) maxPushFactor = pushFactor;
                           //}
                           //else
                           //{
                           //    Debug.DrawLine(translation.Value, translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance, Color.white);

                           //}
                       }
                   }
                   Debug.DrawLine(translation.Value, translation.Value + bestDir * maxBoidObstacleAvoidance, Color.yellow);
                   if (hitGot)
                   {
                       //force = Utils.SteerTowards(localToWorld.Up, translation.Value + bestDir * forceStrenght * Utils.KernelFunction(closestHitDistance / maxBoidObstacleAvoidance));
                       force = Utils.SteerTowards(localToWorld.Up, translation.Value + bestDir * forceStrenght * Utils.KernelFunction(closestHitDistance / maxBoidObstacleAvoidance));
                       Debug.DrawLine(translation.Value, translation.Value + bestDir * forceStrenght * Utils.KernelFunction(closestHitDistance / maxBoidObstacleAvoidance), Color.cyan);
                       Debug.DrawLine(translation.Value, translation.Value + force, Color.red);

                       forceComponent.Force += force;
                   }
               }).Schedule(inputDeps);

            World.GetOrCreateSystem<EndFramePhysicsSystem>().AddInputDependency(inputDeps);

            return inputDeps;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            sphereDirections = new NativeArray<float3>(numOfDirections, Allocator.Persistent);
            int i = 0;
            foreach (float3 dir in Utils.GetPoinsOnSphere(numOfDirections))
            {
                sphereDirections[i] = dir;
                i++;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            sphereDirections.Dispose();
        }
    }
}