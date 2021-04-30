using System;
using OpenTK.Graphics.OpenGL4;
using VoxelGame.Rendering.Vertex;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Vertex buffer object
    /// </summary>
    public class VBO : IDisposable
    {
        /// <summary>
        /// Buffer handle
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// Vertex data
        /// </summary>
        public VertexContainer VertexContainer { get; private set; }

        /// <summary>
        /// Create new vertex buffer based on vertex data
        /// </summary>
        public VBO(VertexContainer vertices)
        {
            Handle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

            VertexContainer = vertices;

            GL.BufferData(BufferTarget.ArrayBuffer, VertexContainer.Length * sizeof(byte), VertexContainer.Elements, BufferUsageHint.StaticDraw);
        }

        /// <summary>
        /// Bind the buffer
        /// </summary>
        public void Bind() => GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);

        /// <summary>
        /// Unbind the buffer
        /// </summary>
        public void Unbind() => GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(Handle);
        }
    }
}
