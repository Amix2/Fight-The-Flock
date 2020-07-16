using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class Utils
{
    public static float3 SteerTowards(float3 velocity, float3 targetOffset)
    {
        float sqrDistToTarget = math.lengthsq(targetOffset);
        if(sqrDistToTarget > 1f)
            return math.normalizesafe(targetOffset) - math.normalizesafe(velocity);
        else
            return (math.normalizesafe(targetOffset) - math.normalizesafe(velocity)) * sqrDistToTarget;
    }
}
