using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

[UpdateBefore(typeof(PushByForceSystem))]
public class SetTargetForceSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float3 center = new float3(0f, 0f, 0f);
        float forceStrenght = Settings.Instance.targetForceStrength;
        return Entities.ForEach((ref ForceComponent force, in Translation translation, in PhysicsVelocity velocity) =>
        {
            force.Force += Utils.SteerTowards(velocity.Linear, center - translation.Value) * forceStrenght;
        }).Schedule(inputDeps);
    }
}