using OpenTK;
using System;
using VoxelGame.Blocks;
using VoxelGame.Worlds;

namespace VoxelGame.Physics
{
    /// <summary>
    /// Simple AABB
    /// </summary>
    public class BoundingBox
    {
        /// <summary>
        /// Minimum
        /// </summary>
        public Vector3 Min { get; set; }

        /// <summary>
        /// Maximum
        /// </summary>
        public Vector3 Max { get; set; }

        /// <summary>
        /// Size
        /// </summary>
        public Vector3 Size => Max - Min;

        public BoundingBox(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            Min = new Vector3(minX, minY, minZ);
            Max = new Vector3(maxX, maxY, maxZ);
        }

        /// <summary>
        /// Transforms the AABB by given translation and scale vector
        /// </summary>
        /// <returns>Transformed copy</returns>
        public BoundingBox Transform(Vector3 translation, Vector3 scale) => new BoundingBox(Min.X + translation.X * scale.X,
                                                                                            Max.X + translation.X * scale.X,
                                                                                            Min.Y + translation.Y * scale.Y,
                                                                                            Max.Y + translation.Y * scale.Y,
                                                                                            Min.Z + translation.Z * scale.Z,
                                                                                            Max.Z + translation.Z * scale.Z);

        /// <summary>
        /// Checks whether the bounding box intersects with this bounding box
        /// </summary>
        /// <returns>True if the box intersects otherwise false</returns>
        public bool Intersects(BoundingBox box, Rigidbody body) => IntersectsWithOffset(box, body, Vector3.Zero);

        /// <summary>
        /// Checks whether the bounding box intersects with this bounding box with added offset
        /// </summary>
        /// <param name="offset">Additional offset</param>
        /// <returns>True if the box intersects otherwise false</returns>
        public bool IntersectsWithOffset(BoundingBox box, Rigidbody body, Vector3 offset) => offset.X + Min.X <= body.Owner.Position.X + box.Max.X &&
                                                                                             offset.X + Max.X >= body.Owner.Position.X + box.Min.X &&
                                                                                             offset.Y + Min.Y <= body.Owner.Position.Y + box.Max.Y &&
                                                                                             offset.Y + Max.Y >= body.Owner.Position.Y + box.Min.Y &&
                                                                                             offset.Z + Min.Z <= body.Owner.Position.Z + box.Max.Z &&
                                                                                             offset.Z + Max.Z >= body.Owner.Position.Z + box.Min.Z;

        /// <summary>
        /// Checks whether this box with added offset intersects given position
        /// </summary>
        /// <returns>True if the box intersects otherwise false</returns>
        public bool IntersectsWithOffset(Vector3 offset, Vector3 position) => offset.X + Min.X <= position.X &&
                                                                              offset.X + Max.X >= position.X &&
                                                                              offset.Y + Min.Y <= position.Y &&
                                                                              offset.Y + Max.Y >= position.Y &&
                                                                              offset.Z + Min.Z <= position.Z &&
                                                                              offset.Z + Max.Z >= position.Z;

        /// <summary>
        /// Checks whether this box intersects voxel world
        /// </summary>
        /// <returns>True is the box intersects otherwise false</returns>
        public bool IntersectsWorld(Rigidbody body) => IntersectsWorldDirectional(body, body.Velocity);

