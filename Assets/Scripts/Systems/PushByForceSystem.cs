using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    public class PushByForceSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dt = UnityEngine.Time.fixedDeltaTime;
            float maxBoidSpeed = Settings.Instance.maxBoidSpeed;
            float minBoidSpeed = Settings.Instance.minBoidSpeed;
            Entities.WithoutBurst().ForEach((ref PhysicsVelocity velocity, ref ForceComponent force, in PhysicsMass mass, in LocalToWorld localToWorld) =>
            {
                velocity.Linear += dt * force.Force; // dv = dt * a = dt * F / m
                if (math.lengthsq(velocity.Linear) == 0) velocity.Linear = localToWorld.Up;
                if (math.lengthsq(velocity.Linear) > maxBoidSpeed * maxBoidSpeed) velocity.Linear = math.normalize(velocity.Linear) * maxBoidSpeed;
                if (math.lengthsq(velocity.Linear) < minBoidSpeed * minBoidSpeed) velocity.Linear = math.normalize(velocity.Linear) * minBoidSpeed;
                Debug.DrawRay(localToWorld.Position, force.Force, Color.red);
                force.Force = new float3(0, 0, 0);
            }).Run();//.Schedule(inputDeps);

            return inputDeps;
        }
    }
}