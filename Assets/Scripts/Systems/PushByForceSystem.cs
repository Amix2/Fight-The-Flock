using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;

public class PushByForceSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dt = Time.DeltaTime;
        return Entities.ForEach((ref PhysicsVelocity velocity, ref ForceComponent force, in PhysicsMass mass) =>
        {
            velocity.Linear += dt * force.Force / mass.InverseMass; // dv = dt * a = dt * F / m
            force.Force = default;
        }).Schedule(inputDeps);
    }
}
