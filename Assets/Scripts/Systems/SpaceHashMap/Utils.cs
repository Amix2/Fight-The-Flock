using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace SpaceMap
{
    public static class Utils
    {
        private const int blockLen = 1024;
        private const int blockLenSqr = blockLen * blockLen;

        public static int GetHash(float3 position, float cellSize)
        {
            int3 cellId = CellIndexFromPosition(position, cellSize);    // round down to cell index
            return cellId.x + cellId.y * blockLen + cellId.z * blockLenSqr;
        }

        public static int3 CellIndexFromPosition(float3 position, float cellSize)
        {
            return (int3)math.floor(position / cellSize);
        }

        public static float3 CellMinPointFromIndex(int3 index, float cellSize)
        {
            return (float3)index * cellSize;
        }

        public static System.Collections.Generic.IEnumerable<int> HashValuesForSphere(float3 center, float radius, float cellSize)
        {
            for (float z = center.z - radius; z <= center.z + radius; z += cellSize)
            {
                int hashZ = (int)(z / cellSize) * blockLenSqr;
                for (float y = center.y - radius; y <= center.y + radius; y += cellSize)
                {
                    int hashY = (int)(y / cellSize) * blockLen;
                    for (float x = center.x - radius; x <= center.x + radius; x += cellSize)
                    {
                        yield return (int)x + hashY + hashZ;
                    }
                }
            }
        }

        public static void CollectInSphere<T, TC>(NativeMultiHashMap<int, T> map, float cellSize, float3 center, float radius, ref TC collector)
            where T : struct, ISpaceMapValue
            where TC : struct, ICollector<T>
        {
            float radiusSqr = radius * radius;
            float3 maxPoint = math.ceil((center + radius) / cellSize) * cellSize;
            float cellSizeHalf = cellSize * 0.5f;
            for (float z = center.z - radius; z <= maxPoint.z; z += cellSize)
            {
                int hashZ = (int)math.floor(z / cellSize) * blockLenSqr;
                for (float y = center.y - radius; y <= maxPoint.y; y += cellSize)
                {
                    int hashY = (int)math.floor(y / cellSize) * blockLen;
                    for (float x = center.x - radius; x <= maxPoint.x; x += cellSize)
                    {
                        int3 index = CellIndexFromPosition(new float3(x, y, z), cellSize);
                        float3 pos = CellMinPointFromIndex(index, cellSize);
                        if (IsCubeCloser(pos + cellSizeHalf, cellSize, center, radius))
                        {
                            int hash = (int)math.floor(x / cellSize) + hashY + hashZ;
                            if (map.TryGetFirstValue(hash, out T item, out NativeMultiHashMapIterator<int> iterator))
                            {
                                do
                                {
                                    if (math.distancesq(center, item.Position) <= radiusSqr)
                                    {
                                        collector.Collect(item);
                                    }
                                } while (map.TryGetNextValue(out item, ref iterator));
                            }
                        }
                    }
                }
            }
        }

        public static void CollectInSphere(float cellSize, float3 center, float radius)
        {
            float radiusSqr = radius * radius;
            int3 minPointIndex = CellIndexFromPosition(center - radius, cellSize);
            float3 maxPoint = math.ceil((center + radius) / cellSize);
            float cellSizeHalf = cellSize * 0.5f;
            for (float z = center.z - radius; z <= maxPoint.z; z += cellSize)
            {
                int hashZ = (int)(z / cellSize) * blockLenSqr;
                for (float y = center.y - radius; y <= maxPoint.y; y += cellSize)
                {
                    int hashY = (int)(y / cellSize) * blockLen;
                    for (float x = center.x - radius; x <= maxPoint.x; x += cellSize)
                    {
                        int3 index = CellIndexFromPosition(new float3(x, y, z), cellSize);
                        float3 pos = CellMinPointFromIndex(index, cellSize);
                        if (IsCubeCloser(pos + cellSizeHalf, cellSize, center, radius))
                        {
                            int hash = (int)x + hashY + hashZ;

                            Gizmos.color = Color.red;
                            Gizmos.DrawCube(new float3(x, y, z), new float3(0.1f, 0.1f, 0.1f));

                            global::Utils.DebugDrawCube(pos, new float3(cellSize, cellSize, cellSize), Color.white);
                        }
                    }
                }
            }
        }

        private static bool IsCubeCloser(float3 nodeCenter, float nodeSize, float3 fromPosition, float distance)
        {
            /*  D = dist(nodeCenter, fromPos) | K = sqrt3 * nodeSize / 2 ? dist = sqrDistance
             *             dist > D - K
             *         dist + K > D
             *     (dist + K)^2 > D^2
             */
            float distK = distance + 1.73205080757f * nodeSize * 0.5f;
            return distK * distK > math.distancesq(nodeCenter, fromPosition);
        }
    }

    public interface ICollector<T> where T : ISpaceMapValue
    {
        void Collect(T item);
    }

    public interface ISpaceMapValue
    {
        float3 Position { get; }
    }
}