using OpenTK;
using System;
using VoxelGame.Assets;
using VoxelGame.Blocks;
using VoxelGame.Rendering;

namespace VoxelGame.Worlds
{
    // TODO: Investigate possibility of cubic chunks for faster remeshing
    /// <summary>
    /// Single chunk
    /// </summary>
    public partial class Chunk : IDisposable
    {
        /// <summary>
        /// Chunk block info
        /// </summary>
        public class BlockState
        {
            public short id;
            public sbyte x;
            public sbyte y;
            public sbyte z;

            public BlockState(sbyte x, sbyte y, sbyte z, Chunk chunk)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
        }

        // Shared materials
        private static Material _chunkMaterial;
        private static Material _chunkWaterMaterial;

        // Chunk size constants
        public const int WIDTH = 16;
        public const int HEIGHT = 128;

        // Cached chunk world transform matrix
        private Matrix4 worldMatrix;

        /// <summary>
        /// Chunk position
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// Whether or not are all neighboring chunks loaded
        /// </summary>
        public bool AreAllNeighborsSet => LeftNeighbor != null && RightNeighbor != null && FrontNeighbor != null && BackNeighbor != null;

        // Neighbors 
        public Chunk LeftNeighbor;
        public Chunk RightNeighbor;
        public Chunk FrontNeighbor;
        public Chunk BackNeighbor;

        /// <summary>
        /// Chunk blocks
        /// </summary>
        public BlockState[,,] Blocks = new BlockState[WIDTH, HEIGHT, WIDTH];

        public Chunk(Vector2 position)
        {
            Position = position;
            worldMatrix = Matrix4.CreateTranslation(Position.X * WIDTH, 0, Position.Y * WIDTH);

            //Default heightmap used for icon generation
            _heightmap = new float[WIDTH, WIDTH];
            _heightmap[0, 0] = 1;
        }


        /// <summary>
        /// Gets block state at chunk relative coordinates - automatically queries neighboring chunks when available
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <returns>Block state or null if not available</returns>
        public BlockState GetBlockState(int x, int y, int z)
        {
            // If out of current chunks bounds - check neighbors
            if (x <= -1)
            {
                if (LeftNeighbor != null)
                    return LeftNeighbor.GetBlockState(WIDTH + x, y, z);

                return null;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbor != null)
                    return RightNeighbor.GetBlockState(x - WIDTH, y, z);

                return null;
            }

            if (z <= -1)
            {
                if (BackNeighbor != null)
                    return BackNeighbor.GetBlockState(x, y, WIDTH + z);

                return null;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbor != null)
                    return FrontNeighbor.GetBlockState(x, y, z - WIDTH);

                return null;
            }

            // Check height
            if (y < 0 || y > HEIGHT - 1)
                return null;

            if (Blocks[x, y, z] == null)
                return null;

