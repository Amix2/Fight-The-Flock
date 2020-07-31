using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

// github.com/Unity-Technologies/EntityComponentSystemSamples/blob/master/UnityPhysicsSamples/Assets/Demos/3.%20Query/Scripts/RaycastWithCustomCollector/RaycastWithCustomCollector.cs
namespace Boids
{
    [UpdateAfter(typeof(ExportPhysicsWorld))]
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    [UpdateBefore(typeof(PushByForceSystem))]
    public class ObstacleAvoidanceSystem : JobComponentSystem
    {
        private NativeArray<float3> raySphereDirections;
        private readonly int rayNumOfDirections = 100;
        private NativeArray<float3> proxySphereDirections;
        private readonly int proxyNumOfDirections = 20;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            NativeArray<float3> rayShereDirections = this.raySphereDirections;
            NativeArray<float3> proxyShereDirections = this.proxySphereDirections;
            int rayNumOfDirections = this.rayNumOfDirections;
            int proxyNumOfDirections = this.proxyNumOfDirections;
            float rayForceStrenght = Settings.Instance.Boid.Forces.wallRayAvoidForceStrength;
            float proxyForceStrenght = Settings.Instance.Boid.Forces.wallProximityAvoidForceStrength;
            uint mask = Settings.Instance.Boid.ObstacleAvoidance.boidObstacleMask;
            float rayAvoidDistance = Settings.Instance.Boid.ObstacleAvoidance.rayAvoidDistance;
            float proxyAvoidDistance = Settings.Instance.Boid.ObstacleAvoidance.proxymityAvoidDistance;

            BuildPhysicsWorld physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
            PhysicsWorld physicsWorld = physicsWorldSystem.PhysicsWorld;

            //inputDeps = JobHandle.CombineDependencies(inputDeps, World.GetOrCreateSystem<EndFramePhysicsSystem>().GetOutputDependency());

            inputDeps = Entities.WithAny<BoidComponent>().ForEach((ref ForceComponent forceComponent, in Translation translation, in LocalToWorld localToWorld) =>
            {
                if (PhysicUtils.Raycast(translation.Value, translation.Value + localToWorld.Up * rayAvoidDistance * 0.8f, mask, 2, physicsWorld)) // Ray avoidance system
                {
                    float3 forceRay = default;
                    float sqrClosestRayHitDistance = float.MaxValue;
                    float bestHitAngle = float.MaxValue;
                    float3 bestDir = localToWorld.Up;
                    bool hitGot = false;
                    for (int i = 0; i < rayNumOfDirections; i++)
                    {
                        float3 currenetDir = math.rotate(localToWorld.Value, rayShereDirections[i]);
                        float currentAngle = 1f - math.dot(currenetDir, localToWorld.Up);
                        if (currentAngle + 0.0001f > bestHitAngle) continue;

                        bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + currenetDir * rayAvoidDistance, mask, 2, physicsWorld, out RaycastHit raycastHit);
                        if (!hit)
                        {
                            bestHitAngle = currentAngle;
                            bestDir = currenetDir;
                        }
                        else
                        {
                            float sqrHitDistance = math.lengthsq(translation.Value - raycastHit.Position);
                            if (sqrClosestRayHitDistance > sqrHitDistance) sqrClosestRayHitDistance = sqrHitDistance;
                            hitGot = true;
                        }
                    }
                    if (hitGot)
                    {
                        forceRay = Utils.SteerTowards(localToWorld.Up, translation.Value + bestDir * rayForceStrenght * Utils.KernelFunction(sqrClosestRayHitDistance / (rayAvoidDistance * rayAvoidDistance), 1));

                        forceComponent.Force += forceRay;
                    }
                }

                float3 force = default;

                for (int i = 0; i < proxyNumOfDirections; i++)
                {
                    float3 currenetDir = math.rotate(localToWorld.Value, proxyShereDirections[i]);

                    bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + currenetDir * proxyAvoidDistance, mask, 2, physicsWorld, out RaycastHit raycastHit);
   
                    if (hit)
                    {
                        float sqrHitDistance = math.lengthsq(translation.Value - raycastHit.Position);

                        float sqrProxyAvoidDistance = proxyAvoidDistance * proxyAvoidDistance;
                        if (sqrHitDistance > sqrProxyAvoidDistance)
                        {
                            continue;    // Raycast is not 100% accurate, sometimes it finds hits outsice of given radius, we just ignore them
                        }
                        float pushFactor = Utils.KernelFunction(sqrHitDistance / sqrProxyAvoidDistance, 1);

                        force += pushFactor * -currenetDir;
                    }
                }

                forceComponent.Force += Utils.SteerTowards(localToWorld.Up, force * proxyForceStrenght);

            }).Schedule(inputDeps);
            
            return inputDeps;
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            raySphereDirections = new NativeArray<float3>(rayNumOfDirections, Allocator.Persistent);
            int i = 0;
            foreach (float3 dir in Utils.GetPoinsOnSphere(rayNumOfDirections))
            {
                raySphereDirections[i] = dir;
                i++;
            }
            proxySphereDirections = new NativeArray<float3>(proxyNumOfDirections, Allocator.Persistent);
            i = 0;
            foreach (float3 dir in Utils.GetPoinsOnSphere(proxyNumOfDirections))
            {
                proxySphereDirections[i] = dir;
                i++;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            raySphereDirections.Dispose();
            proxySphereDirections.Dispose();
        }
    }
}