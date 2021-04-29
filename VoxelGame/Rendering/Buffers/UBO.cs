using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace VoxelGame.Rendering.Buffers
{
    /// <summary>
    /// Uniform buffer objct
    /// </summary>
    /// <typeparam name="T">Type of data in the buffer</typeparam>
    public class UBO<T> where T : struct
    {
        private T _data;        // Data in the buffer
        private int _size;      // Size of the data
        private int _slot;      // Uniform buffer slot index
        private string _name;   // Uniform name

        /// <summary>
        /// Buffer handle
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// Create new uniform buffer
        /// </summary>
        public UBO(T data, string blockName)
        {
            Handle = GL.GenBuffer();

            _name = blockName;
            _size = Marshal.SizeOf<T>();
            _slot = UniformBuffers.TotalUBOs++;

            Update(data);
        }

        /// <summary>
        /// Update the data
        /// </summary>
        public void Update(T Data)
        {
            _data = Data;
            GL.BindBuffer(BufferTarget.UniformBuffer, Handle);
            GL.BufferData(BufferTarget.UniformBuffer, _size, ref _data, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        /// <summary>
        /// Bind the buffer
        /// </summary>
        public void Bind(int program)
        {
            int blockIndex = GL.GetUniformBlockIndex(program, _name);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, _slot, Handle);
            GL.UniformBlockBinding(program, blockIndex, _slot);
        }

        /// <summary>
        /// Unbind the buffer
        /// </summary>
        public void Unbind() => GL.BindBuffer(BufferTarget.UniformBuffer, 0);

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose() => GL.DeleteBuffer(Handle);
    }
}
