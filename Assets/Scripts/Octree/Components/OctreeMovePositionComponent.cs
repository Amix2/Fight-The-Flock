using Unity.Entities;
using Unity.Mathematics;

namespace Octrees
{
    /// <summary>
    /// Component used in moving items inside tree
    /// </summary>
    [GenerateAuthoringComponent]
    public struct OctreeMovePositionComponent : IComponentData
    {
        public float3 Value;
    }
}