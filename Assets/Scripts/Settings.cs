using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public float targetForceStrength;
    public float cohesionForceStrength;
    public float alignmentForceStrength;
    public float avoidanceForceStrength;
    public float maxBoidSpeed;


    public static Settings Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
    }
}
