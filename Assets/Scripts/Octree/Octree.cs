using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Octrees
{
    public struct Octree : IEquatable<Octree>
    {
        [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeList<OctreeNode> nodes;

        [NativeDisableParallelForRestriction, NativeDisableContainerSafetyRestriction]
        public NativeList<OctreeItem> values;

        public NativeArray<ushort> itemID;  // size 1 array to hold next item ID

        public readonly ushort treeID;
        private readonly float3 center;
        private readonly float size;
        private readonly int valuesPerNode;
        private const float sqrt3 = 1.73205080757f;

        #region AddItem

        /// <summary>
        /// Add items via <see cref="OctreeCreator.AddItem{T}(ushort, T, float3, Unity.Entities.Entity, Unity.Entities.EntityManager)"/>
        /// </summary>
        internal ushort AddItem(float3 position)
        {
            itemID[0] = (ushort)(itemID[0] + (ushort)1u);
            ushort id = itemID[0];
            AddItem(new OctreeItem { id = id, position = position }, 0, center, size);

            return id;
        }

        private void AddItem(OctreeItem item, int nodeIndex, float3 center, float size)
        {
            OctreeNode node = nodes[nodeIndex];
            if (node.IsDead) throw new Exception("Operation od dead node, id: " + nodeIndex);
            // Make sure this is the last node, next one is value

            if (node.NextExists && node.NextIsNode) // next is Node, find correct child and add there
            {
                int octIndex = OctIndex(item.position, center);
                int nextNodeindex = nodes[nodeIndex].nextIndex + octIndex;
                node.count++;
                nodes[nodeIndex] = node;
                AddItem(item, nextNodeindex, NewCenterFromOctIndex(center, octIndex, size), size * 0.5f);
                return;
            }
            // Node is in the last level

            if (!node.NextExists)   // Node does not have Value children, create them
            {
                // node without value
                node.nextIndex = values.Length;
                node.count = 0;
                node.nextIsNode = false;
                for (int i = valuesPerNode; i > 0; i--)
                    values.Add(new OctreeItem { id = ushort.MaxValue });
            }
            // Node has values but they might be full -> check and create new nodes if needed
            else if (nodes[nodeIndex].count == valuesPerNode)   // Node does have value children, but they are full, add next level to tree
            {
                int valuesIndex = node.nextIndex;
                node.nextIndex = nodes.Length;
                node.count = 0;
                node.nextIsNode = true; // set next is node
                nodes[nodeIndex] = node;

                // create 8 new nodes
                for (int i = 8; i > 0; i--)
                    nodes.Add(new OctreeNode { nextIndex = -1 });

                // connect values to one of the new nodes
                int octIndex = OctIndex(item.position, center);
                int nextNodeindex = nodes.Length - 8 + octIndex;
                OctreeNode nextNode = nodes[nextNodeindex];
                nextNode.nextIndex = valuesIndex; // values[valuesIndex] contains old values but we cant access them because count == 0
                nextNode.count = 0;
                nextNode.nextIsNode = false;
                nodes[nextNodeindex] = nextNode;

                for (int i = 0; i < valuesPerNode; i++)
                {   // add all old values
                    AddItem(values[valuesIndex + i], nodeIndex, center, size);
                }

                AddItem(item, nodeIndex, center, size);
                return;
            }

            // Node is in the last level and has values with space

            values[node.nextIndex + node.count] = item;
            node.count++;
            nodes[nodeIndex] = node;
        }

        #endregion AddItem

        #region RemoveItem

        /// <summary>
        /// Remove item via <see cref="OctreeCreator.RemoveItem{T}(ushort, ushort, float3)"\>
        /// </summary>
        internal void RemoveItem(ushort itemID, float3 position)
        {
            RemoveItem(itemID, position, 0, center, size);
        }

        private void RemoveItem(ushort itemID, float3 position, int nodeIndex, float3 center, float size)
        {
            OctreeNode node = nodes[nodeIndex];
            if (node.IsDead) throw new Exception("Operation od dead node, id: " + nodeIndex);

            // Make sure this is the last node, next one is value
            if (node.NextExists && node.NextIsNode)
            {
                int octIndex = OctIndex(position, center);
                int nextNodeindex = nodes[nodeIndex].nextIndex + octIndex;
                node.count--;
                nodes[nodeIndex] = node;
                RemoveItem(itemID, position, nextNodeindex, NewCenterFromOctIndex(center, octIndex, size), size * 0.5f);
                return;
            }

            // make sure values exists
            if (!node.NextExists) throw new Exception("Remove item: values are not set for node, id: " + nodeIndex);

            // values contain object
            int valuesIndex = node.nextIndex;
            int removedIndex = 0;
            while (removedIndex < node.count)
            {
                if (values[valuesIndex + removedIndex].id == itemID) break;
                removedIndex++;
            }

            if (removedIndex >= node.count) throw new Exception("Remove item: value cannot be found, id: " + nodeIndex);

            values[valuesIndex + removedIndex] = values[valuesIndex + node.count - 1];
            node.count--;
            nodes[nodeIndex] = node;
        }

        #endregion RemoveItem

        #region MoveItem

        /// <summary>
        /// Change object's position in leafs, returns False if move is not possible
        /// </summary>
        /// <param name="oldPosition">Object's old position</param>
        /// <param name="newPosition">Object's new position</param>
        /// <returns></returns>
        internal bool MoveItemInLeafs(ushort itemID, float3 oldPosition, float3 newPosition)
        {
            return MoveItem(WorkType.Parallel, itemID, oldPosition, newPosition, 0, center, size);
        }

        /// <summary>
        /// Change object's position beteween nodes, but not in leafes, returns False if move is not possible
        /// </summary>
        /// <param name="oldPosition">Object's old position</param>
        /// <param name="newPosition">Object's new position</param>
        /// <returns></returns>
        internal bool MoveItemInNodes(ushort itemID, float3 oldPosition, float3 newPosition)
        {
            return MoveItem(WorkType.Sequential, itemID, oldPosition, newPosition, 0, center, size);
        }

        /// <summary>
        /// Change object's position
        /// </summary>
        /// <param name="oldPosition">Object's old position</param>
        /// <param name="newPosition">Object's new position</param>
        /// <returns></returns>
        internal bool MoveItem(ushort itemID, float3 oldPosition, float3 newPosition)
        {
            return MoveItem(WorkType.Both, itemID, oldPosition, newPosition, 0, center, size);
        }

        private bool MoveItem(WorkType workType, ushort itemID, float3 oldPosition, float3 newPosition, int nodeIndex, float3 center, float size)
        {
            OctreeNode node = nodes[nodeIndex];
            if (node.IsDead) throw new Exception("Operation od dead node, id: " + nodeIndex);

            // Make sure this is the last node, next one is value
            if (node.NextExists && node.NextIsNode)
            {
                int octOldIndex = OctIndex(oldPosition, center);
                int octNewIndex = OctIndex(newPosition, center);
                if (octNewIndex == octOldIndex)
                {
                    // go into the same tree
                    int nextNodeindex = nodes[nodeIndex].nextIndex + octOldIndex;
                    return MoveItem(workType, itemID, oldPosition, newPosition, nextNodeindex, NewCenterFromOctIndex(center, octOldIndex, size), size * 0.5f);
                }
                else
                {   // item has to be moved into new tree
                    if (workType != WorkType.Parallel)
                    {
                        // remove old position
                        float halfSize = size * 0.5f;
                        RemoveItem(itemID, oldPosition, nodes[nodeIndex].nextIndex + octOldIndex, NewCenterFromOctIndex(center, octOldIndex, size), halfSize);
                        // add to new position
                        AddItem(new OctreeItem { id = itemID, position = newPosition }, nodes[nodeIndex].nextIndex + octNewIndex, NewCenterFromOctIndex(center, octNewIndex, size), halfSize);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            // make sure values exists
            if (!node.NextExists) throw new Exception("Remove item: values are not set for node, id: " + nodeIndex);

            if (workType != WorkType.Parallel && workType != WorkType.Both) throw new Exception("Not parallel in pure paraller part, id: " + nodeIndex);

            // values contain object, just update position
            int valuesIndex = node.nextIndex;
            for (int i = node.count - 1; i >= 0; i--)
            {
                if (values[valuesIndex + i].id == itemID)
                {
                    OctreeItem value = values[valuesIndex + i];
                    value.position = newPosition;
                    values[valuesIndex + i] = value;
                    break;
                }
            }
            return true;
        }

        #endregion MoveItem

        #region PremakeDepth

        /// <summary>
        /// Set tree to given depth
        /// </summary>
        public void PremakeDepth(int depth)
        {
            PremakeDepth(0, depth);
        }

        private void PremakeDepth(int nodeIndex, int depth)
        {
            OctreeNode node = nodes[nodeIndex];
            int nextIndex = nodes.Length;
            node.nextIndex = nextIndex;
            node.count = 0;
            node.nextIsNode = true; // set next is node
            nodes[nodeIndex] = node;
            for (int i = 8; i > 0; i--)
                nodes.Add(new OctreeNode { nextIndex = -1 });

            if (depth > 1)
            {
                for (int i = 7; i >= 0; i--)
                {
                    PremakeDepth(nextIndex + i, depth - 1);
                }
            }
        }

        #endregion PremakeDepth

        #region VisitInSphere

        /// <summary>
        /// Collect all items in given sphere
        /// </summary>
        /// <typeparam name="TC">Collector struct</typeparam>
        /// <param name="cCenter">Sphere center</param>
        /// <param name="cRadius">Sphere radius</param>
        /// <param name="collector">Collector struct reference</param>
        public void VisitInSphere<TC>(float3 cCenter, float cRadius, ref TC collector) where TC : struct, ICollector
        {
            VisitInSphere(0, center, size, cCenter, cRadius, ref collector);
        }

        private void VisitInSphere<TC>(int nodeIndex, float3 center, float size, float3 cCenter, float cRadius, ref TC collector) where TC : struct, ICollector
        {
            OctreeNode node = nodes[nodeIndex];
            if (node.IsDead) throw new Exception("Operation od dead node, id: " + nodeIndex);
            if (!node.NextIsNode)
            {
                // perform action on items
                float sqrRad = cRadius * cRadius;
                for (int i = node.count - 1; i >= 0; i--)
                {
                    if (math.distancesq(values[node.nextIndex + i].position, cCenter) <= sqrRad)
                    {
                        collector.Collect(values[node.nextIndex + i]);
                    }
                }
            }
            else
            {
                float halfSize = size * 0.5f;
                for (int octId = 7; octId >= 0; octId--)
                {
                    int childIndex = node.nextIndex + octId;
                    if (nodes[childIndex].count > 0)
                    {
                        float3 childCenter = NewCenterFromOctIndex(center, octId, size);
                        // math.distance(childCenter, cCenter) - halfDiag <= cRadius ===>
                        if (IsNodeCloser(childCenter, size, cCenter, cRadius))
                        {
                            VisitInSphere(childIndex, childCenter, halfSize, cCenter, cRadius, ref collector);
                        }
                    }
                }
            }
        }

        #endregion VisitInSphere

        #region VisitClosest

        /// <summary>
        /// Collect closest item to given position, if no items are found, the collector does nothing
        /// </summary>
        /// <typeparam name="TC"></typeparam>
        /// <param name="fromPos">Given position</param>
        /// <param name="minDistance">Smallest distance to nearest item, insert -1 to get the closest</param>
        /// <param name="maxDistance">Maximum distance</param>
        /// <param name="collector">Collector</param>
        public void VisitClosest<TC>(float3 fromPos, float minDistance, float maxDistance, ref TC collector) where TC : struct, ICollector
        {
            int closestIndex = -1;
            VisitClosest(0, center, size, fromPos, minDistance, ref maxDistance, ref closestIndex);
            if (closestIndex != -1) collector.Collect(values[closestIndex]);
        }

        private void VisitClosest(int nodeIndex, float3 center, float size, float3 fromPos, float minDistance, ref float maxDistance, ref int closestIndex)
        {
            //  maxDistance - always distance to the closest item
            OctreeNode node = nodes[nodeIndex];
            if (node.IsDead) throw new Exception("Operation od dead node, id: " + nodeIndex);
            if (!node.NextIsNode)
            {
                float sqrMaxDistance = maxDistance * maxDistance;
                // perform action on items
                for (int i = node.count - 1; i >= 0; i--)
                {
                    float sqrDist = math.distancesq(values[node.nextIndex + i].position, fromPos);
                    if (sqrDist < sqrMaxDistance && sqrDist > minDistance) // dist > 0 to prevent from collecting itself
                    {
                        maxDistance = math.sqrt(sqrDist);
                        closestIndex = node.nextIndex + i;
                    }
                }
            }
            else
            {
                float halfSize = size * 0.5f;
                int octIndex = OctIndex(fromPos, center);
                for (int octId = 0; octId < 8; octId++)
                {
                    int childOffset = octIndex + octId;
                    if (childOffset == 8)
                    {
                        octIndex -= 8;
                        childOffset -= 8;
                    }
                    int childIndex = node.nextIndex + childOffset;
                    if (nodes[childIndex].count > 0)
                    {
                        float3 childCenter = NewCenterFromOctIndex(center, childOffset, size);
                        // math.distance(childCenter, cCenter) - halfDiag <= cRadius ===>
                        if (IsNodeCloser(childCenter, size, fromPos, maxDistance))
                        {
                            VisitClosest(childIndex, childCenter, halfSize, fromPos, minDistance, ref maxDistance, ref closestIndex);
                        }
                    }
                }
            }
        }

        #endregion VisitClosest

        #region VisitAll

        public void VisitAll(Action<OctreeItem> action)
        {
            VisitAll(0, action);
        }

        private void VisitAll(int nodeIndex, Action<OctreeItem> action)
        {
            if (nodes[nodeIndex].IsDead) throw new Exception("Operation on dead node, id: " + nodeIndex);
            if (!nodes[nodeIndex].NextExists) return;
            if (nodes[nodeIndex].NextIsNode)
            {
                for (int i = 7; i >= 0; i--)
                {
                    VisitAll(nodes[nodeIndex].nextIndex + i, action);
                }
            }
            else
            {   // next is value
                for (int i = nodes[nodeIndex].count - 1; i >= 0; i--)
                {
                    action.Invoke(values[nodes[nodeIndex].nextIndex + i]);
                }
            }
        }

        #endregion VisitAll

        #region Utils

        private static int OctIndex(float3 position, float3 center)
        {
            int octIndex = 0;

            if (position.x > center.x) octIndex += 1;
            if (position.y <= center.y) octIndex += 2;
            if (position.z > center.z) octIndex += 4;
            /* 1st face:    0 | 1
             *              2 | 3
             *
             * 2nd face     4 | 5
             *  (z++)       6 | 7
             */
            return octIndex;
        }

        private static float3 NewCenterFromOctIndex(float3 center, int index, float size)
        {
            float s = size / 4;
            // 01234567
            if (index < 4)
            {   // 0123
                if (index < 2)
                {   // 01
                    if (index == 0)
                    {   // 0
                        center.x -= s;
                        center.y += s;
                        center.z -= s;
                    }
                    else
                    {   // 1
                        center.x += s;
                        center.y += s;
                        center.z -= s;
                    }
                }
                else
                {   // 23
                    if (index == 2)
                    {   // 2
                        center.x -= s;
                        center.y -= s;
                        center.z -= s;
                    }
                    else
                    {   // 3
                        center.x += s;
                        center.y -= s;
                        center.z -= s;
                    }
                }
            }
            else
            {   // 4567
                if (index < 6)
                {   // 45
                    if (index == 4)
                    {   // 4
                        center.x -= s;
                        center.y += s;
                        center.z += s;
                    }
                    else
                    {   // 5
                        center.x += s;
                        center.y += s;
                        center.z += s;
                    }
                }
                else
                {   // 67
                    if (index == 6)
                    {   // 6
                        center.x -= s;
                        center.y -= s;
                        center.z += s;
                    }
                    else
                    {   // 7
                        center.x += s;
                        center.y -= s;
                        center.z += s;
                    }
                }
            }
            return center;
        }

        /// <returns>distance > dist(nodeCenter - fromPos) - sqrt3*nodeSize/2</returns>
        private static bool IsNodeCloser(float3 nodeCenter, float nodeSize, float3 fromPosition, float distance)
        {
            /*  D = dist(nodeCenter, fromPos) | K = sqrt3 * nodeSize / 2 ? dist = sqrDistance
             *             dist > D - K
             *         dist + K > D
             *     (dist + K)^2 > D^2
             */
            float distK = distance + sqrt3 * nodeSize * 0.5f;
            return distK * distK > math.distancesq(nodeCenter, fromPosition);
        }

        private void InitTree()
        {
            nodes.Add(new OctreeNode { nextIndex = -1 });
        }

        internal Octree(ushort ID, float3 center, float size, int valuesPerNode, int initValuesSize = 1, int initNodesSize = 1)
        {
            values = new NativeList<OctreeItem>(initValuesSize, Allocator.Persistent);
            nodes = new NativeList<OctreeNode>(initNodesSize, Allocator.Persistent);
            itemID = new NativeArray<ushort>(1, Allocator.Persistent);
            itemID[0] = (ushort)0u;
            this.center = center;
            this.size = size;
            this.valuesPerNode = valuesPerNode;
            this.treeID = ID;
            InitTree();
        }

        public void Dispose()
        {
            values.Dispose();
            nodes.Dispose();
            itemID.Dispose();
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void Draw()
        {
            DrawLinesOutside(center, size);
            Draw(0, center, size);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void Draw(int nodeInd, float3 center, float size)
        {
#if UNITY_EDITOR
            OctreeNode node = nodes[nodeInd];
            if (node.IsDead) return;
            UnityEditor.Handles.Label(center, "" + node.count);
            if (node.NextIsNode)
            {
                DrawLinesOutside(center, size);
                DrawLinesInside(center, size);
                for (int octId = 0; octId < 8; octId++)
                {
                    Draw(node.nextIndex + octId, NewCenterFromOctIndex(center, octId, size), size * 0.5f);
                }
            }
            else if (node.count > 0)
            {
                //DrawLines(center, size);
                for (int i = 0; i < node.count; i++)
                {
                    DrawItem(values[node.nextIndex + i].position);
                }
            }
#endif
        }

        private void DrawLinesOutside(float3 center, float size)
        {
            float s = size * 0.5f;
            Debug.DrawLine(center + new float3(-s, -s, -s), center + new float3(-s, s, -s));
            Debug.DrawLine(center + new float3(-s, s, -s), center + new float3(s, s, -s));
            Debug.DrawLine(center + new float3(s, s, -s), center + new float3(s, -s, -s));
            Debug.DrawLine(center + new float3(s, -s, -s), center + new float3(-s, -s, -s));

            Debug.DrawLine(center + new float3(-s, -s, s), center + new float3(-s, s, s));
            Debug.DrawLine(center + new float3(-s, s, s), center + new float3(s, s, s));
            Debug.DrawLine(center + new float3(s, s, s), center + new float3(s, -s, s));
            Debug.DrawLine(center + new float3(s, -s, s), center + new float3(-s, -s, s));

            Debug.DrawLine(center + new float3(-s, -s, -s), center + new float3(-s, -s, s));
            Debug.DrawLine(center + new float3(-s, s, -s), center + new float3(-s, s, s));
            Debug.DrawLine(center + new float3(s, s, -s), center + new float3(s, s, s));
            Debug.DrawLine(center + new float3(s, -s, -s), center + new float3(s, -s, s));
        }

        private void DrawLinesInside(float3 center, float size)
        {
            float s = size * 0.5f;
            Debug.DrawLine(center + new float3(0, 0, -s), center + new float3(0, 0, s), Color.red);
            Debug.DrawLine(center + new float3(-s, 0, 0), center + new float3(s, 0, 0), Color.red);
            Debug.DrawLine(center + new float3(0, -s, 0), center + new float3(0, s, 0), Color.red);
        }

        private void DrawItem(float3 position)
        {
            Gizmos.DrawSphere(position, 0.1f);
        }

        public void PrintAllNodes()
        {
            Debug.Log("Tree print nodes " + nodes.Length);
            for (int i = 0; i < nodes.Length; i++)
            {
                OctreeNode node = nodes[i];
                Debug.Log(i + " - count: " + node.count + " | next: " + node.nextIndex);
            }
            Debug.Log("-------");
        }

        public bool Equals(Octree other)
        {
            return this.treeID == other.treeID;
        }

        private enum WorkType
        {
            Sequential,
            Parallel,
            Both
        }

        #endregion Utils
    }

    public struct OctreeNode
    {
        public int nextIndex;   // == -1 -> node does not have next | == -2 node is dead
        public int count; // >=0 items, ==-1 node
        public bool nextIsNode;

        public bool IsDead => nextIndex == -2;
        public bool NextExists => nextIndex >= 0;
        public bool NextIsNode => nextIsNode;
    }

    public struct OctreeItem : IEquatable<OctreeItem>
    {
        public ushort id;
        public float3 position;

        public bool Equals(OctreeItem other)
        {
            if (id != ushort.MaxValue && other.id != ushort.MaxValue) return id == other.id;
            throw new Exception("OctreeItem::Equals on not initialized item");
        }
    }

    public interface ICollector
    {
        void Collect(OctreeItem item);
    }
}