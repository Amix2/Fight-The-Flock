using System.IO;
using UnityEngine;

/// <summary>
/// Generate OctreeIdentityComponents.cs class with Identity Components and method to move all items inside trees
/// </summary>
[CreateAssetMenu(fileName = "Code Generator", menuName = "Octree")]
public class OctreeCodeGenerator : ScriptableObject
{
    public int numberOfTrees;

    public void GenerateClass()
    {
#if UNITY_EDITOR
        GeneratorOctreeIdentityComponents generator = new GeneratorOctreeIdentityComponents(numberOfTrees);
        File.WriteAllText(Application.dataPath + @"/Scripts/Octree/CodeGenerator/OctreeIdentityComponents.cs", generator.GetFullCode());
        Debug.Log("Created " + numberOfTrees + " trees components");
#endif
    }
}