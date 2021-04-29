using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Index buffer object
    /// </summary>
    public class IBO : IDisposable
    {
        private uint[] _indices;
        
        /// <summary>
        /// IBO handle
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// Number of indices in buffer
        /// </summary>
        public int Length { get; private set; }

        public IBO(uint[] indices)
        {
            Length = indices.Length;

            Handle = GL.GenBuffer();

            _indices = indices;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
        }

        /// <summary>
        /// Bind the buffer
        /// </summary>
        public void Bind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);

        /// <summary>
        /// Unbind the buffer
        /// </summary>
        public void Unbind() => GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        /// <summary>
        /// Dispose the buffer
        /// </summary>
        public void Dispose()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffer(Handle);
        }
    }
}
