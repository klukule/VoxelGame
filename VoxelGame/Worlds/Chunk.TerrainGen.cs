using OpenTK;
using System;
using VoxelGame.Blocks;
using VoxelGame.Worlds.Decoration;
using VoxelGame.Worlds;

namespace VoxelGame.Worlds
{
    /// <summary>
    /// Contains methods used to generate raw terrain data
    /// </summary>
    public partial class Chunk
    {
        private float _noiseScale = 0.25f;
        private float[,] _heightmap;
        public float[,] Heightmap => _heightmap;

        /// <summary>
        /// Generates new height map data
        /// </summary>
        public void GenerateHeightMap()
        {
            _heightmap = new float[WIDTH, WIDTH];
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < WIDTH; y++)
                {
                    float NoiseX = (x / (float)WIDTH) + (Position.X);
                    float NoiseY = (y / (float)WIDTH) + (Position.Y);
                    float mainNoise = (float)((World.Instance.TerrainNoise
                                                .Octaves2D(NoiseX, NoiseY, 8, .4f, 2, _noiseScale) + 1) / 2);

                    float ocean = (float)((World.Instance.TerrainNoise
                                               .Octaves2D(NoiseX, NoiseY, 8, .4f, 2, _noiseScale * 0.125f) + 1) / 2) * 6;

                    ocean -= 2;
                    ocean = (float)Math.Pow(MathHelper.Clamp(ocean, 0, 1) + (mainNoise / 10f), 0.6f);

                    _heightmap[x, y] = Math.Max(0, Math.Min(mainNoise + 0.2f, ocean) * 255f);
                }
            }
        }

        /// <summary>
        /// Fill chunk with blocks based on heightmap
        /// </summary>
        public void FillBlocks()
        {
            // Fill chunk with blocks
            for (int x = 0; x < WIDTH; x++)
            {
                for (int z = 0; z < WIDTH; z++)
                {
                    int h = GetHeightAtBlock(x, z);
                    for (int y = 0; y < HEIGHT; y++)
                    {
                        Blocks[x, y, z] = new BlockState((sbyte)x, (sbyte)y, (sbyte)z, this);

                        if (y > h) // Above surface
                        {
                            if (y <= World.Instance.WaterHeight) // Below or at water level
                                Blocks[x, y, z].id = (short)GameBlocks.WATER.ID;

                        }
                        else if (y == h) // At surface
                        {
                            if (y < World.Instance.WaterHeight + 3) // If near water spawn sand
                                Blocks[x, y, z].id = (short)GameBlocks.SAND.ID;
                            else // Otherwise spawn top layer of grass
                                Blocks[x, y, z].id = (short)GameBlocks.GRASS.ID;

                        }
                        else if (y > h - 5) // If less than 5 blocks below surface
                        {
                            if (y < World.Instance.WaterHeight + 3) // If near water spawn sand
                                Blocks[x, y, z].id = (short)GameBlocks.SAND.ID;
                            else // Otherwise dirt
                                Blocks[x, y, z].id = (short)GameBlocks.DIRT.ID;
                        }
                        else // Otherwise stone
                        {
                            Blocks[x, y, z].id = (short)GameBlocks.STONE.ID;
                        }
                    }
                }
            }
            // Spawn OAK trees
            // TODO: Use list of decorators
            using (OakDecorator decorator = new OakDecorator())
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    for (int z = 0; z < WIDTH; z++)
                    {
                        int h = GetHeightAtBlock(x, z);
                        decorator.DecorateAtBlock(this, x, h, z);
                    }
                }
            }
        }

    }
}
