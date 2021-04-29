using OpenTK;
using System;
using VoxelGame.Worlds;

namespace VoxelGame
{
    /// <summary>
    /// Additional math functions
    /// </summary>
    public static class Mathf
    {
        /// <summary>
        /// Linear interpolation between two points
        /// </summary>
        public static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        /// <summary>
        /// Gets forward vector from set pitch, yaw and roll
        /// </summary>
        /// <param name="Rotation">pitch, yaw and roll</param>
        /// <returns>Forward directional vector</returns>
        public static Vector3 GetForwardFromRotation(Vector3 Rotation)
        {

            float yaw = MathHelper.DegreesToRadians(Rotation.Y + 90);
            float pitch = MathHelper.DegreesToRadians(Rotation.X);

            float x = (float)(Math.Cos(yaw) * Math.Cos(pitch));
            float y = (float)Math.Sin(pitch);
            float z = (float)(Math.Cos(pitch) * Math.Sin(yaw));

            return new Vector3(-x, -y, -z).Normalized();
        }

        /// <summary>
        /// Gets right vector from set pitch, yaw and roll
        /// </summary>
        /// <param name="Rotation">pitch, yaw and roll</param>
        /// <returns>Right directional vector</returns>
        public static Vector3 GetRightFromRotation(Vector3 Rotation)
        {
            float yaw = MathHelper.DegreesToRadians(Rotation.Y);

            float x = (float)Math.Cos(yaw);
            float z = (float)Math.Sin(yaw);

            return new Vector3(x, 0, z);
        }

        /// <summary>
        /// Gets up vector from set pitch, yaw and roll
        /// </summary>
        /// <param name="Rotation">pitch, yaw and roll</param>
        /// <returns>Up directional vector</returns>
        public static Vector3 GetUpFromRotation(Vector3 Rotation)
        {
            float pitch = MathHelper.DegreesToRadians(Rotation.X + 90);

            float y = (float)Math.Sin(pitch);

            return new Vector3(0, y, 0);
        }

        /// <summary>
        /// Get boolean based on input chance
        /// </summary>
        /// <param name="chance">The chance</param>
        /// <returns>True/False based on the chance</returns>
        public static bool Chance(float chance)
        {
            if (chance >= 1) return true;
            if (chance <= 0) return false;
            return World.Instance.Randomizer.NextDouble() <= chance;
        }
    }
}
