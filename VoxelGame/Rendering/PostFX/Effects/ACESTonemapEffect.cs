using VoxelGame.Assets;

namespace VoxelGame.Rendering.PostFX
{
    /// <summary>
    /// ACES Tonemap postfx
    /// </summary>
    public class ACESTonemapEffect : BlitEffect
    {
        public override Material BlitMaterial => AssetDatabase.GetAsset<Material>("Materials/PostFX/ACES.mat");
    }
}
