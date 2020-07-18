using Unity.Mathematics;
using UnityEngine;

public class SteerTowardsTest : MonoBehaviour
{
    public float velocityValue;
    public float maxAngleChange;
    public float3 center;

    private void OnDrawGizmos()
    {
        Vector3 velocity = transform.up * velocityValue;
        Gizmos.DrawLine(transform.position, transform.position + velocity);
        Gizmos.DrawSphere(center, 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + maxAngleChange * (Vector3)Utils.SteerTowards(velocity, center - (float3)transform.position));
    }
}