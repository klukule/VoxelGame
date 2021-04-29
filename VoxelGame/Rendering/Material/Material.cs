using Ionic.Zip;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VoxelGame.Assets;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Material
    /// </summary>
    public class Material : ILoadable, IDisposable
    {
        // List of bound textures
        private List<Texture> _textures = new List<Texture>();

        /// <summary>
        /// Material name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Corresponding shader
        /// </summary>
        public Shader Shader { get; private set; }

        /// <summary>
        /// List of bound textures (readonly)
        /// </summary>
        public IReadOnlyCollection<Texture> Textures => _textures;

        /// <summary>
        /// Constructor for activator
        /// </summary>
        public Material() { }

        /// <summary>
        /// Load material from file
        /// </summary>
        private Material(string file, string matName) : this(File.ReadAllLines(file), matName, file) { }

        /// <summary>
        /// Load material from raw lines
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="matName"></param>
        private Material(string[] lines, string matName, string file = "Unknown")
        {
            Name = matName;
            string shader = "";
            List<string[]> uniforms = new List<string[]>();

            //Load data
            foreach (var line in lines)
            {
                if (line.StartsWith("//")) continue;
                if (line.StartsWith("texture")) _textures.Add(AssetDatabase.GetAsset<Texture>(line.Split(' ')[1]));
                else if (line.StartsWith("shader")) shader = line.Split(' ')[1];
                else if (line.StartsWith("uniform")) uniforms.Add(line.Split(' '));
            }

            //Instantiate shader
            Shader = new Shader(shader);
            foreach (var uniform in uniforms)
                ProcessUniform(uniform, file);
        }

        /// <summary>
        /// Loads the unifom
        /// </summary>
        private void ProcessUniform(string[] uniform, string file)
        {
            string type = uniform[1];
            string name = uniform[2];
            string value = uniform[3];

            try
            {
                switch (type)
                {
                    case "number":
                        var f = float.Parse(value);
                        Shader.SetUniform(name, f);
                        break;
                    case "mat4":
                        Matrix4 mat4 = new Matrix4();
                        var mat4Val = value.Split(',');
                        if (mat4Val.Length == 16)
                        {
                            mat4.M11 = float.Parse(mat4Val[0]);
                            mat4.M12 = float.Parse(mat4Val[1]);
                            mat4.M13 = float.Parse(mat4Val[2]);
                            mat4.M14 = float.Parse(mat4Val[3]);
                            mat4.M21 = float.Parse(mat4Val[4]);
                            mat4.M22 = float.Parse(mat4Val[5]);
                            mat4.M23 = float.Parse(mat4Val[6]);
                            mat4.M24 = float.Parse(mat4Val[7]);
                            mat4.M31 = float.Parse(mat4Val[8]);
                            mat4.M32 = float.Parse(mat4Val[9]);
                            mat4.M33 = float.Parse(mat4Val[10]);
                            mat4.M34 = float.Parse(mat4Val[11]);
                            mat4.M41 = float.Parse(mat4Val[12]);
                            mat4.M42 = float.Parse(mat4Val[13]);
                            mat4.M43 = float.Parse(mat4Val[14]);
                            mat4.M44 = float.Parse(mat4Val[15]);

                            Shader.SetUniform(name, mat4);
                        }
                        break;
                    case "vec2":
                        var vec2Val = value.Split(',');
                        if (vec2Val.Length == 2)
                            Shader.SetUniform(name, new Vector2(float.Parse(vec2Val[0]), float.Parse(vec2Val[1])));
                        break;
                    case "vec3":
                        var vec3Val = value.Split(',');
                        if (vec3Val.Length == 3)
                            Shader.SetUniform(name, new Vector3(float.Parse(vec3Val[0]), float.Parse(vec3Val[1]), float.Parse(vec3Val[2])));
                        break;
                    case "vec4":
                        var vec4Val = value.Split(',');
                        if (vec4Val.Length == 4)
                            Shader.SetUniform(name, new Vector4(float.Parse(vec4Val[0]), float.Parse(vec4Val[1]), float.Parse(vec4Val[2]), float.Parse(vec4Val[3])));
                        break;
                }
            }
            catch
            {
                Debug.Log($"There was any error parsing material {file}", DebugLevel.Error);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _textures.Count; i++)
                _textures[i].Dispose();
            Shader?.Dispose();
        }

        /// <summary>
        /// Set material uniform
        /// </summary>
        /// <param name="name">Uniform name</param>
        /// <param name="value">Uniform value</param>
        public void SetUniform(string name, object value) => Shader.SetUniform(name, value);

        /// <summary>
        /// Bind texture
        /// </summary>
        /// <param name="index">Texture index</param>
        /// <param name="tex">Texture</param>
        public void SetTexture(int index, Texture tex)
        {
            if (index >= _textures.Count) return;
            _textures[index] = tex;
        }

        /// <summary>
        /// Bind texture handle to uniform slot
        /// </summary>
        /// <param name="uniform">Uniform name</param>
        /// <param name="handle">Texture handle</param>
        /// <param name="slot">Texture slot</param>
        public void SetScreenSourceTexture(string uniform, int handle, int slot = 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, handle);
            Shader.SetUniform(uniform, slot);
        }

        /// <summary>
        /// Bind the shader
        /// </summary>
        public void Bind()
        {
            Shader.Bind();
            for (int i = 0; i < _textures.Count; i++)
                _textures[i].Bind(i);
        }

        /// <summary>
        /// Unbind the shader
        /// </summary>
        public void Unbind() => Shader.Unbind();

        /// <summary>
        /// Load the material
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="pack">Package</param>
        /// <returns>Loaded material</returns>
        public ILoadable Load(string path, ZipFile pack)
        {
            if (pack.ContainsEntry(path))
            {
                var entry = pack[path];
                MemoryStream outputStream = new MemoryStream();
                entry.Extract(outputStream);
                string text = Encoding.ASCII.GetString(outputStream.ToArray());
                string[] lines = text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim('\r');
                }
                Debug.Log("Loaded material from pack");
                return new Material(lines, Path.GetFileName(path));
            }

            Debug.Log("Loaded material from file");
            return new Material(path, Path.GetFileName(path));
        }
    }
}
