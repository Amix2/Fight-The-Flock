using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Utils
{
    public static float3 SteerTowards(float3 velocity, float3 targetOffset)
    {
        float3 normTargetOffset = math.normalize(targetOffset);
        float sqrDistToTarget = math.lengthsq(targetOffset);
        float distMul = sqrDistToTarget > 1f ? 1f : sqrDistToTarget;

        if (math.lengthsq(velocity) == 0) return normTargetOffset * distMul;

        float3 normVelocity = math.normalize(velocity);
        float cosAng = math.dot(normVelocity, normTargetOffset);


        float3 directionProjection = ProjectionVectorOntoPlane(normVelocity, targetOffset);

        return directionProjection  * (1-cosAng);
    }

    /// <summary>
    /// Get projection of given vector onto given plane
    /// </summary>
    /// <param name="planeVector">Must be normalized</param>
    /// <param name="vector">Lenght influences output</param>
    /// <returns></returns>
    public static float3 ProjectionVectorOntoPlane(float3 planeVector, float3 vector)
    {
        float a = vector.x, b = vector.y, c = vector.z;
        float vx = planeVector.x, vy = planeVector.y, vz = planeVector.z;
        float t = (-vx * a - vy * b - vz * c) / (vx * vx + vy * vy + vz * vz);
        return new float3(
            t*vx+a,
            t*vy+b,
            t*vz+c
            );
    }
}
