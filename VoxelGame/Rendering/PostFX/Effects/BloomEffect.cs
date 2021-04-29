using VoxelGame.Assets;

namespace VoxelGame.Rendering.PostFX
{
    /// <summary>
    /// Bloom postfx
    /// </summary>
    public class Bloom : BlitEffect
    {
        private Material _cutoffMaterial = AssetDatabase.GetAsset<Material>("Materials/PostFX/Cutoff.mat");
        private Material _blurMaterial = AssetDatabase.GetAsset<Material>("Materials/PostFX/Blur.mat");

        // TODO: Bind resize
        private FBO _fbo = new FBO((int)((float)Program.Window.Width / 4), (int)((float)Program.Window.Height / 4), FBO.DepthBufferType.RenderBuffer);
        public override Material BlitMaterial => AssetDatabase.GetAsset<Material>("Materials/PostFX/Bloom.mat");

        public int BlurIterations { get; set; } = 5;
        public float BrightnessCutoff { get; set; } = .8f;
        public float BloomStrength { get; set; } = .009f;

        public override void Render(FBO src)
        {
            // Render bright spots to 1/4 sized frame buffer
            _cutoffMaterial.Shader.SetUniform("u_Cutoff", BrightnessCutoff);
            Blit(src, _fbo, _cutoffMaterial);

            // Perform blur passes on it
            for (int i = 0; i < BlurIterations; i++)
                Blit(_fbo, _fbo, _blurMaterial);

            PreRender(IsLastEffectInStack);

            // Draw the bloom to screen
            BlitMaterial.Shader.SetUniform("u_BlurIterations", BlurIterations);
            BlitMaterial.Shader.SetUniform("u_BloomStrength", BloomStrength);
            BlitMaterial.SetScreenSourceTexture("u_Src_Small", _fbo.ColorHandle, 1);
            BlitMaterial.SetScreenSourceTexture("u_Src", src.ColorHandle);
            Renderer.DrawNow(BlitMesh, BlitMaterial);
        }
    }
}
