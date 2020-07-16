using UnityEditor;
using UnityEngine;

/// <summary>
/// Button to generate octree classes for given number of trees
/// </summary>
[CustomEditor(typeof(OctreeCodeGenerator))]
public class CodeGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (OctreeCodeGenerator)target;

        if (GUILayout.Button("Generate Octree Identity Components", GUILayout.Height(40)))
        {
            script.GenerateClass();
        }
    }
}