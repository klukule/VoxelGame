﻿using OpenTK;

namespace VoxelGame.Rendering.Vertex
{
    /// <summary>
    /// Vertex layout for block rendering
    /// </summary>
    public class VertexBlockContainer : VertexContainer
    {
        public override int[] ElementCount => new[] { 3, 2, 3, 2, 4, 1 };

        // TODO: Allow for partial updates???

        public VertexBlockContainer(Vector3[] positions, Vector2[] uvs, Vector3[] normals, Vector2[] uv2, Vector4[] col, float[] lighting)
        {
            if (positions.Length != uvs.Length)
                Debug.Assert("Vertex position array is not of the same length as vertex UV array!");


            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                _elements.Add(position.X);
                _elements.Add(position.Y);
                _elements.Add(position.Z);

                RecalculateBounds(position);

                _elements.Add(uvs[i].X);
                _elements.Add(uvs[i].Y);

                _elements.Add(normals[i].X);
                _elements.Add(normals[i].Y);
                _elements.Add(normals[i].Z);

                _elements.Add(uv2[i].X);
                _elements.Add(uv2[i].Y);

                _elements.Add(col[i].X);
                _elements.Add(col[i].Y);
                _elements.Add(col[i].Z);
                _elements.Add(col[i].W);

                _elements.Add(lighting[i]);
            }
        }
    }
}