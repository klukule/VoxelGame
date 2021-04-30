using OpenTK;
using System;

namespace VoxelGame.Rendering.Vertex
{
    /// <summary>
    /// Vertex layout for block rendering
    /// </summary>
    public class VertexBlockContainer : VertexContainer
    {
        public override int[] ElementCount => new[] { 3, 2, 3, 2, 4, 1 };

        // TODO: Allow for partial updates???

        public VertexBlockContainer(Vector3[] positions, Vector2[] uvs, Vector3[] normals, Vector2[] uv2, Vector4[] col, uint[] lighting)
        {
            if (positions.Length != uvs.Length)
                Debug.Assert("Vertex position array is not of the same length as vertex UV array!");


            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                _elements.AddRange(BitConverter.GetBytes(position.X));
                _elements.AddRange(BitConverter.GetBytes(position.Y));
                _elements.AddRange(BitConverter.GetBytes(position.Z));

                RecalculateBounds(position);

                _elements.AddRange(BitConverter.GetBytes(uvs[i].X));
                _elements.AddRange(BitConverter.GetBytes(uvs[i].Y));

                _elements.AddRange(BitConverter.GetBytes(normals[i].X));
                _elements.AddRange(BitConverter.GetBytes(normals[i].Y));
                _elements.AddRange(BitConverter.GetBytes(normals[i].Z));

                _elements.AddRange(BitConverter.GetBytes(uv2[i].X));
                _elements.AddRange(BitConverter.GetBytes(uv2[i].Y));

                _elements.AddRange(BitConverter.GetBytes(col[i].X));
                _elements.AddRange(BitConverter.GetBytes(col[i].Y));
                _elements.AddRange(BitConverter.GetBytes(col[i].Z));
                _elements.AddRange(BitConverter.GetBytes(col[i].W));

                _elements.AddRange(BitConverter.GetBytes(lighting[i]));
            }
        }
    }
}
