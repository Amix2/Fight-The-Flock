using Unity.Mathematics;
using Unity.Physics;

public static class PhysicUtils
{
    /// <summary>
    /// BuildPhysicsWorld physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
    /// CollisionWorld collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
    /// </summary>
    /// <param name="RayFrom"></param>
    /// <param name="RayTo"></param>
    /// <param name="collidesWithMask"></param>
    /// <param name="belongsToMask"></param>
    /// <param name="physicsWorld"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    public static bool Raycast(float3 RayFrom, float3 RayTo, uint collidesWithMask, uint belongsToMask, ref PhysicsWorld physicsWorld, out RaycastHit hit)
    {
        CollisionWorld collisionWorld = physicsWorld.CollisionWorld;
        RaycastInput input = new RaycastInput()
        {
            Filter = new CollisionFilter()
            {
                CollidesWith = collidesWithMask,
                BelongsTo = belongsToMask,
                GroupIndex = 0
            },
            Start = RayFrom,
            End = RayTo
        };

        return collisionWorld.CastRay(input, out hit);
    }
}