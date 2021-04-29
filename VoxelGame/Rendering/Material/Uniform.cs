using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Shader uniform wrapper
    /// </summary>
    public class Uniform
    {
        /// <summary>
        /// Uniform name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Uniform location
        /// </summary>
        public int UniformLocation { get; private set; }

        /// <summary>
        /// Uniform value
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Uniform shader
        /// </summary>
        public Shader Shader { get; private set; }

        /// <summary>
        /// Create new uniform
        /// </summary>
        /// <param name="name">Uniform name</param>
        /// <param name="location">Uniform location</param>
        /// <param name="shader">Owning shader</param>
        /// <param name="value">Initial value</param>
        public Uniform(string name, int location, Shader shader, object value)
        {
            Name = name;
            UniformLocation = location;
            Shader = shader;
            SetValue(value);
        }

        /// <summary>
        /// Binds the unifom
        /// </summary>
        public void Bind()
        {
            if (Value == null) return;

            if (Value is double d)
                GL.ProgramUniform1(Shader.Handle, UniformLocation, (float)d);
            else if (Value is float f)
                GL.ProgramUniform1(Shader.Handle, UniformLocation, f);
            else if (Value is int i)
                GL.ProgramUniform1(Shader.Handle, UniformLocation, i);
            else if (Value is Vector2 vec2)
                GL.ProgramUniform2(Shader.Handle, UniformLocation, vec2.X, vec2.Y);
            else if (Value is Vector3 vec3)
                GL.ProgramUniform3(Shader.Handle, UniformLocation, vec3.X, vec3.Y, vec3.Z);
            else if (Value is Vector4 vec4)
                GL.ProgramUniform4(Shader.Handle, UniformLocation, vec4.X, vec4.Y, vec4.Z, vec4.W);
            else if (Value is Matrix4 mat4)
                GL.ProgramUniformMatrix4(Shader.Handle, UniformLocation, false, ref mat4);
        }

        /// <summary>
        /// Set uniform value
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(object value)
        {
            // TODO: Move to setter???
            if (value == null) return;
            if (UniformLocation == -1) UniformLocation = Shader.GetUniformLocation(Name);
            Value = value;
        }
    }
}