            return Blocks[x, y, z];
        }

        /// <summary>
        /// Gets block id for given chunk-relative coordinates, automatically queries neighbors when necessary
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <returns>Numerical block id or -1 if not found</returns>
        public short GetBlockID(int x, int y, int z)
        {
            if (x <= -1)
            {
                if (LeftNeighbor != null)
                    return LeftNeighbor.GetBlockID(WIDTH + x, y, z);

                return -1;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbor != null)
                    return RightNeighbor.GetBlockID(x - WIDTH, y, z);

                return -1;
            }

            if (z <= -1)
            {
                if (BackNeighbor != null)
                    return BackNeighbor.GetBlockID(x, y, WIDTH + z);

                return -1;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbor != null)
                    return FrontNeighbor.GetBlockID(x, y, z - WIDTH);

                return -1;
            }

            if (y < 0 || y > HEIGHT - 1)
                return 0;

            if (Blocks[x, y, z] == null)
                return 0;

            return Blocks[x, y, z].id;
        }

        /// <summary>
        /// Gets chunk height at given X,Z coordinate
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="z">Z</param>
        /// <returns>Returns chunk height at given coordinate or 0</returns>
        public int GetHeightAtBlock(int x, int z)
        {
            if (x <= -1)
            {
                if (LeftNeighbor != null)
                    return LeftNeighbor.GetHeightAtBlock(WIDTH + x, z);

                return 0;
            }

            if (x >= WIDTH)
            {
                if (RightNeighbor != null)
                    return RightNeighbor.GetHeightAtBlock(x - WIDTH, z);

                return 0;
            }

            if (z <= -1)
            {
                if (BackNeighbor != null)
                    return BackNeighbor.GetHeightAtBlock(x, WIDTH + z);

                return 0;
            }

            if (z >= WIDTH)
            {
                if (FrontNeighbor != null)
                    return FrontNeighbor.GetHeightAtBlock(x, z - WIDTH);

                return 0;
            }

            // Remap range
            int h = (int)(_heightmap[x, z] / 4);
            h += 16;
            return h;
        }

        /// <summary>
        /// Places the block at specified coordinates
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="block">The block</param>
        /// <param name="updateChunk">Whether to cause chunk update or not</param>
        public void PlaceBlock(int x, int y, int z, Block block, bool updateChunk = true) => PlaceBlock(x, y, z, (short)block.ID, updateChunk);

        /// <summary>
        /// Places the block at specified coordinates
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="blockIndex">The block ID</param>
        /// <param name="updateChunk">Whether to cause chunk update or not</param>
        public void PlaceBlock(int x, int y, int z, short blockIndex, bool updateChunk = true)
        {
            if (y >= HEIGHT)
            {
                Debug.Log($"Tried placing a block at: {x},{y},{z} but the Y value is too high!", DebugLevel.Warning);
                return;
            }

            if (x <= -1)
            {
                if (LeftNeighbor != null)
                    LeftNeighbor.PlaceBlock(WIDTH + x, y, z, blockIndex, updateChunk);
            }
            else if (x >= WIDTH)
            {
                if (RightNeighbor != null)
                    RightNeighbor.PlaceBlock(x - WIDTH, y, z, blockIndex, updateChunk);
            }
            else if (z <= -1)
            {
                if (BackNeighbor != null)
                    BackNeighbor.PlaceBlock(x, y, WIDTH + z, blockIndex, updateChunk);
            }
            else if (z >= WIDTH)
            {
                if (FrontNeighbor != null)
                    FrontNeighbor.PlaceBlock(x, y, z - WIDTH, blockIndex, updateChunk);
            }
            else if (Blocks[x, y, z] != null)
            {
                Blocks[x, y, z].id = blockIndex;
                Rebuild();
            }
            else if (y < HEIGHT)
            {
                Blocks[x, y, z] = new BlockState((sbyte)x, (sbyte)y, (sbyte)z, this);
                Blocks[x, y, z].id = blockIndex;
                Rebuild();
            }

            // Performs chunk update and updates chunk heightmap
            void Rebuild()
            {
                if (updateChunk)
                    World.Instance.RequestChunkUpdate(this, true, x, z, true);

                if (y > GetHeightAtBlock(x, z))
                {
                    // Remap range
                    int newY = y - 16;
                    newY *= 4;
                    int oldHeight = (int)_heightmap[x, z];
                    _heightmap[x, z] = newY;
                    Debug.Log("Updated the heightmap from: " + oldHeight + " to " + newY);
                }
            }
        }

        /// <summary>
        /// Destroy block at given coordintates
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        public void DestroyBlock(int x, int y, int z)
        {
            PlaceBlock(x, y, z, 0, true);
            if (y == GetHeightAtBlock(x, z))
            {
                // Remap range
                int newY = y - 16;
                newY *= 4;
                int oldHeight = (int)_heightmap[x, z];
                _heightmap[x, z] = newY;
                Debug.Log("Updated the heightmap from: " + oldHeight + " to " + newY);
            }
        }

        /// <summary>
        /// Render chunk icon (1x1x1 block preview)
        /// </summary>
        public void RenderForIcon()
        {
            // Build mesh
            if (shouldRebuildMesh)
            {
                // Load material & assign texture
                if (_chunkMaterial == null)
                {
                    _chunkMaterial = AssetDatabase.GetAsset<Material>("Materials/World/Blocks.mat");
                    if (World.Instance != null) // We can access loaded texture pack if we have world loaded
                        _chunkMaterial.SetTexture(0, World.Instance.TexturePack.Blocks);
                    else // Otherwise we need to load the texture pack from cache
                        _chunkMaterial.SetTexture(0, AssetDatabase.GetAsset<TexturePack>("").Blocks);
                }

                mesh?.Dispose();
                mesh = new Mesh(blockContainer, indices);
                shouldRebuildMesh = false;
            }

            // Render the block preview immediately
            if (mesh != null)
            {
                Matrix4 mat =
                    Matrix4.CreateTranslation(-1, -0.9f, 0) * Matrix4.CreateFromQuaternion(new Quaternion(-0.2391197f, 0.369638f, -0.09904902f, 0.8924006f)) *
                    Matrix4.CreateScale(1, -1, -1);
                Renderer.DrawNow(mesh, _chunkMaterial, mat);
            }
        }

        /// <summary>
        /// Standard chunk rendering
        /// </summary>
        public void Render()
        {
            // Build mesh
            if (shouldRebuildMesh)
            {
                // Load material & assign texture
                if (_chunkMaterial == null)
                {
                    _chunkMaterial = AssetDatabase.GetAsset<Material>("Materials/World/Blocks.mat");
                    if (World.Instance != null) // We can access loaded texture pack if we have world loaded
                        _chunkMaterial.SetTexture(0, World.Instance.TexturePack.Blocks);
                    else // Otherwise we need to load the texture pack from cache
                        _chunkMaterial.SetTexture(0, AssetDatabase.GetAsset<TexturePack>("").Blocks);
                }

                mesh?.Dispose();
                mesh = new Mesh(blockContainer, indices);
                shouldRebuildMesh = false;
            }

            // Build water mesh
            if (shouldRebuildWaterMesh)
            {
                // Load material & assign texture
                if (_chunkWaterMaterial == null)
                {
                    _chunkWaterMaterial = AssetDatabase.GetAsset<Material>("Materials/World/Water.mat");
                    if (World.Instance != null) // We can access loaded texture pack if we have world loaded
                        _chunkWaterMaterial.SetTexture(0, World.Instance.TexturePack.Blocks);
                    else // Otherwise we need to load the texture pack from cache
                        _chunkWaterMaterial.SetTexture(0, AssetDatabase.GetAsset<TexturePack>("").Blocks);
                }

                waterMesh?.Dispose();
                waterMesh = new Mesh(waterContainer, indicesWater);
                shouldRebuildWaterMesh = false;
            }

            // Enqueue for drawing if meshes are valid
            if (waterMesh != null)
                Renderer.DrawRequest(waterMesh, _chunkWaterMaterial, worldMatrix);
            if (mesh != null)
                Renderer.DrawRequest(mesh, _chunkMaterial, worldMatrix);
        }

        /// <summary>
        /// Dispose meshes
        /// </summary>
        public void Dispose()
        {
            // Remove link to neighboring chunks
            if (RightNeighbor != null)
            {
                RightNeighbor.LeftNeighbor = null;
                RightNeighbor = null;
            }

            if (LeftNeighbor != null)
            {
                LeftNeighbor.RightNeighbor = null;
                LeftNeighbor = null;
            }

            if (BackNeighbor != null)
            {
                BackNeighbor.FrontNeighbor = null;
                BackNeighbor = null;
            }

            if (FrontNeighbor != null)
            {
                FrontNeighbor.BackNeighbor = null;
                FrontNeighbor = null;
            }

            // Dispose meshes
            mesh?.Dispose();
            waterMesh?.Dispose();
        }
    }
}
