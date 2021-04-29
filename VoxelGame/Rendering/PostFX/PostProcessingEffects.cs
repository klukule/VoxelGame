using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace VoxelGame.Rendering.PostFX
{
    /// <summary>
    /// PostFX Manager
    /// </summary>
    public static class PostProcessingEffects
    {
        private static List<BlitEffect> _effects = new List<BlitEffect>();

        /// <summary>
        /// Dispose any effects
        /// </summary>
        public static void Dispose()
        {
            foreach (var blitEffect in _effects)
                blitEffect.Dispose();
        }

        /// <summary>
        /// Register new postfx
        /// </summary>
        /// <param name="effect">PostFX</param>
        public static void RegisterEffect(BlitEffect effect) => _effects.Add(effect);

        /// <summary>
        /// Render the PostFX stack
        /// </summary>
        public static void RenderEffects()
        {
            if (_effects.Count == 0) return;

            _effects[0].PreRender(_effects.Count == 1);     // Pre-render
            _effects[0].Render(Renderer.FrameBuffer);       // Bind output from 3D renderer
            _effects[0].PostRender(_effects.Count == 1);    // Post-render

            for (var index = 1; index < _effects.Count; index++)
            {
                var effect = _effects[index];

                // If last enable sRGB output
                if (index == _effects.Count - 1)
                    GL.Enable(EnableCap.FramebufferSrgb);
                else
                    GL.Disable(EnableCap.FramebufferSrgb);

                effect.PreRender(index == _effects.Count - 1);  // Pre-render
                effect.Render(_effects[index - 1].SourceFbo);   // Bind output from previous effect
                effect.PostRender(index == _effects.Count - 1); // Post-render
            }
        }
    }
}
