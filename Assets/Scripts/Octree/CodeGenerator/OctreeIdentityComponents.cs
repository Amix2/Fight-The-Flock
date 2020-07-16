
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////
//
//		AUTO-GEMERATED CODE
//
//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////

namespace Octrees
{
	internal partial class MoveTreeItemsSystem : JobComponentSystem
	{
		private JobHandle MoveItemsForTree(JobHandle inputDeps, Octree[] trees, NativeQueue<MoveInNodes> moveInNodesQueue)
		{
			JobHandle jobHandle = inputDeps;
            NativeQueue<MoveInNodes>.ParallelWriter parallelWriter = moveInNodesQueue.AsParallelWriter();
			
			if(IdentityComponents.IdentityComponents.UsedComponentCount >= 0)
			{
				Octree tree = trees[0];
				jobHandle = Entities.ForEach((ref IdentityComponents.OctreeIdentityComponent0 c, ref OctreeIdComponent octreeId, ref OctreeMovePositionComponent translationMemory, ref Translation translation) =>
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
				Debug.AssertFormat(i < 1, "Too many trees requested, max amount: 1, requested: {0}", i);

                if (UsedComponentCount < i) UsedComponentCount = i;
                switch (i) {
                    
					case 0:  entityManager.AddComponentData(entity, new OctreeIdentityComponent0()); return;
                }
            }
        }
	    
		[GenerateAuthoringComponent] public struct OctreeIdentityComponent0 : IComponentData {};
	}
}