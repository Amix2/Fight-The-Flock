using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

public class ObstacleAvoidanceSystem : JobComponentSystem
{
    private NativeArray<float3> sphereDirections;
    private readonly int numOfDirections = 20;

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

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        NativeArray<float3> sphereDirections = this.sphereDirections;
        int numOfDirections = this.numOfDirections;
        float maxBoidObstacleAvoidance = Settings.Instance.maxBoidObstacleAvoidance;
        float minBoidObstacleDist = Settings.Instance.minBoidObstacleDist;
        float forceStrenght = Settings.Instance.avoidanceForceStrength;
        float boidObstacleProximityPush = Settings.Instance.boidObstacleProximityPush;
        BuildPhysicsWorld physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
        CollisionWorld collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;

        return Entities.WithAny<BoidComponent>().ForEach((ref ForceComponent forceComponent, in Translation translation, in LocalToWorld localToWorld) =>
            {
                float3 front = localToWorld.Up * 3f;
                float3 force = default;
                for (int i = 0; i < numOfDirections; i++)
                {
                    bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance, collisionWorld
                        , out RaycastHit raycastHit);
                    if (hit)
                    {
                        float hitDistance = math.length(translation.Value - raycastHit.Position);
                        float push;
                        if (hitDistance > minBoidObstacleDist)
                        {
                            push = Utils.KernelFunction((hitDistance - minBoidObstacleDist) / (maxBoidObstacleAvoidance - minBoidObstacleDist));
                        }
                        else
                        {
                            push = boidObstacleProximityPush * Utils.KernelFunction((hitDistance) / (minBoidObstacleDist)) + 1;
                        }
                        force += push * -sphereDirections[i];
                    }
                }
                force = Utils.SteerTowards(localToWorld.Up, force / numOfDirections * forceStrenght);
                forceComponent.Force += force;

            }).Schedule(inputDeps);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        sphereDirections.Dispose();
    }
}