using System;
using System.Collections.Generic;
using System.Text;
using VoxelGame.Blocks;

namespace VoxelGame.Items
{
    public static class GameItems
    {
        public static void Init()
        {
            // TODO: Remove NULL block once item icon generation is fixed up... and make Block abstract again
            var NULL = new BlockItem(new Block()); // Null/air block 
            GRASS = new BlockItem(GameBlocks.GRASS);
            STONE = new BlockItem(GameBlocks.STONE);
            SAND = new BlockItem(GameBlocks.SAND);
            LOG_OAK = new BlockItem(GameBlocks.LOG_OAK);
            PLANKS_OAK = new BlockItem(GameBlocks.PLANKS_OAK);
            DIRT = new BlockItem(GameBlocks.DIRT);
            GLOWSTONE = new BlockItem(GameBlocks.GLOWSTONE);
        }

        public static BlockItem DIRT { get; private set; }

        public static BlockItem GRASS { get; private set; }

        public static BlockItem STONE { get; private set; }

        public static BlockItem SAND { get; private set; }

        public static BlockItem LOG_OAK { get; private set; }

        public static BlockItem PLANKS_OAK { get; private set; }
        public static BlockItem GLOWSTONE { get; private set; }
    }
}
