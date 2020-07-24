using Unity.Entities;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public float targetForceStrength;
    public float cohesionForceStrength;
    public float alignmentForceStrength;
    public float sharedAvoidanceForceStrength;
    public float wallAvoidanceForceStrength;
    public float maxBoidSpeed;
    public float minBoidSpeed;
    public float maxBoidObstacleAvoidance;
    public float minBoidObstacleDist;
    public float boidObstacleProximityPush;
    public float boidSurroundingsViewRange;
    public float boidSeparationDistance;
    public uint boidObstacleMask;

    public static Settings Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
        FixedRateUtils.EnableFixedRateWithCatchUp(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SimulationSystemGroup>(), Time.fixedDeltaTime);
    }
}