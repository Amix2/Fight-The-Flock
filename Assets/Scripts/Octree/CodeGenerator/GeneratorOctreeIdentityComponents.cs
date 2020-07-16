namespace Octrees
{
    public class GeneratorOctreeIdentityComponents
    {
        private readonly int numberOfTrees;

        public GeneratorOctreeIdentityComponents(int numberOfTrees)
        {
            this.numberOfTrees = numberOfTrees;
        }

        private string GetOneMoveItemsForTreeLoop(int nr)
        {
            string text = string.Format(@"
			if(IdentityComponents.IdentityComponents.UsedComponentCount >= {0})
			{{
				Octree tree = trees[{0}];
				jobHandle = Entities.ForEach((ref IdentityComponents.OctreeIdentityComponent{0} c, ref OctreeIdComponent octreeId, ref OctreeMovePositionComponent translationMemory, ref Translation translation) =>
				{{
					MoveOneItemInLeaves(tree, {0}, parallelWriter, ref octreeId, ref translationMemory, ref translation);
				}}).Schedule(jobHandle);
                jobHandle = MoveAllItemsInNodes(tree, moveInNodesQueue, jobHandle);
			}}
            else return jobHandle;", nr);
            return text;
        }

        private string GetAllMoveItemsForTreeLoop(int nr)
        {
            string text = "";
            for (int i = 0; i < nr; i++)
                text += GetOneMoveItemsForTreeLoop(i);
            return text;
        }

        private string GetOneAddComponent(int nr)
        {
            string text = string.Format(@"
					case {0}:  entityManager.AddComponentData(entity, new OctreeIdentityComponent{0}()); return;", nr);
            return text;
        }

        private string GetAllAddComponent(int nr)
        {
            string text = "";
            for (int i = 0; i < nr; i++)
                text += GetOneAddComponent(i);
            return text;
        }

        private string GetOneComponent(int nr)
        {
            string text = string.Format(@"
		[GenerateAuthoringComponent] public struct OctreeIdentityComponent{0} : IComponentData {{}};", nr);
            return text;
        }

        private string GetAllComponent(int nr)
        {
            string text = "";
            for (int i = 0; i < nr; i++)
                text += GetOneComponent(i);
            return text;
        }

        private string GetTemplate(int nr)
        {
            string text = string.Format(@"
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
{{
	internal partial class MoveTreeItemsSystem : JobComponentSystem
	{{
		private JobHandle MoveItemsForTree(JobHandle inputDeps, Octree[] trees, NativeQueue<MoveInNodes> moveInNodesQueue)
		{{
			JobHandle jobHandle = inputDeps;
            NativeQueue<MoveInNodes>.ParallelWriter parallelWriter = moveInNodesQueue.AsParallelWriter();
			//AllMoveItemsForTreeLoop

			return jobHandle;
		}}
	}}
	namespace IdentityComponents
	{{
		public static class IdentityComponents
		{{
			public static int UsedComponentCount = -1;
			public static void SetIdentityComponent(int i, Entity entity, EntityManager entityManager)
			{{
				Debug.AssertFormat(i < {0}, ""Too many trees requested, max amount: {0}, requested: {{0}}"", i);

                if (UsedComponentCount < i) UsedComponentCount = i;
                switch (i) {{
                    //AllAddComponent
                }}
            }}
        }}
	    //AllComponent
	}}
}}", nr);
            return text;
        }

        public string GetFullCode()
        {
            return GetTemplate(numberOfTrees)
                .Replace("//AllMoveItemsForTreeLoop", GetAllMoveItemsForTreeLoop(numberOfTrees))
                .Replace("//AllAddComponent", GetAllAddComponent(numberOfTrees))
                .Replace("//AllComponent", GetAllComponent(numberOfTrees));
        }
    }
}