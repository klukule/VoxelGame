using OpenTK.Graphics.OpenGL4;
using OpenTK;
using OpenTK.Input;
using System;
using OpenTK.Graphics;
using VoxelGame.Assets;
using VoxelGame.Rendering.Buffers;
using VoxelGame.Worlds;
using VoxelGame.UI.Menus;
using VoxelGame.Rendering.PostFX;

namespace VoxelGame
{
    /// <summary>
    /// Game Window and Main Game loop
    /// </summary>
    public partial class Window : GameWindow
    {
        /// <summary>
        /// Standard clear color
        /// </summary>
        public static readonly Vector4 CLEAR_COLOR = new Vector4(0, 0, 0, 1); // new Vector4(.39f, .58f, .92f, 1.0f);

        /// <summary>
        /// Window width
        /// </summary>
        public static int WindowWidth { get; private set; }

        /// <summary>
        /// Window height
        /// </summary>
        public static int WindowHeight { get; private set; }

        /// <summary>
        /// Is done loading base assets
        /// </summary>
        public static bool IsLoadingDone { get; private set; }

        /// <summary>
        /// Main menu instance
        /// </summary>
        public MainMenu MainMenu { get; private set; }

        /// <summary>
        /// Create new instance of the window
        /// </summary>
        public Window(int width, int height, string title) : base(width, height, GraphicsMode.Default, title) { }

        protected override void OnLoad(EventArgs e)
        {
            Initialize();
            OnResize(null);
            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            Deinitialize();
            base.OnUnload(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Update((float)e.Time);
            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (IsLoadingDone)
                Render();

            Context.SwapBuffers();
            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (IsLoadingDone)
                GL.Viewport(ClientRectangle);
            base.OnResize(e);
        }
    }
}
