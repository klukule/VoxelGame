using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Vertex array object
    /// </summary>
    public class VAO : IDisposable
    {
        private VBO _vertexBuffer;
        
        /// <summary>
        /// Buffer handle
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// Create new vertex array object from vertex buffer
        /// </summary>
        /// <param name="buff"></param>
        public VAO(VBO buff)
        {
            Handle = GL.GenVertexArray();
            _vertexBuffer = buff;

            Bind();
            _vertexBuffer.Bind();

            // Bind the vbo to attributes
            int offset = 0;
            for (int i = 0; i < buff.VertexContainer.ElementCount.Length; i++)
            {
                int count = buff.VertexContainer.ElementCount[i];                               // Number of floats in current attribute
                int byteSize = count * sizeof(float);                                           // Size in bytes
                int totalSize = buff.VertexContainer.TotalElementCount * sizeof(float);    // Total vbo size

                // Bind
                GL.EnableVertexAttribArray(i);
                GL.VertexAttribPointer(i, count, VertexAttribPointerType.Float, false, totalSize, offset);

                offset += byteSize;
            }
        }

        /// <summary>
        /// Bind
        /// </summary>
        public void Bind() => GL.BindVertexArray(Handle);

        /// <summary>
        /// Unbind
        /// </summary>
        public void Unbind() => GL.BindVertexArray(0);

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            // Unbind and dispose
            GL.BindVertexArray(0);
            GL.DeleteVertexArray(Handle);
        }
    }
}
