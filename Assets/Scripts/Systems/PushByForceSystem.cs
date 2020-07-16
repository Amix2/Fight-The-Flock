using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;

public class PushByForceSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dt = Time.DeltaTime;
        float maxBoidSpeed = Settings.Instance.maxBoidSpeed;
        return Entities.ForEach((ref PhysicsVelocity velocity, ref ForceComponent force, in PhysicsMass mass) =>
        {
            velocity.Linear += dt * force.Force / mass.InverseMass; // dv = dt * a = dt * F / m
            if (math.lengthsq(velocity.Linear) > maxBoidSpeed * maxBoidSpeed) velocity.Linear = math.normalize(velocity.Linear);
            force.Force = default;
        }).Schedule(inputDeps);
    }
}
