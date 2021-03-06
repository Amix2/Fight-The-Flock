﻿using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Internal;

public static class Utils
{
    public static float3 SteerTowards(float3 velocity, float3 targetOffset)
    {
        float3 normTargetOffset = math.normalizesafe(targetOffset);
        float sqrDistToTarget = math.lengthsq(targetOffset);
        float distMul = sqrDistToTarget > 1f ? 1f : sqrDistToTarget;

        if (math.lengthsq(velocity) == 0) return normTargetOffset;

        float3 normVelocity = math.normalize(velocity);
        float cosAng = math.dot(normVelocity, normTargetOffset);

        float3 directionProjection = ProjectionVectorOntoPlane(normVelocity, targetOffset);

        return math.normalizesafe(directionProjection) * math.sqrt(sqrDistToTarget) * (1 - cosAng);
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
            t * vx + a,
            t * vy + b,
            t * vz + c
            );
    }

    /// <summary>
    /// Yield all points on a sphere with radius = 1
    /// </summary>
    /// <param name="numPoints">Number of points to generate</param>
    /// <returns></returns>
    public static IEnumerable<float3> GetPoinsOnSphere(int numPoints)
    {
        float goldenRatio = (1 + math.sqrt(5)) * 0.5f;
        float angleIncrement = math.PI * 2 * goldenRatio;

        for (int i = 0; i < numPoints; i++)
        {
            float t = (float)i / numPoints;
            float inclination = math.acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = math.sin(inclination) * math.cos(azimuth);
            float y = math.sin(inclination) * math.sin(azimuth);
            float z = math.cos(inclination);
            yield return new float3(x, y, z);
        }
    }

    /// <summary>
    /// Kernel function (1-x^{0}) ^ {1}
    /// </summary>
    /// <param name="x">Should be in range [0,1]</param>
    /// <param name="steepness">{0}</param>
    /// <param name="delayGrowth">{1}</param>
    /// <returns>Value [0,1]</returns>
    public static float KernelFunction(float x, int steepness = 2, int delayGrowth = 3)
    {
#if UNITY_EDITOR
        if (x < 0 || x > 1) throw new System.Exception("KernelFunction with wrong parameter x: " + x);
#endif
        return math.pow(1 - math.pow(x, steepness), delayGrowth);
    }

    public static void DebugDrawCube(float3 minPoint, float3 size, Color color)
    {
        /*
         * 2 3
         * 0 1
         * --------
         * 6 7
         * 4 5
         */
        float3[] points = new float3[]
        {
            minPoint,
            minPoint + size * new float3(1,0,0),
            minPoint + size * new float3(0,1,0),
            minPoint + size * new float3(1,1,0),
            minPoint + size * new float3(0,0,1),
            minPoint + size * new float3(1,0,1),
            minPoint + size * new float3(0,1,1),
            minPoint + size * new float3(1,1,1),
        };
        Debug.DrawLine(points[0], points[1], color);
        Debug.DrawLine(points[0], points[2], color);
        Debug.DrawLine(points[2], points[3], color);
        Debug.DrawLine(points[1], points[3], color);

        Debug.DrawLine(points[4], points[5], color);
        Debug.DrawLine(points[4], points[6], color);
        Debug.DrawLine(points[6], points[7], color);
        Debug.DrawLine(points[5], points[7], color);

        Debug.DrawLine(points[0], points[4], color);
        Debug.DrawLine(points[1], points[5], color);
        Debug.DrawLine(points[2], points[6], color);
        Debug.DrawLine(points[3], points[7], color);

    }

}