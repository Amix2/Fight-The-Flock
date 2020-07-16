using Octrees;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpaceTree : MonoBehaviour
{
    public static ushort BoidTreeID { get; private set; }
    public static Octree BoidTree => OctreeCreator.GetTree(BoidTreeID);
    public static NativeHashMap<ushort, Entity> BoidItemMap => OctreeCreator.GetIdMap<Entity>(BoidTreeID);

    public float3 boidTreeCenter;
    public float boidTreeSize;
    public int boidTreeItemSize;

    // Start is called before the first frame update
    void Start()
    {
        BoidTreeID = OctreeCreator.CreateTree<Entity>(boidTreeCenter, boidTreeSize, boidTreeItemSize);
    }

    private void OnDestroy()
    {
        OctreeCreator.Dispose();
    }

}
