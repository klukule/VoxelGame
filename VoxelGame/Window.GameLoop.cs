using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using VoxelGame.Assets;
using VoxelGame.Blocks;
using VoxelGame.Containers;
using VoxelGame.Crafting;
using VoxelGame.Items;
using VoxelGame.Physics;
using VoxelGame.Rendering;
using VoxelGame.Rendering.Buffers;
using VoxelGame.Rendering.PostFX;
using VoxelGame.UI;
using VoxelGame.UI.Menus;
using VoxelGame.Worlds;

namespace VoxelGame
{
    public partial class Window
    {
        /// <summary>
        /// Initialize core logic
        /// </summary>
        private void Initialize()
        {
            // Loading start
            IsLoadingDone = false;
            AssetDatabase.Load(AssetDatabase.DEFAULT_DATABASE);         // Open asset database

            GameBlocks.Init();                                          // Initialize blocks
            GameItems.Init();                                           // Initialize items
            CraftingRecipeDatabase.Init();                              // Initialize crafting recipes

            // Setup opengl
            VSync = VSyncMode.Off;
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.FramebufferSrgb);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(CLEAR_COLOR.X, CLEAR_COLOR.Y, CLEAR_COLOR.Z, CLEAR_COLOR.Z);

            AssetDatabase.GetAsset<Material>("Materials/Fallback.mat"); // Load default material

            MainMenu = new MainMenu();                                  // Create main menu
            MainMenu.Show();                                            // Show it

            Program.Settings.UpdateAll();                               // Apply loaded settings

            AssetDatabase.GetAsset<TexturePack>("");                    // Load default texture pack

            IconGenerator.GenerateBlockItemIcons();                     // Generate item icons

            WindowWidth = Program.Settings.WindowWidth;                 // Cache width
            WindowHeight = Program.Settings.WindowHeight;               // Cache height

            // Register postprocessing
            PostProcessingEffects.RegisterEffect(new Bloom());
            PostProcessingEffects.RegisterEffect(new ACESTonemapEffect());

            // Loading end
            IsLoadingDone = true;
        }

        /// <summary>
        /// Deinitialize
        /// </summary>
        private void Deinitialize()
        {
            // Dispose of everything
            World.Instance?.Dispose();
            AssetDatabase.Dispose();
            UniformBuffers.Dispose();
            PostProcessingEffects.Dispose();
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="deltaTime">Delta time </param>
        private void Update(float deltaTime)
        {
            // Update time
            Time.GameTime += deltaTime;
            Time.DeltaTime = deltaTime;

            Time.UpdateFrameRate(1f / Time.DeltaTime);

            if (IsLoadingDone)                                  // Once the loading is done start regular updates
            {
                WindowWidth = Width;
                WindowHeight = Height;

                Input.Update();                                 // Update input

                if (World.Instance != null)                     // If we have any world loaded
                {
                    World.Instance.Update();                    // Update world
                    Sim.Update(Time.DeltaTime);  // Simulate physics
                }

                if (Focused)                                    // Handle Alt+F4 only when focused
                {
                    KeyboardState kbdState = Keyboard.GetState();

                    if (kbdState.IsKeyDown(Key.F4) && kbdState.IsKeyDown(Key.AltLeft))
                        Exit();
                }

                // Update time UBO
                UniformBuffers.TimeBuffer.Update(new TimeUniformBuffer() { DeltaTime = Time.DeltaTime, Time = Time.GameTime });
            }
        }

        /// <summary>
        /// Render frame
        /// </summary>
        private void Render()
        {
            GUI.BeginFrame();                       // Begin frame

            Renderer.CulledCount = 0;               // Reset cull count
            Renderer.DrawCalls = 0;                 // Reset draw calls

            World.Instance?.Render();               // Render voxel world and entities
            Renderer.DrawQueue();                   // Draw queued drawcalls

            PostProcessingEffects.RenderEffects();  // Render PostFX

            World.Instance?.RenderGUI();            // Render World UI
            ContainerRenderer.RenderGUI();          // Render Containers
            Menu.RenderGUI();                       // Render Menus

            Input.PostRenderUpdate();               // Reset input state

            GUI.EndFrame();                         // End frame
        }
    }
}
