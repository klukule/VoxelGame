using OpenTK;
using System.Collections.Generic;
using VoxelGame.Physics;

namespace VoxelGame.Rendering.Vertex
{
    /// <summary>
    /// Helper class containing vertex layout and data which are sent to GPU
    /// </summary>
    public class VertexContainer
    {
        protected List<float> _elements = new List<float>();                    // Raw list of chained elements as one array
        protected BoundingBox _boundingBox = new BoundingBox(0, 0, 0, 0, 0, 0); // Tight bounding box around the geometry

        /// <summary>
        /// Per-attribute element count
        /// </summary>
        /// <remarks>
        /// float = 1,
        /// vec2  = 2,
        /// vec3  = 3,
        /// vec4  = 4
        /// </remarks>
        public virtual int[] ElementCount { get; } = { 3, 2 };

        /// <summary>
        /// Gets stored data as array
        /// </summary>
        public float[] Elements => _elements.ToArray();

        /// <summary>
        /// Gets total element count (sum of Element count array entries)
        /// </summary>
        public int TotalElementCount
        {
            get
            {
                int val = 0;
                for (int i = 0; i < ElementCount.Length; i++)
                    val += ElementCount[i];
                return val;
            }
        }

        /// <summary>
        /// Gets the length of the data stored
        /// </summary>
        public int Length => _elements.Count;

        /// <summary>
        /// Gets the bounding box around the geometry
        /// </summary>
        public BoundingBox BoundingBox => _boundingBox;

        /// <summary>
        /// Updates bounding box to contain the position
        /// </summary>
        /// <param name="position">THe position</param>
        protected void RecalculateBounds(Vector3 position)
        {
            // Update max
            if (position.X > _boundingBox.Max.X)
                _boundingBox.Max = new Vector3(position.X, _boundingBox.Max.Y, _boundingBox.Max.Z);
            if (position.Y > _boundingBox.Max.Y)
                _boundingBox.Max = new Vector3(_boundingBox.Max.X, position.Y, _boundingBox.Max.Z);
            if (position.Z > _boundingBox.Max.Z)
                _boundingBox.Max = new Vector3(_boundingBox.Max.X, _boundingBox.Max.Y, position.Z);

            // Update min
            if (position.X < _boundingBox.Min.X)
                _boundingBox.Min = new Vector3(position.X, _boundingBox.Min.Y, _boundingBox.Min.Z);
            if (position.Y < _boundingBox.Min.Y)
                _boundingBox.Min = new Vector3(_boundingBox.Min.X, position.Y, _boundingBox.Min.Z);
            if (position.Z < _boundingBox.Min.Z)
                _boundingBox.Min = new Vector3(_boundingBox.Min.X, _boundingBox.Min.Y, position.Z);

        }

        /// <summary>
        /// Creates new instance with data
        /// </summary>
        /// <param name="positions">Vertex positions</param>
        /// <param name="uvs">Vertex UVs</param>
        public VertexContainer(Vector3[] positions, Vector2[] uvs)
        {
            if (positions.Length != uvs.Length)
                Debug.Assert("Vertex position array is not of the same length as vertex UV array!");

            // Chain positions and UVs together
            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];

                _elements.Add(position.X);
                _elements.Add(position.Y);
                _elements.Add(position.Z);

                RecalculateBounds(position);    // Update bounding box

                _elements.Add(uvs[i].X);
                _elements.Add(uvs[i].Y);
            }
        }

        /// <summary>
        /// Internal constructor, because overriden classes implement differnt chaining logic based on their needs
        /// </summary>
        protected VertexContainer() { }
    }
}
