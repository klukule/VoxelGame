using OpenTK;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Blocks;
using VoxelGame.Worlds;

namespace VoxelGame.Worlds.Decoration
{
    /// <summary>
    /// Spawns oak trees
    /// </summary>
    public class GlowstoneDecorator : Decorator
    {
        private int _maxTrees = 8;
        private int _treeDists = 3;
        private float _treeChance = .0125f;
        private List<Vector2> _treePoses = new List<Vector2>();

        public override void Dispose()
        {

        }

        /// <summary>
        /// Places leaf block on given coordinate if air
        /// </summary>
        void PlaceLeaf(int x, int y, int z, Chunk chunk)
        {
            int blockid = chunk.GetBlockID(x, y, z);
            if (blockid == 0)
                chunk.PlaceBlock(x, y, z, GameBlocks.LEAVES_OAK, false); // Do not cause chunk update
        }

        public override void DecorateAtBlock(Chunk chunk, int x, int y, int z)
        {
            // Spawn only on grass
            if (chunk.GetBlockID(x, y, z) != (short)GameBlocks.GRASS.ID)
                return;

            // Check the chance
            bool chance = Mathf.Chance(_treeChance);
            if (!chance) return;

            // Check for maximum spawned trees per chunk
            if (_treePoses.Count >= _maxTrees)
                return;

            var otherX = Chunk.WIDTH - x;
            var otherZ = Chunk.WIDTH - z;

            // Check whether the whole tree is fully inside current chunk - cross-chunk spawning not (yet) supported
            if (x >= _treeDists && z >= _treeDists && otherX >= _treeDists && otherZ >= _treeDists)
            {
                // Check if there are no trees nearby
                if (_treePoses.Count == 0 || _treePoses.Any(v => Vector2.Distance(v, new Vector2(x, z)) > _treeDists))
                {
                    // Replace grass for dirt (do to cause chunk update)
                    chunk.PlaceBlock(x, y, z, GameBlocks.DIRT, false);

                    // Spawn tree trunk
                     chunk.PlaceBlock(x, y + 1, z, (short)GameBlocks.GLOWSTONE.ID, false);

                    // Mark tree spawn position
                    _treePoses.Add(new Vector2(x, z));
                }
            }
        }
    }
}
