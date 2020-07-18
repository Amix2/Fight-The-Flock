
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;

public static class PhysicUtils
{
    /// <summary>
    /// BuildPhysicsWorld physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
    /// CollisionWorld collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
    /// </summary>
    /// <param name="RayFrom"></param>
    /// <param name="RayTo"></param>
    /// <param name="collisionWorld"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    public static bool Raycast(float3 RayFrom, float3 RayTo, uint colliderMask, CollisionWorld collisionWorld, out RaycastHit hit)
    {
        RaycastInput input = new RaycastInput()
        {
            Filter = new CollisionFilter()
            {
                CollidesWith = colliderMask, // all 1s, so all layers, collide with everything 
                BelongsTo = colliderMask,
                GroupIndex = 0
            },
            Start = RayFrom,
            End = RayTo
        };

        return collisionWorld.CastRay(input, out hit);
    }
}
