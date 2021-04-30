using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Blocks;
using VoxelGame.Worlds;

namespace VoxelGame.Items
{
    public class AStarTesterItem : Item
    {
        public override string Key => "Item_AStar_Test";

        public AStarTesterItem()
        {
            GenerateGraphics();
            ItemDatabase.RegisterItem(this);
        }

        public override void OnInteract(Vector3 position, Chunk chunk)
        {
            const int SEARCH_RADIUS = 32;
            var xo = (int)position.X;
            var yo = (int)position.Y;
            var zo = (int)position.Z;

            Vector3? from = null;
            Vector3? to = null;

            for (int x = -SEARCH_RADIUS; x <= SEARCH_RADIUS; x++)
            {
                for (int z = -SEARCH_RADIUS; z <= SEARCH_RADIUS; z++)
                {
                    for (int y = -SEARCH_RADIUS; y <= SEARCH_RADIUS; y++)
                    {
                        if (chunk.GetBlockID(xo + x, yo + y, zo + z) == GameBlocks.GLOWSTONE.ID)
                        {
                            from = new Vector3(chunk.Position.X * Chunk.WIDTH + position.X, position.Y, chunk.Position.Y * Chunk.WIDTH + position.Z);
                            to = from.Value + new Vector3(x, y + 1, z);
                            break;
                        }
                    }
                    if (from != null) break;
                }
                if (from != null) break;
            }

            if (from != null && to != null)
            {
                Debug.Log($"AStar Search from: {from} to: {to}", DebugLevel.Warning);

                var result = AStarPathFinder.FindPath(from.Value, to.Value);
                if (result == null)
                    Debug.Log($"AStar Path not found", DebugLevel.Warning);
                else
                {
                    Debug.Log($"AStar Path found", DebugLevel.Warning);
                    for (int i = 0; i < result.Count; i++)
                    {
                        var last = i == result.Count - 1;
                        var block = result[i];

                        var chunkPosition = block.ToChunkPosition();
                        var voxelPosition = block.ToChunkSpace();

                        if (World.Instance.TryGetChunkAtPosition((int)chunkPosition.X, (int)chunkPosition.Z, out var c))
                            if (c.GetBlockID((int)voxelPosition.X, (int)voxelPosition.Y, (int)voxelPosition.Z) <= 0)
                                c.PlaceBlock((int)voxelPosition.X, (int)voxelPosition.Y, (int)voxelPosition.Z, GameBlocks.LOG_OAK, true);
                    }
                }
            }
            else
            {
                Debug.Log($"AStar Search target not found", DebugLevel.Warning);
            }
        }
    }


    public class AStarPathFinder
    {

        private static List<Vector3> TracePath(Vector3 start, Vector3 goal, Dictionary<Vector3, Vector3> parents)
        {
            var list = new List<Vector3>();
            var current = goal;
            while (current != start)
            {
                current = parents[current];
                list.Insert(0, current);
            }
            list.Add(goal);
            return list;
        }

        private static bool CanOccupyVoxel(Vector3 voxel)
        {
            var chunkPosition = voxel.ToChunkPosition();
            var voxelPosition = voxel.ToChunkSpace();

            if (World.Instance.TryGetChunkAtPosition((int)chunkPosition.X, (int)chunkPosition.Z, out var chunk))
                return chunk.GetBlockID((int)voxelPosition.X, (int)voxelPosition.Y, (int)voxelPosition.Z) <= 0;
            return false;
        }

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

        public static List<Vector3> FindPath(Vector3 start, Vector3 end, float maxCost = 120)
        {
            // www.redblobgames.com/pathfinding/a-star/implementation.html
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
                if (current == end)
                    return TracePath(start, end, parents);

                closedset.Add(current);


                foreach (var next in GetNeighbors(current))
                {
                    if (closedset.Contains(next))
                        continue;
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

    public class PriorityQueue<T>
    {
        private readonly List<Tuple<T, double>> _elements = new List<Tuple<T, double>>();

        public int Count => _elements.Count;

        public void Enqueue(T item, double priority) => _elements.Add(Tuple.Create(item, priority));

        public T Dequeue()
        {
            int bestIndex = 0;

            for (int i = 0; i < _elements.Count; i++)
                if (_elements[i].Item2 < _elements[bestIndex].Item2)
                    bestIndex = i;

            T bestItem = _elements[bestIndex].Item1;
            _elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}
