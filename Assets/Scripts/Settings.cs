using UnityEngine;

public class Settings : MonoBehaviour
{
    public float targetForceStrength;
    public float cohesionForceStrength;
    public float alignmentForceStrength;
    public float avoidanceForceStrength;
    public float maxBoidSpeed;
    public float minBoidSpeed;
    public float maxBoidObstacleAvoidance;
    public float minBoidObstacleDist;
    public float boidObstacleProximityPush;
    public uint boidObstacleMask;

    public static Settings Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }
}