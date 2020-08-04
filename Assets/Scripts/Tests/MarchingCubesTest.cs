#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using UnityEditor;
using UnityEngine;

public class MarchingCubesTest : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float surfaceLevel;
    [Range(0.0000001f, 0.5f)]
    public float noiseFactor = 1f;

    public UnityEngine.Material material;

    public float3 size;
    public float3 sampleCount;
    public int3 count;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        //MarchingCubes.CreateChunkGameObject(new SpawnChunkData { minPoint = default, size = size, sampleCount = sampleCount, noiseFactor = noiseFactor, isoSurfaceLevel = surfaceLevel });
    }

}

[CustomEditor(typeof(MarchingCubesTest))]
public class MarchingCubesTestEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        MarchingCubesTest tar = (MarchingCubesTest)target;
        if (GUILayout.Button("Generate"))
        {
            GameObject oldMap = GameObject.Find("Map");
            if (oldMap != null) DestroyImmediate(oldMap);
            GameObject mapGO = new GameObject("Map");
            for (int x=0; x<tar.count.x; x++)
            {
                for(int y=0; y<tar.count.y; y++)
                {
                    for(int z=0; z<tar.count.z; z++)
                    {
                        GameObject gameObject = MarchingCubes.CreateChunkGameObject(new SpawnChunkData { minPoint = tar.size*new int3(x,y,z), size = tar.size, sampleCount = tar.sampleCount, noiseFactor = tar.noiseFactor, isoSurfaceLevel = tar.surfaceLevel });
                        gameObject.GetComponent<MeshRenderer>().material = tar.material;
                        gameObject.transform.SetParent(mapGO.transform);
                    }
                }
            }
        }
    }

}
#endif
