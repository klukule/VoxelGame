using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VoxelGame.Assets;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Culling type
    /// </summary>
    public enum CullType
    {
        None,
        Front,
        Back
    }

    /// <summary>
    /// Blending type
    /// </summary>
    public enum BlendType
    {
        None,
        OneMinus
    }

    /// <summary>
    /// Shader
    /// </summary>
    public class Shader : IDisposable
    {
        private Dictionary<string, Uniform> _uniforms = new Dictionary<string, Uniform>();
        private bool _isDisposed = false;

        /// <summary>
        /// Shader handle
        /// </summary>
        public int Handle { get; private set; }

        /// <summary>
        /// Whether this shader is transparent
        /// </summary>
        public bool IsTransparent { get; private set; }

        /// <summary>
        /// Shader blending function
        /// </summary>
        public BlendType Blending { get; private set; }

        /// <summary>
        /// Shader culling
        /// </summary>
        public CullType CullingType { get; private set; } = CullType.Back;

        /// <summary>
        /// Returns uniforms bound to this shader
        /// </summary>
        public IReadOnlyDictionary<string, Uniform> Uniforms => _uniforms;

        /// <summary>
        /// Create new shader from file
        /// </summary>
        /// <param name="fileLocation">File</param>
        public Shader(string fileLocation)
        {
            short shaderType = -1;
            string[] src = new string[2];
            //-1 = none, 0 = vertex, 1 = fragment
            List<string> lines;

            // Load raw shader text from asset database
            if (AssetDatabase.Package.ContainsEntry(fileLocation))
            {
                var entry = AssetDatabase.Package[fileLocation];
                MemoryStream outputStream = new MemoryStream();
                entry.Extract(outputStream);
                string text = Encoding.ASCII.GetString(outputStream.ToArray());
                lines = text.Split('\n').ToList();
                for (int i = 0; i < lines.Count; i++)
                    lines[i] = lines[i].Trim('\r');
            }
            else // Or from file if not available
                lines = File.ReadAllLines(fileLocation).ToList();

            // Split shader to vertex and fragment source
            foreach (var line in lines)
            {
                if (line.Contains("#shader"))
                {
                    if (line.Contains("vertex"))
                        shaderType = 0;
                    else if (line.Contains("fragment"))
                        shaderType = 1;
                }
                else
                    src[shaderType] += line + "\n";
            }

            CompileShader(src[0], src[1]);
        }

        /// <summary>
        /// Create shader from vertex and fragment source
        /// </summary>
        /// <param name="vertexSrc"></param>
        /// <param name="fragmentSrc"></param>
        public Shader(string vertexSrc, string fragmentSrc) => CompileShader(vertexSrc, fragmentSrc);

        /// <summary>
        /// Get uniform location
        /// </summary>
        /// <param name="uniform">Uniform</param>
        /// <returns>Uniform location; -1 if uniform not found</returns>
        public int GetUniformLocation(string uniform)
        {
            int loc = GL.GetUniformLocation(Handle, uniform);
            if (loc == -1)
                Debug.Log($"Uniform {uniform} doesn't exist in shader {Handle}!", DebugLevel.Warning);

            return loc;
        }

        /// <summary>
        /// Set uniform to the value
        /// </summary>
        /// <param name="name">Uniform name</param>
        /// <param name="value">The value</param>
        public void SetUniform(string name, object value)
        {
            if (_uniforms.TryGetValue(name, out Uniform uniform))
                uniform.SetValue(value);
        }

        /// <summary>
        /// Bind uniform
        /// </summary>
        /// <param name="name">Name</param>
        public void BindUniform(string name)
        {
            if (_uniforms.TryGetValue(name, out Uniform uniform))
                uniform.Bind();
        }

        /// <summary>
        /// Get uniform wrapper
        /// </summary>
        /// <param name="name">Uniform name</param>
        /// <returns>Instance of the uniform wrapper; or null if not found</returns>
        public Uniform GetUniform(string name)
        {
            if (ContainsUniform(name))
                return _uniforms[name];
            return null;
        }

        /// <summary>
        /// Checks if uniform exists or not
        /// </summary>
        /// <param name="name">Uniform name</param>
        /// <returns>True if unifom exists; otherwise false</returns>
        public bool ContainsUniform(string name) => _uniforms.ContainsKey(name);

        /// <summary>
        /// Bind the shader
        /// </summary>
        public void Bind()
        {
            // Setup blending
            if (IsTransparent)
            {
                if (Blending != BlendType.None)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendEquation(BlendEquationMode.FuncAdd);
                    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                }
            }
            else
            {
                GL.Disable(EnableCap.Blend);
            }

            // Setup culling
            switch (CullingType)
            {
                case CullType.None:
                    GL.Disable(EnableCap.CullFace);
                    break;
                case CullType.Front:
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Front);
                    break;
                case CullType.Back:
                    GL.Enable(EnableCap.CullFace);
                    GL.CullFace(CullFaceMode.Back);
                    break;
            }

            // Bind shader
            GL.UseProgram(Handle);

            // Bind uniforms
            foreach (var uniform in _uniforms.Values)
                uniform.Bind();
        }

        /// <summary>
        /// Unbind shader
        /// </summary>
        public void Unbind() => GL.UseProgram(0);


        public void Dispose() => Dispose(true);

        /// <summary>
        /// Dispose the shader
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                GL.DeleteProgram(Handle);
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Compiles the shader from source
        /// </summary>
        /// <param name="vertexSrc">Vertex shader source</param>
        /// <param name="fragmentSrc">Fragment shader source</param>
        void CompileShader(string vertexSrc, string fragmentSrc)
        {
            // Parse uniform line
            void GetUniform(string line)
            {
                var elements = line.Split(' ');
                string type = elements[1];
                string name = elements[2].Trim(';');

                Debug.Log($"Found uniform: {name} of type {type}");

                _uniforms.Add(name, new Uniform(name, GetUniformLocation(name), this, null));
            }

            // Find all uncommented uniforms that are not UBOs
            void CheckForUniforms(string src)
            {
                var lines = src.Split('\n');
                foreach (var line in lines)
                    if (line.Contains("uniform") && !line.StartsWith("//") && !line.Contains("layout(std140)"))
                        GetUniform(line);
            }

            // Find and parse all includes
            void CheckForIncludes()
            {
                Check(ref vertexSrc);
                Check(ref fragmentSrc);

                void Check(ref string source)
                {
                    var lines = source.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("#include "))
                        {
                            string inc = line.Split(' ')[1].Trim('"');
                            string location = "Shaders/" + inc;

                            var entry = AssetDatabase.Package[location];
                            MemoryStream outputStream = new MemoryStream();
                            entry.Extract(outputStream);
                            string text = Encoding.ASCII.GetString(outputStream.ToArray());

                            text = text.Replace("?", "").Replace("\r", "").Replace("\t", "");

                            string file = text;
                            source = source.Replace(line, file);
                        }
                    }
                }
            }

            // Check for transparent/opaque
            void CheckForQueue()
            {
                var lines = fragmentSrc.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("#queue"))
                    {
                        var types = line.Split(' ');
                        var type = types[1];
                        if (type == "transparent")
                            IsTransparent = true;
                        else
                            IsTransparent = false;

                        fragmentSrc = fragmentSrc.Replace(line, "");
                    }
                }

            }

            // Check for blending mode
            void CheckForBlending()
            {
                var lines = fragmentSrc.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("#blend"))
                    {
                        var types = line.Split(' ');
                        var type = types[1];
                        if (type == "none")
                            Blending = BlendType.None;
                        else if (type == "oneminus")
                            Blending = BlendType.OneMinus;

                        fragmentSrc = fragmentSrc.Replace(line, "");
                    }
                }

            }

            // Check for culling
            void CheckForCulling()
            {
                var lines = fragmentSrc.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("#culling"))
                    {
                        var types = line.Split(' ');
                        var type = types[1];
                        if (type == "front")
                            CullingType = CullType.Front;
                        else if (type == "none")
                            CullingType = CullType.None;
                        else if (type == "back")
                            CullingType = CullType.Back;

                        fragmentSrc = fragmentSrc.Replace(line, "");
                    }
                }

            }

            // Load includes
            CheckForIncludes();

            // Extract additional info
            CheckForQueue();
            CheckForBlending();
            CheckForCulling();

            int vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, vertexSrc);

            int fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, fragmentSrc);

            // Compile vertex shader
            GL.CompileShader(vertShader);

            string infoLogVert = GL.GetShaderInfoLog(vertShader);
            if (infoLogVert != string.Empty)
                throw new Exception("Vertex Shader " + vertShader + " failed to compile!\n" + infoLogVert);
            else
                Debug.Log($"VertexContainer shader {vertShader} compiled successfully!");

            // Compile fragment shader
            GL.CompileShader(fragShader);

            string infoLogFrag = GL.GetShaderInfoLog(fragShader);

            if (infoLogFrag != string.Empty)
                throw new Exception("FragShader " + fragShader + " failed to compile!\n" + infoLogFrag);
            else
                Debug.Log($"Fragment shader {fragShader} compiled successfully!");

            // Create shader program
            Handle = GL.CreateProgram();

            // Attach and link
            GL.AttachShader(Handle, vertShader);
            GL.AttachShader(Handle, fragShader);

            GL.LinkProgram(Handle);

            GL.DetachShader(Handle, vertShader);
            GL.DetachShader(Handle, fragShader);

            // Delete shader source after linking
            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);

            // Get uniforms
            CheckForUniforms(vertexSrc);
            CheckForUniforms(fragmentSrc);
        }
    }
}
