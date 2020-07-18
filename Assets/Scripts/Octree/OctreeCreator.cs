using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Octrees
{
    public static class OctreeCreator
    {
        public static List<Octree> octrees = new List<Octree>();
        private static ushort NextKey => (ushort)octrees.Count;

        public static Dictionary<ushort, IOctreeMap> octreeMaps = new Dictionary<ushort, IOctreeMap>();

        public static Octree GetTree(ushort id)
        {
            return octrees[id];
        }

        public static NativeHashMap<ushort, T> GetIdMap<T>(ushort id) where T : struct, IEquatable<T>
        {
            return ((OctreeIdMap<T>)octreeMaps[id]).map;
        }

        /// <summary>
        /// Create octree and add to the list
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="center">Tree center position</param>
        /// <param name="size">Tree size in all directions</param>
        /// <param name="valuesPerNode">Leaf size, number of items in the last level</param>
        /// <param name="initValuesSize">Initial size of item array</param>
        /// <param name="initNodesSize">Initial size of node array</param>
        /// <returns></returns>
        public static ushort CreateTree<T>(float3 center, float size, int valuesPerNode = 16, int initValuesSize = 64, int initNodesSize = 64) where T : struct, IEquatable<T>
        {
            return AddTree<T>(new Octree(OctreeCreator.NextKey, center, size, valuesPerNode, initValuesSize, initNodesSize));
        }

        private static ushort AddTree<T>(Octree tree) where T : struct, IEquatable<T>
        {
            ushort key = (ushort)octrees.Count;
            octrees.Insert(key, tree); // copy
            octreeMaps.Add(key, new OctreeIdMap<T> { map = new NativeHashMap<ushort, T>(8, Allocator.Persistent) });
            return key;
        }

        /// <summary>
        /// Add item to given tree, inserts item into item-id map
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="treeID">Tree id</param>
        /// <param name="item">item</param>
        /// <param name="position">items's position</param>
        /// <param name="entity">Entity which generaes position update</param>
        public static void AddItem<T>(ushort treeID, T item, float3 position, Entity entity, EntityManager entityManager) where T : struct, IEquatable<T>
        {
            Octree tree = octrees[treeID];
            ushort itemID = tree.AddItem(position);
            OctreeIdMap<T> map = (OctreeIdMap<T>)octreeMaps[treeID];
            map.map.Add(itemID, item);

            entityManager.AddComponentData(entity, new OctreeMovePositionComponent { Value = position });
            entityManager.AddComponentData(entity, new OctreeIdComponent { itemID = itemID });
            IdentityComponents.IdentityComponents.SetIdentityComponent(treeID, entity, entityManager);
        }

        /// <summary>
        /// Add item to given tree, inserts item into item-id map
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="treeID">Tree id</param>
        /// <param name="item">item</param>
        /// <param name="position">items's position</param>
        /// <param name="entity">Entity which generaes position update</param>
        public static void AddItem<T>(ushort treeID, T item, float3 position, Entity entity, EntityCommandBuffer.Concurrent commandBuffer) where T : struct, IEquatable<T>
        {
            Octree tree = octrees[treeID];
            ushort itemID = tree.AddItem(position);
            OctreeIdMap<T> map = (OctreeIdMap<T>)octreeMaps[treeID];
            map.map.Add(itemID, item);

            commandBuffer.AddComponent(treeID, entity, new OctreeMovePositionComponent { Value = position });
            commandBuffer.AddComponent(treeID, entity, new OctreeIdComponent { itemID = itemID });
            IdentityComponents.IdentityComponents.SetIdentityComponent(treeID, entity, commandBuffer);
        }

        public static void PremakeDepth(ushort treeID, int depth)
        {
            Octree tree = octrees[treeID];
            tree.PremakeDepth(depth);
        }

        /// <summary>
        /// Remove item from given tree
        /// </summary>
        /// <param name="treeID">Tree id</param>
        /// <param name="itemID">Item id</param>
        /// <param name="position">Item's position, schould be taken from OctreeMovePositionComponent</param>
        public static void RemoveItem<T>(ushort treeID, ushort itemID, float3 position) where T : struct, IEquatable<T>
        {
            Octree tree = octrees[treeID];
            tree.RemoveItem(itemID, position);
            OctreeIdMap<T> map = (OctreeIdMap<T>)octreeMaps[treeID];
            map.map.Remove(itemID);
        }

        public static void Dispose()
        {
            foreach (IOctreeMap map in octreeMaps.Values)
            {
                map.Dispose();
            }

            foreach (Octree tree in octrees)
            {
                tree.Dispose();
            }
        }
    }

    public interface IOctreeMap
    {
        void Dispose();
    }

    public struct OctreeIdMap<T> : IOctreeMap where T : struct, IEquatable<T>
    {
        public NativeHashMap<ushort, T> map;

        public void Dispose()
        {
            map.Dispose();
        }
    }
}