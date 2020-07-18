using Unity.Entities;

/// <summary>
/// Component for assignement tree item ID to given entity
/// </summary>
[GenerateAuthoringComponent]
public struct OctreeIdComponent : IComponentData
{
    public ushort itemID;

    public OctreeIdComponent(ushort itemID)
    {
        this.itemID = itemID;
    }
}