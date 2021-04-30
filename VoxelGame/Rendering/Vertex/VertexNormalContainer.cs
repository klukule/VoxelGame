using OpenTK;
using System;

namespace VoxelGame.Rendering.Vertex
{
    /// <summary>
    /// Vertex layout containing additional normal vector
    /// </summary>
    public class VertexNormalContainer : VertexContainer
    {
        public override int[] ElementCount => new[] { 3, 2, 3 };

        public VertexNormalContainer(Vector3[] positions, Vector2[] uvs, Vector3[] normals)
        {
            if (positions.Length != uvs.Length)
                Debug.Assert("Vertex position array is not of the same length as vertex UV array!");

            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                //_elements.Add(position.X);
                //_elements.Add(position.Y);
                //_elements.Add(position.Z);

                _elements.AddRange(BitConverter.GetBytes(position.X));
                _elements.AddRange(BitConverter.GetBytes(position.Y));
                _elements.AddRange(BitConverter.GetBytes(position.Z));

                RecalculateBounds(position);


                _elements.AddRange(BitConverter.GetBytes(uvs[i].X));
                _elements.AddRange(BitConverter.GetBytes(uvs[i].Y));

                _elements.AddRange(BitConverter.GetBytes(normals[i].X));
                _elements.AddRange(BitConverter.GetBytes(normals[i].Y));
                _elements.AddRange(BitConverter.GetBytes(normals[i].Z));
            }
        }
    }
}
