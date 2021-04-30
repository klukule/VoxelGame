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
        public override string IconLocation => "Textures/Items/Def_Item.png";
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
                Debug.Log($"AStar Search from: {from} to: {to}", DebugLevel.Debug);

                var result = AStarPathFinder.FindPath(from.Value, to.Value);
                if (result == null)
                    Debug.Log($"AStar Path not found", DebugLevel.Debug);
                else
                {
                    Debug.Log($"AStar Path found", DebugLevel.Debug);
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
                Debug.Log($"AStar Search target not found", DebugLevel.Debug);
            }
        }
    }
}
