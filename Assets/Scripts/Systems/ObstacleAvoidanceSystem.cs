using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using RaycastHit = Unity.Physics.RaycastHit;

//[UpdateBefore(typeof(PushByForceSystem))]
[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(PushByForceSystem))]
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
        uint mask = Settings.Instance.boidObstacleMask;
        BuildPhysicsWorld physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
        PhysicsWorld physicsWorld = physicsWorldSystem.PhysicsWorld;
        inputDeps = JobHandle.CombineDependencies(inputDeps, World.GetOrCreateSystem<EndFramePhysicsSystem>().GetOutputDependency());
        return Entities.WithAny<BoidComponent>().ForEach((ref ForceComponent forceComponent, in Translation translation, in LocalToWorld localToWorld) =>
            {
                float3 force = default;
                for (int i = 0; i < numOfDirections; i++)
                {
                    RaycastInput input = new RaycastInput()
                    {
                        Filter = new CollisionFilter()
                        {
                            CollidesWith = mask,
                            BelongsTo = 2,
                            GroupIndex = 0
                        },
                        Start = translation.Value,
                        End = translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance
                    };
                    //bool hit = PhysicUtils.Raycast(translation.Value, translation.Value + sphereDirections[i] * maxBoidObstacleAvoidance, mask, ref physicsWorld , out RaycastHit raycastHit);
                    bool hit = physicsWorld.CastRay(input, out RaycastHit raycastHit);

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