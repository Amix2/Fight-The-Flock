using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

class RotateUpSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        return Entities.WithAny<BoidComponent>().ForEach((ref Rotation rotation, in PhysicsVelocity velocity) =>
        {
            float3 vel = velocity.Linear;
            if (math.lengthsq(vel) > 0)
            {
                //this is any vector ever(it literally doesnt matter what you put there as long as it's not perpendicular to your vector)
                float3 anyVector = new float3(vel.z-1, vel.x+1, vel.y);

                //This generates a random vector pernendicular to your green vector.
                float3 vector =  math.cross(velocity.Linear, anyVector);
                rotation.Value = quaternion.LookRotationSafe(math.normalize(vector), velocity.Linear);
            }
        }).Schedule(inputDeps);
    }
}
