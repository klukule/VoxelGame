using OpenTK;
using VoxelGame.Blocks;
using VoxelGame.Worlds;

namespace VoxelGame.Physics
{
    /// <summary>
    /// Result of ray x voxel raycast
    /// </summary>
    public struct RayVoxelOut
    {
        /// <summary>
        /// Hit block id
        /// </summary>
        public short BlockID;

        /// <summary>
        /// Hit position
        /// </summary>
        public Vector3 BlockPosition;

        /// <summary>
        /// Hit chunk
        /// </summary>
        public Vector2 ChunkPosition;

        /// <summary>
        /// Position for placement
        /// </summary>
        public Vector3 PlacementPosition;

        /// <summary>
        /// Chunk for placement
        /// </summary>
        public Vector2 PlacementChunk;

        /// <summary>
        /// Hit face normal
        /// </summary>
        public Vector3 HitNormal;
    }

    /// <summary>
    /// Raycasting helpers against voxel world
    /// </summary>
    public static class Raycast
    {
        private const float stepSize = 0.125f;

        /// <summary>
        /// Raycast in voxel world
        /// </summary>
        /// <param name="position">Ray origin</param>
        /// <param name="direction">Ray direction</param>
        /// <param name="distance">Ray distance</param>
        /// <param name="output">Raycast result</param>
        /// <returns>True if hit; otherwise false</returns>
        public static bool CastVoxel(Vector3 position, Vector3 direction, float distance, out RayVoxelOut output)
        {
            output = new RayVoxelOut();
            Vector3 curPos = position;
            Vector3 lastPos = new Vector3();
            float distTravelled = 0;

            World world = World.Instance;

            while (distTravelled < distance)
            {
                var chunkPos = curPos.ToChunkPosition();
                var pos = curPos.ToChunkSpaceFloored();

                if (world.TryGetChunkAtPosition((int)chunkPos.X, (int)chunkPos.Z, out Chunk chunk))
                {
                    var possibleBlock = BlockDatabase.GetBlock(chunk.GetBlockID((int)(pos.X), (int)(pos.Y), (int)(pos.Z)));

                    if (possibleBlock?.CollisionShape != null)
                    {
                        var blockPos = (chunkPos * Chunk.WIDTH) + pos;
                        if (possibleBlock.CollisionShape.IntersectsWithOffset(blockPos, curPos))
                        {
                            output.BlockID = (short)possibleBlock.ID;
                            output.BlockPosition = new Vector3((int)(pos.X), (int)(pos.Y), (int)(pos.Z));
                            output.ChunkPosition = new Vector2((int)chunkPos.X, (int)chunkPos.Z);

                            var placeChunk = lastPos.ToChunkPosition();
                            var placePos = lastPos.ToChunkSpaceFloored();

                            output.PlacementPosition = new Vector3((int)(placePos.X), (int)(placePos.Y), (int)(placePos.Z));
                            output.PlacementChunk = new Vector2((int)placeChunk.X, (int)placeChunk.Z);

                            return true;
                        }
                    }
                }

                lastPos = curPos;
                curPos += direction * stepSize;
                distTravelled += stepSize;
            }

            output = default;
            return false;
        }
    }
}
