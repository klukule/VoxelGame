using Ionic.Zip;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VoxelGame.Assets;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Texture object
    /// </summary>
    public class Texture : ILoadable, IDisposable
    {
        private TextureWrapMode _wrapMode = TextureWrapMode.ClampToEdge;    // Texture wrap mode
        private List<Color4> _pixels = new List<Color4>();                  // RGBA pixels
        private List<byte> _pixelBytes = new List<byte>();                  // Pixels in staggered byte form: R,G,B,A,R,G,B,A....

        /// <summary>
        /// Texture handle
        /// </summary>
        public int Handle { get; }

        /// <summary>
        /// Texture width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Texture height
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Texture wrap mode
        /// </summary>
        public TextureWrapMode WrapMode
        {
            get => _wrapMode;
            set
            {
                if (_wrapMode == value || Handle == 0) return;
                _wrapMode = value;
                GL.TextureParameter(Handle, TextureParameterName.TextureWrapS, (int)_wrapMode);
                GL.TextureParameter(Handle, TextureParameterName.TextureWrapT, (int)_wrapMode);
            }
        }

        /// <summary>
        /// Activator constructor
        /// </summary>
        public Texture() { }

        /// <summary>
        /// Creates new texture from the file
        /// </summary>
        private Texture(string file, bool srgb = true, bool mips = true) : this(File.ReadAllBytes(file), srgb, mips) { }

        /// <summary>
        /// Creates new texture from image byte buffer
        /// </summary>
        private Texture(byte[] buffer, bool srgb = true, bool mips = true)
        {
            Image<Rgba32> img = Image.Load(buffer);
            Width = img.Width;
            Height = img.Height;
            Rgba32[] pixels = img.GetPixelMemoryGroup().Single().Span.ToArray();

            // Cache pixels
            foreach (Rgba32 p in pixels)
            {
                _pixelBytes.Add(p.R);
                _pixelBytes.Add(p.G);
                _pixelBytes.Add(p.B);
                _pixelBytes.Add(p.A);

                _pixels.Add(new Color4(p.R / 255f, p.G / 255f, p.B / 255f, p.A / 255f));
            }

            // Generate OpenGL texture
            Handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)_wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)_wrapMode);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexImage2D(TextureTarget.Texture2D, 0, srgb ? PixelInternalFormat.Srgb8Alpha8 : PixelInternalFormat.Rgba8, img.Width, img.Height,
                0, PixelFormat.Rgba, PixelType.UnsignedByte, _pixelBytes.ToArray());

            // Generate mips
            if (mips) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        /// <summary>
        /// Creates emapty texture of given size
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public Texture(int width, int height)
        {
            Handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, Handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)_wrapMode);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)_wrapMode);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Alpha, width, height,
                0, PixelFormat.Alpha, PixelType.UnsignedByte, IntPtr.Zero);
        }

        /// <summary>
        /// Creates new texture from Frame Buffer
        /// </summary>
        /// <param name="fbo">Frame Buffer</param>
        /// <param name="disposeFbo">Whether to dispose the FBO</param>
        public Texture(FBO fbo, bool disposeFbo)
        {
            Width = fbo.Width;
            Height = fbo.Height;
            Handle = fbo.ColorHandle;

            if (disposeFbo) fbo.DisposeWithoutColorHandle();
        }

        /// <summary>
        /// Get pixel at specific position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns></returns>
        public Color4 GetPixel(int x, int y) => _pixels[x * Height + y];

        // TODO: USE SETTERS
        /// <summary>
        /// Set Min filter
        /// </summary>
        public void SetMinFilter(TextureMinFilter filter) => GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int)filter);

        /// <summary>
        /// Set Mag filter
        /// </summary>
        public void SetMagFilter(TextureMagFilter filter) => GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int)filter);

        /// <summary>
        /// Load texture from the database
        /// </summary>
        public ILoadable Load(string path, ZipFile pack)
        {
            bool srgb = true; // All texture files are sRGB

            if (pack.ContainsEntry(path)) // Found in pack
            {
                var entry = pack[path];
                MemoryStream outputStream = new MemoryStream();
                entry.Extract(outputStream);
                Debug.Log("Loaded texture from pack");
                return new Texture(outputStream.GetBuffer(), srgb, srgb);
            }
            else // Fallback to file
            {
                Debug.Log("Loaded texture from file");
                return new Texture(path, srgb, srgb);
            }
        }

        /// <summary>
        /// Bind the texture
        /// </summary>
        public void Bind(int slot = 0)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }

        /// <summary>
        /// Dispose the texture
        /// </summary>
        public void Dispose() => GL.DeleteTexture(Handle);
    }
}
