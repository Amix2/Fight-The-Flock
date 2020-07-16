using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public class PushByForceSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dt = Time.DeltaTime;
        float maxBoidSpeed = Settings.Instance.maxBoidSpeed;
        float minBoidSpeed = Settings.Instance.minBoidSpeed;
        return Entities.ForEach((ref PhysicsVelocity velocity, ref ForceComponent force, in PhysicsMass mass, in LocalToWorld localToWorld) =>
        {
            velocity.Linear += dt * force.Force; // dv = dt * a = dt * F / m
            if (math.lengthsq(velocity.Linear) == 0) velocity.Linear = localToWorld.Up;
            if (math.lengthsq(velocity.Linear) > maxBoidSpeed * maxBoidSpeed) velocity.Linear = math.normalize(velocity.Linear) * maxBoidSpeed;
            if (math.lengthsq(velocity.Linear) < minBoidSpeed * minBoidSpeed) velocity.Linear = math.normalize(velocity.Linear) * minBoidSpeed;
            force.Force = default;
        }).Schedule(inputDeps);
    }
}
