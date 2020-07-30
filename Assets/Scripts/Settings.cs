using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class Settings : MonoBehaviour
{
    [SerializeField]
    public BoidSettings Boid;

    public static Settings Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
        FixedRateUtils.EnableFixedRateWithCatchUp(World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SimulationSystemGroup>(), Time.fixedDeltaTime);
    }

    private void Update()
    {
    }

    [Serializable]
    public struct BoidSettings
    {
        public float maxSpeed;
        public float minSpeed;
        public float surroundingsViewRange;
        public float separationDistance;

        public ForcesSettings Forces;
        public ObstacleAvoidanceSettings ObstacleAvoidance;

    }

    [Serializable]
    public struct ForcesSettings
    {
        public float targetForceStrength;
        public float cohesionForceStrength;
        public float alignmentForceStrength;
        public float sharedAvoidanceForceStrength;
        public float wallRayAvoidForceStrength;
        public float wallProximityAvoidForceStrength;
    }
    [Serializable]
    public struct ObstacleAvoidanceSettings
    {
        public uint boidObstacleMask;
        public float rayAvoidDistance;
        public float proxymityAvoidDistance;
    }
}
