using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////
//
//		AUTO-GEMERATED CODE
//
//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////

internal partial class MoveTreeItemsSystem : JobComponentSystem
{
    private JobHandle MoveItemsForTree(JobHandle inputDeps, Octree[] trees, NativeQueue<MoveInNodes> moveInNodesQueue)
    {
        JobHandle jobHandle = inputDeps;
        NativeQueue<MoveInNodes>.ParallelWriter parallelWriter = moveInNodesQueue.AsParallelWriter();

        if (IdentityComponents.IdentityComponents.UsedComponentCount >= 0)
        {
            Octree tree = trees[0];
            jobHandle = Entities.WithAll<IdentityComponents.OctreeIdentityComponent0>().ForEach((ref OctreeIdComponent octreeId, ref OctreeMovePositionComponent translationMemory, ref Translation translation) =>
            {
                MoveOneItemInLeaves(tree, 0, parallelWriter, ref octreeId, ref translationMemory, ref translation);
            }).Schedule(jobHandle);
            jobHandle = MoveAllItemsInNodes(tree, moveInNodesQueue, jobHandle);
        }
        else return jobHandle;

        return jobHandle;
    }
}

namespace IdentityComponents
{
    public static class IdentityComponents
    {
        public static int UsedComponentCount = -1;

        public static void SetIdentityComponent(int i, Entity entity, EntityManager entityManager)
        {
            if (i >= 1) throw new System.Exception("Too many trees requested, max amount: 1, requested: " + i);

            if (UsedComponentCount < i) UsedComponentCount = i;
            switch (i)
            {
                case 0: entityManager.AddComponentData(entity, new OctreeIdentityComponent0()); return;
            }
        }

        public static void SetIdentityComponent(int i, Entity entity, EntityCommandBuffer.ParallelWriter commandBuffer)
        {
            if (i >= 1) throw new System.Exception("Too many trees requested, max amount: 1, requested: " + i);

            if (UsedComponentCount < i) UsedComponentCount = i;
            switch (i)
            {
                case 0: commandBuffer.AddComponent(i, entity, new OctreeIdentityComponent0()); return;
            }
        }
    }

    [GenerateAuthoringComponent] public struct OctreeIdentityComponent0 : IComponentData { };
}