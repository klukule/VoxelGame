using Ionic.Zip;
using Newtonsoft.Json;
using OpenTK;
using System.IO;
using System.Text;
using VoxelGame.Blocks;
using VoxelGame.Rendering;

namespace VoxelGame.Assets
{
    /// <summary>
    /// Texture pack definition
    /// </summary>
    public class TexturePack : ILoadable
    {
        // Some constants for asset location
        private const string TEXTURES_LOC = "Textures/";
        private const string META_LOC = "Pack/Pack.json";
        private const string ATLAS_LOC = "Pack/Blocks.json";
        public const string DEFAULT_PATH = "Packs/Default/";

        /// <summary>
        /// Texture pack name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Texture pack description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Texture pack icon
        /// </summary>
        public Texture IconTexture { get; set; }

        /// <summary>
        /// Block atlas texture
        /// </summary>
        public Texture Blocks { get; set; }

        /// <summary>
        /// Crosshair texture
        /// </summary>
        public Texture Crosshair { get; set; }

        /// <summary>
        /// Texture atlas metadata
        /// </summary>
        public TexturePackBlocks BlockData { get; set; }

        /// <summary>
        /// Loads texture pack from path
        /// </summary>
        /// <param name="path">The path - UNUSED</param>
        /// <param name="pack">The package</param>
        /// <returns>Texture pack instance</returns>
        public ILoadable Load(string path, ZipFile pack)
        {
            // Load and deserialize texture pack meta
            MemoryStream stream = new MemoryStream();
            pack[META_LOC].Extract(stream);
            TexturePack texPack = JsonConvert.DeserializeObject<TexturePack>(Encoding.ASCII.GetString(stream.ToArray()));//(File.ReadAllText(path + "/Pack.json"));

            // Load textures
            texPack.IconTexture = AssetDatabase.GetAsset<Texture>(TEXTURES_LOC + "Pack_Icon.png");
            texPack.Blocks = AssetDatabase.GetAsset<Texture>(TEXTURES_LOC + "Blocks.png");
            texPack.Crosshair = AssetDatabase.GetAsset<Texture>(TEXTURES_LOC + "GUI/Crosshair.png");

            // Load block atlas metadata
            stream = new MemoryStream();
            pack[ATLAS_LOC].Extract(stream);
            texPack.BlockData = JsonConvert.DeserializeObject<TexturePackBlocks>(Encoding.ASCII.GetString(stream.ToArray()));

            // One texture atlas slot size
            float oneSlotX = 1f / texPack.BlockData.BlocksPerRow;
            float oneSlotY = 1f / texPack.BlockData.BlocksPerColumn;
            
            // Caluculate UV and Mask for each block
            foreach (var block in texPack.BlockData.Blocks)
            {
                var bl = BlockDatabase.GetBlock(block.Id);

                if (bl == null)
                    continue;
                {
                    block.Back.X *= oneSlotX;
                    block.Back.Y *= oneSlotY;

                    block.Front.X *= oneSlotX;
                    block.Front.Y *= oneSlotY;

                    block.Left.X *= oneSlotX;
                    block.Left.Y *= oneSlotY;

                    block.Right.X *= oneSlotX;
                    block.Right.Y *= oneSlotY;

                    block.Top.X *= oneSlotX;
                    block.Top.Y *= oneSlotY;

                    block.Bottom.X *= oneSlotX;
                    block.Bottom.Y *= oneSlotY;

                    block.BackMask.X *= oneSlotX;
                    block.BackMask.Y *= oneSlotY;

                    block.FrontMask.X *= oneSlotX;
                    block.FrontMask.Y *= oneSlotY;

                    block.LeftMask.X *= oneSlotX;
                    block.LeftMask.Y *= oneSlotY;

                    block.RightMask.X *= oneSlotX;
                    block.RightMask.Y *= oneSlotY;

                    block.TopMask.X *= oneSlotX;
                    block.TopMask.Y *= oneSlotY;

                    block.BottomMask.X *= oneSlotX;
                    block.BottomMask.Y *= oneSlotY;

                }

                bl.Back = new Block.Face(
                    new Rect(block.Back.X, block.Back.Y, block.Back.X + oneSlotX, block.Back.Y + oneSlotY),
                    new Rect(block.BackMask.X, block.BackMask.Y, block.BackMask.X + oneSlotX, block.BackMask.Y + oneSlotY)
                );

                bl.Front = new Block.Face(
                    new Rect(block.Front.X, block.Front.Y, block.Front.X + oneSlotX, block.Front.Y + oneSlotY),
                    new Rect(block.FrontMask.X, block.FrontMask.Y, block.FrontMask.X + oneSlotX, block.FrontMask.Y + oneSlotY)
                );

                bl.Left = new Block.Face(
                    new Rect(block.Left.X, block.Left.Y, block.Left.X + oneSlotX, block.Left.Y + oneSlotY),
                    new Rect(block.LeftMask.X, block.LeftMask.Y, block.LeftMask.X + oneSlotX, block.LeftMask.Y + oneSlotY)
                );

                bl.Right = new Block.Face(
                    new Rect(block.Right.X, block.Right.Y, block.Right.X + oneSlotX, block.Right.Y + oneSlotY),
                    new Rect(block.RightMask.X, block.RightMask.Y, block.RightMask.X + oneSlotX, block.RightMask.Y + oneSlotY)
                );

                bl.Top = new Block.Face(
                    new Rect(block.Top.X, block.Top.Y, block.Top.X + oneSlotX, block.Top.Y + oneSlotY),
                    new Rect(block.TopMask.X, block.TopMask.Y, block.TopMask.X + oneSlotX, block.TopMask.Y + oneSlotY)
                );

                bl.Bottom = new Block.Face(
                    new Rect(block.Bottom.X, block.Bottom.Y, block.Bottom.X + oneSlotX, block.Bottom.Y + oneSlotY),
                    new Rect(block.BottomMask.X, block.BottomMask.Y, block.BottomMask.X + oneSlotX, block.BottomMask.Y + oneSlotY)
                );

                BlockDatabase.SetBlock(block.Id, bl);
            }

            return texPack;
        }

        /// <summary>
        /// Dispose any assets
        /// </summary>
        public void Dispose()
        {
            IconTexture?.Dispose();
            Blocks?.Dispose();
        }

        /// <summary>
        /// Structure containing parsed block information from texture pack
        /// </summary>
        public class TexturePackBlocks
        {
            /// <summary>
            /// Blocks per texture atlas row
            /// </summary>
            public int BlocksPerRow;

            /// <summary>
            /// Blocks per texture atlas column
            /// </summary>
            public int BlocksPerColumn;

            /// <summary>
            /// Block information
            /// </summary>
            public Block[] Blocks;

            /// <summary>
            /// Structure containing UVs and Mask for given block
            /// </summary>
            public class Block
            {
                public string Id { get; set; }
                public Vector2 Top = Vector2.Zero;
                public Vector2 Bottom = Vector2.Zero;
                public Vector2 Left = Vector2.Zero;
                public Vector2 Right = Vector2.Zero;
                public Vector2 Front = Vector2.Zero;
                public Vector2 Back = Vector2.Zero;

                public Vector2 TopMask = Vector2.Zero;
                public Vector2 BottomMask = Vector2.Zero;
                public Vector2 LeftMask = Vector2.Zero;
                public Vector2 RightMask = Vector2.Zero;
                public Vector2 FrontMask = Vector2.Zero;
                public Vector2 BackMask = Vector2.Zero;
            }
        }
    }
}
