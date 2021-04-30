using OpenTK;
using System.Collections.Generic;
using VoxelGame.Worlds;

namespace VoxelGame
{
    /// <summary>
    /// Simple A* algorithm implementation on 3D voxel grid
    /// </summary>
    /// <remarks>
    /// Based on: www.redblobgames.com/pathfinding/a-star/implementation.html
    /// </remarks>
    public class AStarPathFinder
    {
        /// <summary>
        /// Back traces path from end to start
        /// </summary>
        /// <param name="start">Start</param>
        /// <param name="end">End</param>
        /// <param name="parents">List of parents for each node</param>
        /// <returns>List of waypoints</returns>
        private static List<Vector3> TracePath(Vector3 start, Vector3 end, Dictionary<Vector3, Vector3> parents)
        {
            var list = new List<Vector3>();
            var current = end;
            while (current != start)
            {
                current = parents[current];
                list.Insert(0, current);
            }
            list.Add(end);
            return list;
        }

        /// <summary>
        /// Checks whether given voxel is empty or not
        /// </summary>
        /// <param name="voxel">The voxel position</param>
        /// <returns>True if voxel is empty; otherwise false</returns>
        private static bool CanOccupyVoxel(Vector3 voxel)
        {
            var chunkPosition = voxel.ToChunkPosition();
            var voxelPosition = voxel.ToChunkSpace();

            if (World.Instance.TryGetChunkAtPosition((int)chunkPosition.X, (int)chunkPosition.Z, out var chunk))
                return chunk.GetBlockID((int)voxelPosition.X, (int)voxelPosition.Y, (int)voxelPosition.Z) <= 0;
            return false;
        }

        /// <summary>
        /// Generates list of valid neighbors that can be visited
        /// </summary>
        /// <param name="current">Current position</param>
        /// <returns>List of valid neighbors</returns>
        private static IEnumerable<Vector3> GetNeighbors(Vector3 current)
        {
            // Check all neighbors
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    for (int z = -1; z <= 1; z++)
                    {
                        if (x == 0 && y == 0 && z == 0) continue;
                        var next = current + new Vector3(x, y, z);
                        // Straight
                        if (x == 0 || z == 0)
                        {
                            if (CanOccupyVoxel(next) &&
                                CanOccupyVoxel(next + Vector3.UnitY) &&
                                !CanOccupyVoxel(next - Vector3.UnitY)
                                )
                                yield return next;
                        }
                        else // Diagonal
                        {

                            if (CanOccupyVoxel(next) &&
                                CanOccupyVoxel(next + Vector3.UnitY) &&
                                !CanOccupyVoxel(next - Vector3.UnitY) &&
                                CanOccupyVoxel(new Vector3(current.X + x, current.Y, current.Z)) && // Additional check on one axis in direction of movement
                                CanOccupyVoxel(new Vector3(current.X, current.Y, current.Z + z))    // Additional check on second axis in direction of movement
                                )
                                yield return next;
                        }
                    }
        }

        /// <summary>
        /// Finds walkable path from start to end with maximum cost
        /// </summary>
        /// <param name="start">Starting point in world space</param>
        /// <param name="end">End point in world space</param>
        /// <param name="maxCost">Maximum cost of the path</param>
        /// <returns>If path found returns list of voxels to walk through; otherwise null</returns>
        public static List<Vector3> FindPath(Vector3 start, Vector3 end, float maxCost = 120)
        {
            var parents = new Dictionary<Vector3, Vector3>();
            var costs = new Dictionary<Vector3, float>();
            var openset = new PriorityQueue<Vector3>();
            var closedset = new HashSet<Vector3>();

            openset.Enqueue(start, 0);
            parents[start] = start;
            costs[start] = Vector3.Distance(start, end);

            while (openset.Count > 0)
            {
                var current = openset.Dequeue();
                if (Vector3.Distance(current, end) < 0.5f)
                    return TracePath(start, current, parents);

                closedset.Add(current);


                foreach (var next in GetNeighbors(current))
                {
                    if (closedset.Contains(next))continue;

                    var cost = (int)(costs[current] + Vector3.Distance(current, next));
                    
                    if (cost > maxCost) continue;
                    
                    if (!costs.ContainsKey(next) || cost < costs[next])
                    {
                        costs[next] = cost;
                        var priority = cost + Vector3.Distance(next, end);
                        openset.Enqueue(next, priority);
                        parents[next] = current;
                    }
                }
            }

            return null;
        }
    }
}
