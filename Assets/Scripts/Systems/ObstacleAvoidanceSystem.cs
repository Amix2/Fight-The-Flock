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

            inputDeps = JobHandle.CombineDependencies(inputDeps, World.GetOrCreateSystem<EndFramePhysicsSystem>().GetOutputDependency());

            inputDeps = Entities.WithoutBurst().WithAny<BoidComponent>().ForEach((ref ForceComponent forceComponent, in Translation translation, in LocalToWorld localToWorld) =>
            {
                if (PhysicUtils.Raycast(translation.Value, translation.Value + localToWorld.Up * rayAvoidDistance * 0.8f, mask, 2, physicsWorld, out RaycastHit raycastHitTmp)) // Ray avoidance system
                {   
                    float3 forceRay = default;
                    float closestRayHitDistance = float.MaxValue;
                    float bestHitAngle = float.MaxValue;
                    float3 bestDir = localToWorld.Up;
                    bool hitGot = false;
                    for (int i = 0; i < rayNumOfDirections; i++)
                    {
                        float3 currenetDir = math.rotate(localToWorld.Value, rayShereDirections[i]);
                        float currentAngle = 1f - math.dot(currenetDir, localToWorld.Up);
                        if (currentAngle + 0.01f > bestHitAngle) continue;

                        bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + currenetDir * rayAvoidDistance, mask, 2, physicsWorld, out RaycastHit raycastHit);
                        //Debug.DrawLine(translation.Value, translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance, Color.blue);
                        if (!hit)
                        {
                            bestHitAngle = currentAngle;
                            bestDir = currenetDir;
                        }
                        else
                        {
                            float hitDistance = math.length(translation.Value - raycastHit.Position);
                            if (closestRayHitDistance > hitDistance) closestRayHitDistance = hitDistance;
                            hitGot = true;
                            Debug.DrawLine(translation.Value, raycastHit.Position, Color.green);
                        }
                    }
                    //Debug.DrawLine(translation.Value, translation.Value + bestDir * maxBoidObstacleAvoidance, Color.yellow);
                    if (hitGot)
                    {
                        //force = Utils.SteerTowards(localToWorld.Up, translation.Value + bestDir * forceStrenght * Utils.KernelFunction(closestHitDistance / maxBoidObstacleAvoidance));
                        forceRay = Utils.SteerTowards(localToWorld.Up, translation.Value + bestDir * rayForceStrenght * Utils.KernelFunction(closestRayHitDistance / rayAvoidDistance));
                        //Debug.DrawLine(translation.Value, translation.Value + bestDir * forceStrenght * Utils.KernelFunction(closestHitDistance / maxBoidObstacleAvoidance), Color.cyan);
                        Debug.DrawLine(translation.Value, translation.Value + forceRay, Color.red);

                        forceComponent.Force += forceRay;
                    }
                }

                float3 force = default;

                for (int i = 0; i < proxyNumOfDirections; i++)
                {
                    float3 currenetDir = proxyShereDirections[i];

                    bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + currenetDir * proxyAvoidDistance, mask, 2, physicsWorld, out RaycastHit raycastHit);
   
                    if (hit)
                    {
                        float hitDistance = math.length(translation.Value - raycastHit.Position);

                        if (hitDistance > proxyAvoidDistance)
                        {
                            continue;    // Raycast is not 100% accurate, sometimes it finds hits outsice of given radius, we just ignore them
                        }
                        float pushFactor = Utils.KernelFunction(hitDistance / proxyAvoidDistance);
                        Debug.DrawLine(translation.Value, raycastHit.Position, Color.cyan);

                        force += pushFactor * -currenetDir;
                    }
                }
                Debug.DrawRay(translation.Value, force, Color.red);

                forceComponent.Force += Utils.SteerTowards(localToWorld.Up, force * proxyForceStrenght);

            }).Schedule(inputDeps);
            
            //   {
            //       if (PhysicUtils.Raycast(translation.Value, translation.Value + localToWorld.Up * maxBoidObstacleAvoidance*0.8f, mask, 2, physicsWorld, out RaycastHit raycastHitTmp))
            //       {
            //           float3 force = default;
            //           float closestHitDistance = float.MaxValue;
            //           float bestHitAngle = float.MaxValue;
            //           float3 bestDir = localToWorld.Up;
            //           bool hitGot = false;
            //           float maxPushFactor = 0;
            //           for (int i = 0; i < numOfDirections; i++)
            //           {
            //                float3 currenetDir = math.rotate(localToWorld.Value, sphereDirections[i]);
            //               float currentAngle = 1f - math.dot(currenetDir, localToWorld.Up);
            //               if (currentAngle + 0.01f > bestHitAngle) continue;

            //               bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + currenetDir * maxBoidObstacleAvoidance, mask, 2, physicsWorld, out RaycastHit raycastHit);
            //               //Debug.DrawLine(translation.Value, translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance, Color.blue);
            //               if (!hit)
            //               {
            //                   bestHitAngle = currentAngle;
            //                   bestDir = currenetDir;
            //               }
            //               else
            //               {
            //                   float hitDistance = math.length(translation.Value - raycastHit.Position);
            //                   if (closestHitDistance > hitDistance) closestHitDistance = hitDistance;
            //                   hitGot = true;
            //                   //Debug.DrawLine(translation.Value, raycastHit.Position, Color.green);
            //               }
            //               {
            //                   //if (hit)
            //                   //{
            //                   //    float hitDistance = math.length(translation.Value - raycastHit.Position);

            //                   //    if (hitDistance > maxBoidObstacleAvoidance)
            //                   //    {
            //                   //        continue;    // Raycast is not 100% accurate, sometimes it finds hits outsice of given radius, we just ignore them
            //                   //    }
            //                   //    float pushFactor;
            //                   //    if (hitDistance > minBoidObstacleDist)
            //                   //    {
            //                   //        pushFactor = Utils.KernelFunction((hitDistance - minBoidObstacleDist) / (maxBoidObstacleAvoidance - minBoidObstacleDist));
            //                   //        //Debug.DrawLine(translation.Value, raycastHit.Position, Color.yellow);
            //                   //    }
            //                   //    else
            //                   //    {
            //                   //        pushFactor = boidObstacleProximityPush * Utils.KernelFunction(hitDistance / minBoidObstacleDist) + 1;
            //                   //        //Debug.DrawLine(translation.Value, raycastHit.Position, Color.red);

            //                   //    }
            //                   //    force += pushFactor * -sphereDirections[i];
            //                   //    if (maxPushFactor < pushFactor) maxPushFactor = pushFactor;
            //                   //}
            //                   //else
            //                   //{
            //                   //    Debug.DrawLine(translation.Value, translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance, Color.white);

            //                   //}
            //               }
            //           }
            //           //Debug.DrawLine(translation.Value, translation.Value + bestDir * maxBoidObstacleAvoidance, Color.yellow);
            //           if (hitGot)
            //           {
            //               //force = Utils.SteerTowards(localToWorld.Up, translation.Value + bestDir * forceStrenght * Utils.KernelFunction(closestHitDistance / maxBoidObstacleAvoidance));
            //               force = Utils.SteerTowards(localToWorld.Up, translation.Value + bestDir * forceStrenght * Utils.KernelFunction(closestHitDistance / maxBoidObstacleAvoidance));
            //               //Debug.DrawLine(translation.Value, translation.Value + bestDir * forceStrenght * Utils.KernelFunction(closestHitDistance / maxBoidObstacleAvoidance), Color.cyan);
            //               //Debug.DrawLine(translation.Value, translation.Value + force, Color.red);

            //               forceComponent.Force += force;
            //           }
            //       }
            //       {
            //           float3 force = default;
            //           for (int i = 0; i < numOfDirections; i++)
            //           {
            //               bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + sphereDirections[i] * minBoidObstacleDist, mask, 2, physicsWorld, out RaycastHit raycastHit);
            //               if (hit)
            //               {
            //                   float hitDistance = math.length(translation.Value - raycastHit.Position);
            //                   force += -1 * (sphereDirections[i]) * boidObstacleProximityPush * Utils.KernelFunction(hitDistance / minBoidObstacleDist);
            //               }
            //           }
            //           //Debug.DrawLine(translation.Value, translation.Value + force, Color.cyan);
            //           forceComponent.Force += force;
            //       }
            //   }).Schedule(inputDeps);

            //World.GetOrCreateSystem<EndFramePhysicsSystem>().AddInputDependency(inputDeps);

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