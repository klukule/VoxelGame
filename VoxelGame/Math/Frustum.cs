using OpenTK;
using VoxelGame.Physics;

namespace VoxelGame
{
    /// <summary>
    /// Camera frustum
    /// </summary>
    /// <remarks>
    /// Implemented based on: https://www.gamedevs.org/uploads/fast-extraction-viewing-frustum-planes-from-world-view-projection-matrix.pdf DirectX implementation
    /// </remarks>
    public class Frustum
    {
        Plane[] planes = new Plane[6];

        /// <summary>
        /// Update frustum with the view*projection matrix
        /// </summary>
        /// <param name="clipMatrix">The clip matrix</param>
        public void UpdateMatrix(Matrix4 clipMatrix)
        {
            // Near
            planes[0].Normal = new Vector3(clipMatrix.M13, clipMatrix.M23, clipMatrix.M33);
            planes[0].Distance = clipMatrix.M43;
            planes[0].Normalize();

            // Far
            planes[1].Normal = new Vector3(clipMatrix.M14 - clipMatrix.M13, clipMatrix.M24 - clipMatrix.M23, clipMatrix.M34 - clipMatrix.M33);
            planes[1].Distance = clipMatrix.M44 - clipMatrix.M43;
            planes[1].Normalize();

            // Left
            planes[2].Normal = new Vector3(clipMatrix.M14 + clipMatrix.M11, clipMatrix.M24 + clipMatrix.M21, clipMatrix.M34 + clipMatrix.M31);
            planes[2].Distance = clipMatrix.M44 + clipMatrix.M41;
            planes[2].Normalize();

            // Right
            planes[3].Normal = new Vector3(clipMatrix.M14 - clipMatrix.M11, clipMatrix.M24 - clipMatrix.M21, clipMatrix.M34 - clipMatrix.M31);
            planes[3].Distance = clipMatrix.M44 - clipMatrix.M41;
            planes[3].Normalize();

            // Up
            planes[4].Normal = new Vector3(clipMatrix.M14 - clipMatrix.M12, clipMatrix.M24 - clipMatrix.M22, clipMatrix.M34 - clipMatrix.M32);
            planes[4].Distance = clipMatrix.M44 - clipMatrix.M42;
            planes[4].Normalize();

            // Down
            planes[5].Normal = new Vector3(clipMatrix.M14 + clipMatrix.M12, clipMatrix.M24 + clipMatrix.M22, clipMatrix.M34 + clipMatrix.M32);
            planes[5].Distance = clipMatrix.M44 + clipMatrix.M42;
            planes[5].Normalize();
        }

        /// <summary>
        /// Create frustum based on specified matrix
        /// </summary>
        /// <param name="m"></param>
        public Frustum(Matrix4 m)
        {
            // Initialize planes
            for (int i = 0; i < 6; i++)
                planes[i] = new Plane(Vector3.Zero, 0);

            // Update
            UpdateMatrix(m);
        }

        /// <summary>
        /// Frustum x point intersection
        /// </summary>
        /// <param name="p">The point</param>
        /// <returns>True if intersects; otherwise false</returns>
        public bool Intersects(Vector3 p)
        {
            // perform half-space test on all planes, if one is out... it's outside
            bool r = true;

            for (int i = 0; i < 6; i++)
                if (planes[i].DistanceToPoint(p) < 0)
                    r = false;

            return r;
        }

        /// <summary>
        /// Frustum x AABB intersection
        /// </summary>
        /// <param name="box">The box</param>
        /// <returns>Truf if intersects; otherwise false</returns>
        public bool Intersects(BoundingBox box)
        {
            // perform half-space test on all planes and all 8 points, if one is out... it's outside
            for (int i = 0; i < 6; i++)
            {
                if (planes[i].DistanceToPoint(box.Min) >= 0) continue;
                if (planes[i].DistanceToPoint(new Vector3(box.Max.X, box.Min.Y, box.Min.Z)) >= 0) continue;
                if (planes[i].DistanceToPoint(new Vector3(box.Min.X, box.Max.Y, box.Min.Z)) >= 0) continue;
                if (planes[i].DistanceToPoint(new Vector3(box.Min.X, box.Min.Y, box.Max.Z)) >= 0) continue;
                if (planes[i].DistanceToPoint(new Vector3(box.Max.X, box.Max.Y, box.Min.Z)) >= 0) continue;
                if (planes[i].DistanceToPoint(new Vector3(box.Max.X, box.Min.Y, box.Max.Z)) >= 0) continue;
                if (planes[i].DistanceToPoint(new Vector3(box.Max.X, box.Min.Y, box.Max.Z)) >= 0) continue;
                if (planes[i].DistanceToPoint(new Vector3(box.Min.X, box.Max.Y, box.Max.Z)) >= 0) continue;
                if (planes[i].DistanceToPoint(box.Max) >= 0) continue;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Frustum plane
        /// </summary>
        private class Plane
        {
            /// <summary>
            /// The normal that represents the plane.
            /// </summary>
            public Vector3 Normal { get; set; }

            /// <summary>
            /// The distance of the plane along its normal from the origin.
            /// </summary>
            public float Distance { get; set; }

            /// <summary>
            /// Construct the plane from normal and distance
            /// </summary>
            /// <param name="nrml">Normal</param>
            /// <param name="dist">Distance</param>
            public Plane(Vector3 nrml, float dist)
            {
                Normal = nrml;
                Distance = dist;
            }

            /// <summary>
            /// Normalize the values
            /// </summary>
            public void Normalize()
            {
                float scale = 1 / Normal.Length;
                Normal *= scale;
                Distance *= scale;
            }

            /// <summary>
            /// Distance from point and plane.
            /// </summary>
            /// <param name="p"></param>
            /// <returns>Signed distance from plane based on normal</returns>
            /// <remarks>
            /// value < 0, then point p lies in the negative halfspace.
            /// value = 0, then point p lies on the plane.
            /// value > 0, then point p lies in the positive halfspace.
            /// </remarks>
            public float DistanceToPoint(Vector3 p) => Normal.X * p.X + Normal.Y * p.Y + Normal.Z * p.Z + Distance;
        }
    }
}
