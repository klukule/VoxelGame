using OpenTK;
using OpenTK.Graphics;
using System;
using VoxelGame.Worlds;

namespace VoxelGame
{
    /// <summary>
    /// Misc. math extensions for vectors and strings
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Convert Color4 to Vector4
        /// </summary>
        public static Vector4 ToVector4(this Color4 col) => new Vector4(col.R, col.G, col.B, col.A);

        /// <summary>
        /// Finds the position of the chunk that the vector is in
        /// </summary>
        public static Vector3 ToChunkPosition(this Vector3 vec) => new Vector3((float)Math.Floor(vec.X / Chunk.WIDTH), 0, (float)Math.Floor(vec.Z / Chunk.WIDTH));

        /// <summary>
        /// Finds the position of the vector relative to the chunk it is inside
        /// </summary>
        public static Vector3 ToChunkSpace(this Vector3 vec) => (vec - ToChunkPosition(vec) * Chunk.WIDTH);

        /// <summary>
        /// Finds the position of the vector relative to the chunk it is inside - floors to nearest integer value
        /// </summary>
        public static Vector3 ToChunkSpaceFloored(this Vector3 vec)
        {
            var final = ToChunkSpace(vec);
            return new Vector3(MathF.Floor(final.X), MathF.Floor(final.Y), MathF.Floor(final.Z));
        }

        /// <summary>
        /// Gets random color
        /// </summary>
        public static Color4 GetRandomColor()
        {
            byte r, g, b, a;
            r = (byte)World.Instance.Randomizer.Next(0, 255);
            g = (byte)World.Instance.Randomizer.Next(0, 255);
            b = (byte)World.Instance.Randomizer.Next(0, 255);
            a = (byte)World.Instance.Randomizer.Next(0, 255);

            return new Color4(r, g, b, a);
        }

        /// <summary>
        /// Convert string to seed, using simple sum of ASCII char values
        /// </summary>
        public static int GetSeed(this string str)
        {
            int num = 0;
            for (int i = 0; i < str.Length; i++)
                num += (int)str[i];

            return num;
        }
    }
}
