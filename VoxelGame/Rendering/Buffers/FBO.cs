﻿using System;
using OpenTK.Graphics.OpenGL4;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Frame buffer object
    /// </summary>
    public class FBO : IDisposable
    {
        /// <summary>
        /// Type of bound depth buffer texture
        /// </summary>
        public enum DepthBufferType
        {
            /// <summary>
            /// No depth texture
            /// </summary>
            None = 0,

            /// <summary>
            /// Standard texture
            /// </summary>
            Texture = 1,

            /// <summary>
            /// Render buffer
            /// </summary>
            RenderBuffer = 2
        }

        /// <summary>
        /// Type of depth texture bound
        /// </summary>
        public DepthBufferType Type { get; }

        /// <summary>
        /// Frame buffer width
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Frame bufffer height
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Frame buffer handle
        /// </summary>
        public int Handle { get; }

        /// <summary>
        /// Color texture handle
        /// </summary>
        public int ColorHandle { get; }

        /// <summary>
        /// Depth texture handle
        /// </summary>
        public int DepthHandle { get; }

        /// <summary>
        /// Create new frame buffer of given size
        /// </summary>
        public FBO(int width, int height, DepthBufferType type)
        {
            Type = type;
            Width = width;
            Height = height;

            Handle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            // Initialise color texture
            ColorHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ColorHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba16f, width, height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, ColorHandle, 0);

            // Initialise depth information
            switch (type)
            {
                case DepthBufferType.Texture:
                    DepthHandle = GL.GenTexture();
                    GL.BindTexture(TextureTarget.Texture2D, DepthHandle);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, width, height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthHandle, 0);
                    break;
                case DepthBufferType.RenderBuffer:
                    DepthHandle = GL.GenRenderbuffer();
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, DepthHandle);
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, DepthHandle);
                    break;
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        /// <summary>
        /// Bind framebuffer
        /// </summary>
        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
            GL.Viewport(0, 0, Width, Height);
        }

        /// <summary>
        /// Unbind framebuffer
        /// </summary>
        public void Unbind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(Program.Window.ClientRectangle);
        }

        /// <summary>
        /// Bind for read
        /// </summary>
        public void BindToRead()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Handle);
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
        }

        /// <summary>
        /// Dispose 
        /// </summary>
        public void Dispose()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffer(Handle);
            GL.DeleteTexture(ColorHandle);
            GL.DeleteTexture(DepthHandle);
            GL.DeleteRenderbuffer(DepthHandle);
        }

        /// <summary>
        /// Dispose framebuffer except for color texture
        /// </summary>
        public void DisposeWithoutColorHandle()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DeleteFramebuffer(Handle);
            GL.DeleteTexture(DepthHandle);
            GL.DeleteRenderbuffer(DepthHandle);
        }
    }
}
