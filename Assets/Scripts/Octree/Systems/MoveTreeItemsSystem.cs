using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Octrees
{
    /// <summary>
    /// System for moving components inside the tree
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    internal partial class MoveTreeItemsSystem : JobComponentSystem
    {
        private NativeQueue<MoveInNodes> moveInNodesQueue;  // Persistent Queue

        /// <summary>
        /// Update method to schedule all jobs for moving items inside the tree
        /// </summary>
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            Octree[] trees = OctreeCreator.octrees.ToArray();

            JobHandle jobHandle = MoveItemsForTree(inputDeps, trees, moveInNodesQueue);

            return jobHandle;
        }

        /// <summary>
        /// Method for moving items inside 1 node, only updates positions without changeing tree structure, parallel work
        /// </summary>
        protected static void MoveOneItemInLeaves(Octree tree, ushort treeId, NativeQueue<MoveInNodes>.ParallelWriter parallelWriter, ref OctreeIdComponent octreeId, ref OctreeMovePositionComponent translationMemory, ref Translation translation)
        {
            bool3 dif = translationMemory.Value != translation.Value;
            if (dif.x || dif.y || dif.z)
            {
                bool moved = tree.MoveItemInLeafs(octreeId.itemID, translationMemory.Value, translation.Value);
                if (!moved)
                {
                    parallelWriter.Enqueue(new MoveInNodes
                    {
                        newPosition = translation.Value,
                        oldPosition = translationMemory.Value,
                        itemId = octreeId.itemID,
                    });
                }

                translationMemory.Value = translation.Value;
            }
        }

        /// <summary>
        /// Method for moving items between different nodes, only 1 thread per tree
        /// </summary>
        protected static JobHandle MoveAllItemsInNodes(Octree tree, NativeQueue<MoveInNodes> moveInNodesQueue, JobHandle jobHandle)
        {
            MoveItemsInNodes moveJob = new MoveItemsInNodes
            {
                itemsQueue = moveInNodesQueue,
                octree = tree
            };

            return moveJob.Schedule(jobHandle);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            moveInNodesQueue = new NativeQueue<MoveInNodes>(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            moveInNodesQueue.Dispose();
        }

        protected struct MoveItemsInNodes : IJob
        {
            public NativeQueue<MoveInNodes> itemsQueue;
            public Octree octree;

            public void Execute()
            {
                while (itemsQueue.TryDequeue(out MoveInNodes item))
                {
                    octree.MoveItemInNodes(item.itemId, item.oldPosition, item.newPosition);
                }
            }
        }

        protected struct MoveInNodes
        {
            public float3 newPosition;
            public float3 oldPosition;
            public ushort itemId;
        }
    }
}