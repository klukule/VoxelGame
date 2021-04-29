using OpenTK;
using OpenTK.Graphics;
using VoxelGame.Physics;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Get block mask color delegate
    /// </summary>
    /// <param name="x">Block X coordinate</param>
    /// <param name="y">Block Y coordinate</param>
    /// <param name="z">Block Z coordinate</param>
    /// <returns>Block mask color</returns>
    public delegate Color4 BlockColorDelegate(int x, int y, int z);

    /// <summary>
    /// Block base class
    /// </summary>
    public class Block
    {
        /// <summary>
        /// Unique block id
        /// </summary>
        public virtual string Key { get; set; } = "";

        /// <summary>
        /// Automatically assigned numeric block id
        /// </summary>
        public int ID { get; set; } = -1;

        /// <summary>
        /// Block mask color
        /// </summary>
        public virtual BlockColorDelegate BlockColor { get; set; } = (x, y, z) => Color4.White;

        /// <summary>
        /// Block opacity
        /// </summary>
        /// <remarks>
        /// Used in light propagation function to appropriately attenuate the light
        /// </remarks>
        public virtual sbyte Opacity { get; set; } = 15;

        /// <summary>
        /// Whether or not the block is transparent
        /// </summary>
        public virtual bool IsTransparent { get; set; } = false;

        /// <summary>
        /// Whether or not the block culls blocks of the same type
        /// </summary>
        public virtual bool TransparencyCullsSelf { get; set; } = false;

        /// <summary>
        /// Block collision shape
        /// </summary>
        public virtual BoundingBox CollisionShape { get; set; } = new BoundingBox(0, 1, 0, 1, 0, 1);

        /// <summary>
        /// Top Face
        /// </summary>
        public Face Top { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));
        
        /// <summary>
        /// Bottom Face
        /// </summary>
        public Face Bottom { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));

        /// <summary>
        /// Left Face
        /// </summary>
        public Face Left { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));

        /// <summary>
        /// Right Face
        /// </summary>
        public Face Right { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));

        /// <summary>
        /// Front Face
        /// </summary>
        public Face Front { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));

        /// <summary>
        /// Back Face
        /// </summary>
        public Face Back { get; set; } = new Face(new Rect(0, 0, 1, 1), new Rect(0, 0, 0, 0));

        /// <summary>
        /// Creates new instance of the block
        /// </summary>
        public Block()
        {
            BlockDatabase.RegisterBlock(this);
        }

        /// <summary>
        /// Called when the block is broken
        /// </summary>
        /// <param name="WorldPosition">Block position</param>
        /// <param name="ChunkPosition">Chunk position</param>
        public virtual void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
        }

        /// <summary>
        /// Called when the block is placed
        /// </summary>
        /// <param name="WorldPosition">Block position</param>
        /// <param name="ChunkPosition">Chunk position</param>
        public virtual void OnPlace(Vector3 WorldPosition, Vector2 ChunkPosition)
        {

        }

        /// <summary>
        /// Helper structure containing UV and Mask UV coordinates for each block face
        /// </summary>
        public sealed class Face
        {
            public Face(Rect uv1, Rect uv2)
            {
                UV1 = uv1;
                UV2 = uv2;

                if (UV2.X == -1 || UV2.Y == -1)
                    UV2 = new Rect(-1, -1, -1, -1);
            }

            /// <summary>
            /// UV1 - standard UVs
            /// </summary>
            public Rect UV1 { get; }

            /// <summary>
            /// UV2 - Additional "mask" UVs
            /// </summary>
            /// <remarks>
            /// (-1, -1, -1, -1) = not used
            /// </remarks>
            public Rect UV2 { get; }
        }
    }
}
