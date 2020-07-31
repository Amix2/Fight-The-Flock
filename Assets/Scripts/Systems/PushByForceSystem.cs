using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    [UpdateAfter(typeof(EndFramePhysicsSystem))]
    public class PushByForceSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float dt = UnityEngine.Time.fixedDeltaTime;
            float maxBoidSpeed = Settings.Instance.Boid.maxSpeed;
            float minBoidSpeed = Settings.Instance.Boid.minSpeed;
            Transform transform = Marker.Transform;
            float3 pos = transform.position;
            quaternion quaternion = transform.rotation;
            float sqrMaxSpeed = maxBoidSpeed * maxBoidSpeed;
                float sqrMinSpeed = minBoidSpeed * minBoidSpeed;
            
            inputDeps = Entities.ForEach((ref PhysicsVelocity velocity, ref Rotation rotation, ref ForceComponent force, ref PhysicsMass mass, ref LocalToWorld localToWorld, ref Translation translation) =>
            {
                //translation.Value = pos;
                //rotation.Value = quaternion;
                velocity.Linear += dt * force.Force; // dv = dt * a = dt * F / m
                if (math.lengthsq(velocity.Linear) > sqrMaxSpeed) velocity.Linear = math.normalize(velocity.Linear) * maxBoidSpeed;
                else if (math.lengthsq(velocity.Linear) == 0) velocity.Linear = localToWorld.Up;
                else if (math.lengthsq(velocity.Linear) < sqrMinSpeed) velocity.Linear = math.normalize(velocity.Linear) * minBoidSpeed;
                
                force.Force = new float3(0, 0, 0);
            }).Schedule(inputDeps);

            return inputDeps;
        }
    }
}