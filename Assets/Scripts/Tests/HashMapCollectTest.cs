using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HashMapCollectTest : MonoBehaviour
{
    public float radius = 1;

    private void OnDrawGizmosSelected()
    {
        SpaceMap.Utils.CollectInSphere(1, transform.position, radius);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