        /// <summary>
        /// Checks whether this box intersects voxle world along specific direction
        /// </summary>
        /// <param name="body">Parent rigidbody</param>
        /// <param name="direction">Direction vector used as offset for checks</param>
        /// <returns>True if the box intersects otherwise false</returns>
        public bool IntersectsWorldDirectional(Rigidbody body, Vector3 direction)
        {
            // Offsets for bottom plane
            bool BottomBackLeft()
            {
                var pos = body.Owner.Position + new Vector3(Min.X, Min.Y, Min.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return CheckWorld(chunkPosition, posInChunk);
            }
            bool BottomBackRight()
            {
                var pos = body.Owner.Position + new Vector3(Max.X, Min.Y, Min.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return CheckWorld(chunkPosition, posInChunk);
            }
            bool BottomFrontLeft()
            {
                var pos = body.Owner.Position + new Vector3(Min.X, Min.Y, Max.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return CheckWorld(chunkPosition, posInChunk);
            }
            bool BottomFrontRight()
            {
                var pos = body.Owner.Position + new Vector3(Max.X, Min.Y, Max.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return CheckWorld(chunkPosition, posInChunk);
            }

            // Offsets for top plane
            bool TopBackLeft()
            {
                var pos = body.Owner.Position + new Vector3(Min.X, Max.Y, Min.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return CheckWorld(chunkPosition, posInChunk);
            }
            bool TopBackRight()
            {
                var pos = body.Owner.Position + new Vector3(Max.X, Max.Y, Min.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return CheckWorld(chunkPosition, posInChunk);
            }
            bool TopFrontLeft()
            {
                var pos = body.Owner.Position + new Vector3(Min.X, Max.Y, Max.Z) + direction;
                var chunkPosition = (pos).ToChunkPosition();
                var posInChunk = (pos).ToChunkSpaceFloored();
                return CheckWorld(chunkPosition, posInChunk);
            }
            bool TopFrontRight()
            {
                var pos = body.Owner.Position + new Vector3(Max.X, Max.Y, Max.Z) + direction;
                var chunkPosition = pos.ToChunkPosition();
                var posInChunk = pos.ToChunkSpaceFloored();
                return CheckWorld(chunkPosition, posInChunk);
            }

            // Checks if box intersect given block position
            bool CheckWorld(Vector3 chunkPosition, Vector3 posInChunk)
            {
                // Get chunk
                if (World.Instance.TryGetChunkAtPosition((int)chunkPosition.X, (int)chunkPosition.Z, out Chunk chunk))
                {
                    // Get block in chunk
                    short id = chunk.GetBlockID((int)(posInChunk.X), (int)(posInChunk.Y), (int)(posInChunk.Z));
                    if (id == 0) return false; // Doesn't intersect if air

                    Block block = BlockDatabase.GetBlock(id);
                    if (block == null) return false; // Block not found, doesn't intersect

                    if (block.CollisionShape != null) // If block has collision
                    {
                        // Check if the block intersects
                        var blockPos = (chunkPosition * Chunk.WIDTH) + posInChunk;
                        return block.CollisionShape.IntersectsWithOffset(this, body, blockPos - direction);
                    }
                    return false;
                }
                return false;
            }

            // Horizontal movement or falling -> need to check bottom of the box for collision
            if (!(direction.X == 0 && direction.Z == 0 && direction.Y > 0))
            {
                if (BottomBackLeft())
                    return true;
                if (BottomBackRight())
                    return true;
                if (BottomFrontLeft())
                    return true;
                if (BottomFrontRight())
                    return true;
            }

            // Positive vertical movement -> need to check top of the box for collision
            if (direction.Y >= 0)
            {
                direction.Y = -direction.Y;

                if (TopBackLeft())
                    return true;
                if (TopBackRight())
                    return true;
                if (TopFrontLeft())
                    return true;
                if (TopFrontRight())
                    return true;
            }


            return false;
        }


        /// <summary>
        /// Checks wheteher the ray intersects bounding box
        /// </summary>
        /// <param name="position">Ray origin</param>
        /// <param name="direction">Ray direction (unit vector)</param>
        /// <returns>True if intersects; otherwise false</returns>
        public bool IntersectsRay(Rigidbody body, Vector3 position, Vector3 direction, out float distance)
        {
            var dirfrac = new Vector3(1f / direction.X, 1f / direction.Y, 1f / direction.Z);

            var t1 = (body.Owner.Position + Min - position) * dirfrac;
            var t2 = (body.Owner.Position + Max - position) * dirfrac;

            float tmin = MathF.Max(MathF.Max(MathF.Min(t1.X, t2.X), MathF.Min(t1.Y, t2.Y)), MathF.Min(t1.Z, t2.Z));
            float tmax = MathF.Min(MathF.Min(MathF.Max(t1.X, t2.X), MathF.Max(t1.Y, t2.Y)), MathF.Max(t1.Z, t2.Z));

            // Intersects, but behind us
            if(tmax < 0)
            {
                distance = tmax;
                return false;
            }

            // if tmin > tmax, doesn't intersect
            if(tmin > tmax)
            {
                distance = tmax;
                return false;
            }

            distance = tmin;
            return true;
        }
    }
}
