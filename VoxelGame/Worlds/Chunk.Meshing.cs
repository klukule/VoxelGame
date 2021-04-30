using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Blocks;
using VoxelGame.Rendering;
using VoxelGame.Rendering.Vertex;

namespace VoxelGame.Worlds
{
    /// <summary>
    /// Contains mesh generation and light calculation functions
    /// </summary>
    public partial class Chunk
    {
        private VertexBlockContainer blockContainer;
        private VertexNormalContainer waterContainer;

        private bool shouldRebuildMesh;
        private bool shouldRebuildWaterMesh;

        private uint[] indices;
        private uint[] indicesWater;

        private Mesh mesh;
        private Mesh waterMesh;
        /// <summary>
        /// Generates chunk mesh and calculates lighting
        /// </summary>
        public void GenerateMesh()
        {
            // Initialize structures
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uv2 = new List<Vector2>();
            List<Vector4> col = new List<Vector4>();
            List<uint> light = new List<uint>();

            List<Vector3> verticesWater = new List<Vector3>();
            List<Vector2> uvsWater = new List<Vector2>();
            List<Vector3> normalsWater = new List<Vector3>();

            List<uint> indices = new List<uint>();
            List<uint> indicesWater = new List<uint>();

            uint indexCount = 0;
            uint indexCountWater = 0;

            Block workingBlock = null;



            var lightmap = _lightmap;

            /////////////////////////////////////////
            /// MESH CONSTRUCTION
            /////////////////////////////////////////

            // Foreach block in chunk
            for (int x = 0; x < WIDTH; x++)
            {
                for (int z = 0; z < WIDTH; z++)
                {
                    for (int y = 0; y < HEIGHT; y++)
                    {
                        int id = GetBlockID(x, y, z);

                        // Skip air
                        if (id == 0)
                            continue;

                        // Check if block has any face that can be rendered, if so add it to the buffer

                        workingBlock = BlockDatabase.GetBlock(id);

                        // Water is special case
                        if (workingBlock.ID == GameBlocks.WATER.ID)
                        {
                            if (ShouldDrawBlockFacing(x, y, z - 1, workingBlock.ID))
                                AddBackFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x, y, z + 1, workingBlock.ID))
                                AddFrontFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x - 1, y, z, workingBlock.ID))
                                AddLeftFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x + 1, y, z, workingBlock.ID))
                                AddRightFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x, y + 1, z, workingBlock.ID))
                                AddTopFaceWater(x, y, z);

                            if (ShouldDrawBlockFacing(x, y - 1, z, workingBlock.ID))
                                AddBottomFaceWater(x, y, z);
                        }
                        else // Standard block
                        {
                            if (ShouldDrawBlockFacing(x, y, z - 1, workingBlock.ID))
                                AddBackFace(x, y, z);

                            if (ShouldDrawBlockFacing(x, y, z + 1, workingBlock.ID))
                                AddFrontFace(x, y, z);

                            if (ShouldDrawBlockFacing(x - 1, y, z, workingBlock.ID))
                                AddLeftFace(x, y, z);

                            if (ShouldDrawBlockFacing(x + 1, y, z, workingBlock.ID))
                                AddRightFace(x, y, z);

                            if (ShouldDrawBlockFacing(x, y + 1, z, workingBlock.ID))
                                AddTopFace(x, y, z);

                            if (ShouldDrawBlockFacing(x, y - 1, z, workingBlock.ID))
                                AddBottomFace(x, y, z);
                        }
                    }
                }
            }

            // Block Z+ face
            void AddFrontFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(1 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 1 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 1 + z));

                uint lightVal = lightmap[x + WIDTH, y, z + WIDTH + 1];
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.Front.UV1.X, workingBlock.Front.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Front.UV1.Width, workingBlock.Front.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Front.UV1.Width, workingBlock.Front.UV1.Height));
                uvs.Add(new Vector2(workingBlock.Front.UV1.X, workingBlock.Front.UV1.Height));

                uv2.Add(new Vector2(workingBlock.Front.UV2.X, workingBlock.Front.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Front.UV2.Width, workingBlock.Front.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Front.UV2.Width, workingBlock.Front.UV2.Height));
                uv2.Add(new Vector2(workingBlock.Front.UV2.X, workingBlock.Front.UV2.Height));

                col.Add(workingBlock.BlockColor(x, y, z).ToVector4());
                col.Add(workingBlock.BlockColor(x, y, z).ToVector4());
                col.Add(workingBlock.BlockColor(x, y, z).ToVector4());
                col.Add(workingBlock.BlockColor(x, y, z).ToVector4());

                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));
                normals.Add(new Vector3(0, 0, 1));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            // Block Z- face
            void AddBackFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(0 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(1 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 0 + z));

                uint lightVal = lightmap[x + WIDTH, y, z + WIDTH - 1];
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.Back.UV1.X, workingBlock.Back.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Back.UV1.Width, workingBlock.Back.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Back.UV1.Width, workingBlock.Back.UV1.Height));
                uvs.Add(new Vector2(workingBlock.Back.UV1.X, workingBlock.Back.UV1.Height));

                uv2.Add(new Vector2(workingBlock.Back.UV2.X, workingBlock.Back.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Back.UV2.Width, workingBlock.Back.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Back.UV2.Width, workingBlock.Back.UV2.Height));
                uv2.Add(new Vector2(workingBlock.Back.UV2.X, workingBlock.Back.UV2.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(0, 0, -1));
                normals.Add(new Vector3(0, 0, -1));
                normals.Add(new Vector3(0, 0, -1));
                normals.Add(new Vector3(0, 0, -1));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            // Block Y+ face
            void AddTopFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(1 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(1 + x, 1 + y, 1 + z));

                uint lightVal = y == 255 ? 0xFFFF : lightmap[x + WIDTH, y + 1, z + WIDTH];
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.Top.UV1.X, workingBlock.Top.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Top.UV1.Width, workingBlock.Top.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Top.UV1.Width, workingBlock.Top.UV1.Height));
                uvs.Add(new Vector2(workingBlock.Top.UV1.X, workingBlock.Top.UV1.Height));

                uv2.Add(new Vector2(workingBlock.Top.UV2.X, workingBlock.Top.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Top.UV2.Width, workingBlock.Top.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Top.UV2.Width, workingBlock.Top.UV2.Height));
                uv2.Add(new Vector2(workingBlock.Top.UV2.X, workingBlock.Top.UV2.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            // Block Y- face
            void AddBottomFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(1 + x, 0 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 0 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 0 + z));

                int lx = x + WIDTH;
                int lz = z + WIDTH;
                int ly = y;

                uint lightVal = y == 0 ? 0xFFFF : lightmap[x + WIDTH, y - 1, z + WIDTH];
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.Bottom.UV1.X, workingBlock.Bottom.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Bottom.UV1.Width, workingBlock.Bottom.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Bottom.UV1.Width, workingBlock.Bottom.UV1.Height));
                uvs.Add(new Vector2(workingBlock.Bottom.UV1.X, workingBlock.Bottom.UV1.Height));

                uv2.Add(new Vector2(workingBlock.Bottom.UV2.X, workingBlock.Bottom.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Bottom.UV2.Width, workingBlock.Bottom.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Bottom.UV2.Width, workingBlock.Bottom.UV2.Height));
                uv2.Add(new Vector2(workingBlock.Bottom.UV2.X, workingBlock.Bottom.UV2.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));
                normals.Add(new Vector3(0, -1, 0));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            // Block X+ face
            void AddRightFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(1 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(1 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 1 + z));
                vertices.Add(new Vector3(1 + x, 0 + y, 0 + z));

                int lx = x + WIDTH;
                int lz = z + WIDTH;
                int ly = y;

                uint lightVal = lightmap[x + WIDTH + 1, y, z + WIDTH];
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.Right.UV1.X, workingBlock.Right.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Right.UV1.Width, workingBlock.Right.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Right.UV1.Width, workingBlock.Right.UV1.Height));
                uvs.Add(new Vector2(workingBlock.Right.UV1.X, workingBlock.Right.UV1.Height));

                uv2.Add(new Vector2(workingBlock.Right.UV2.X, workingBlock.Right.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Right.UV2.Width, workingBlock.Right.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Right.UV2.Width, workingBlock.Right.UV2.Height));
                uv2.Add(new Vector2(workingBlock.Right.UV2.X, workingBlock.Right.UV2.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(1, 0, 0));
                normals.Add(new Vector3(1, 0, 0));
                normals.Add(new Vector3(1, 0, 0));
                normals.Add(new Vector3(1, 0, 0));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            // Block X- face
            void AddLeftFace(int x, int y, int z)
            {
                vertices.Add(new Vector3(0 + x, 1 + y, 1 + z));
                vertices.Add(new Vector3(0 + x, 1 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 0 + z));
                vertices.Add(new Vector3(0 + x, 0 + y, 1 + z));

                uint lightVal = lightmap[x + WIDTH - 1, y, z + WIDTH];
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);
                light.Add(lightVal);

                uvs.Add(new Vector2(workingBlock.Left.UV1.X, workingBlock.Left.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Left.UV1.Width, workingBlock.Left.UV1.Y));
                uvs.Add(new Vector2(workingBlock.Left.UV1.Width, workingBlock.Left.UV1.Height));
                uvs.Add(new Vector2(workingBlock.Left.UV1.X, workingBlock.Left.UV1.Height));

                uv2.Add(new Vector2(workingBlock.Left.UV2.X, workingBlock.Left.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Left.UV2.Width, workingBlock.Left.UV2.Y));
                uv2.Add(new Vector2(workingBlock.Left.UV2.Width, workingBlock.Left.UV2.Height));
                uv2.Add(new Vector2(workingBlock.Left.UV2.X, workingBlock.Left.UV2.Height));

                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());
                col.Add(workingBlock.BlockColor((int)(Position.X * WIDTH) + x, (int)(Position.X * WIDTH) + y, (int)(Position.X * WIDTH) + z).ToVector4());

                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));
                normals.Add(new Vector3(-1, 0, 0));

                indices.Add(indexCount);
                indices.Add(indexCount + 1);
                indices.Add(indexCount + 2);

                indices.Add(indexCount + 2);
                indices.Add(indexCount + 3);
                indices.Add(indexCount);

                indexCount += 4;
            }

            // Water Z+ face
            void AddFrontFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(1 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 1 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 1 + z));

                uvsWater.Add(new Vector2(workingBlock.Front.UV1.X, workingBlock.Front.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Front.UV1.Width, workingBlock.Front.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Front.UV1.Width, workingBlock.Front.UV1.Height));
                uvsWater.Add(new Vector2(workingBlock.Front.UV1.X, workingBlock.Front.UV1.Height));

                normalsWater.Add(new Vector3(0, 0, 1));
                normalsWater.Add(new Vector3(0, 0, 1));
                normalsWater.Add(new Vector3(0, 0, 1));
                normalsWater.Add(new Vector3(0, 0, 1));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            // Water Z- face
            void AddBackFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(0 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(1 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 0 + z));

                uvsWater.Add(new Vector2(workingBlock.Back.UV1.X, workingBlock.Back.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Back.UV1.Width, workingBlock.Back.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Back.UV1.Width, workingBlock.Back.UV1.Height));
                uvsWater.Add(new Vector2(workingBlock.Back.UV1.X, workingBlock.Back.UV1.Height));

                normalsWater.Add(new Vector3(0, 0, -1));
                normalsWater.Add(new Vector3(0, 0, -1));
                normalsWater.Add(new Vector3(0, 0, -1));
                normalsWater.Add(new Vector3(0, 0, -1));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            // Water Y+ face
            void AddTopFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(1 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(1 + x, 1 + y, 1 + z));

                uvsWater.Add(new Vector2(workingBlock.Top.UV1.X, workingBlock.Top.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Top.UV1.Width, workingBlock.Top.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Top.UV1.Width, workingBlock.Top.UV1.Height));
                uvsWater.Add(new Vector2(workingBlock.Top.UV1.X, workingBlock.Top.UV1.Height));

                normalsWater.Add(new Vector3(0, 1, 0));
                normalsWater.Add(new Vector3(0, 1, 0));
                normalsWater.Add(new Vector3(0, 1, 0));
                normalsWater.Add(new Vector3(0, 1, 0));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            // Water Y- face
            void AddBottomFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(1 + x, 0 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 0 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 0 + z));

                uvsWater.Add(new Vector2(workingBlock.Bottom.UV1.X, workingBlock.Bottom.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Bottom.UV1.Width, workingBlock.Bottom.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Bottom.UV1.Width, workingBlock.Bottom.UV1.Height));
                uvsWater.Add(new Vector2(workingBlock.Bottom.UV1.X, workingBlock.Bottom.UV1.Height));

                normalsWater.Add(new Vector3(0, -1, 0));
                normalsWater.Add(new Vector3(0, -1, 0));
                normalsWater.Add(new Vector3(0, -1, 0));
                normalsWater.Add(new Vector3(0, -1, 0));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            // Water X+ face
            void AddRightFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(1 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(1 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 1 + z));
                verticesWater.Add(new Vector3(1 + x, 0 + y, 0 + z));

                uvsWater.Add(new Vector2(workingBlock.Right.UV1.X, workingBlock.Right.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Right.UV1.Width, workingBlock.Right.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Right.UV1.Width, workingBlock.Right.UV1.Height));
                uvsWater.Add(new Vector2(workingBlock.Right.UV1.X, workingBlock.Right.UV1.Height));

                normalsWater.Add(new Vector3(1, 0, 0));
                normalsWater.Add(new Vector3(1, 0, 0));
                normalsWater.Add(new Vector3(1, 0, 0));
                normalsWater.Add(new Vector3(1, 0, 0));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            // Water X- face
            void AddLeftFaceWater(int x, int y, int z)
            {
                verticesWater.Add(new Vector3(0 + x, 1 + y, 1 + z));
                verticesWater.Add(new Vector3(0 + x, 1 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 0 + z));
                verticesWater.Add(new Vector3(0 + x, 0 + y, 1 + z));

                uvsWater.Add(new Vector2(workingBlock.Left.UV1.X, workingBlock.Left.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Left.UV1.Width, workingBlock.Left.UV1.Y));
                uvsWater.Add(new Vector2(workingBlock.Left.UV1.Width, workingBlock.Left.UV1.Height));
                uvsWater.Add(new Vector2(workingBlock.Left.UV1.X, workingBlock.Left.UV1.Height));

                normalsWater.Add(new Vector3(-1, 0, 0));
                normalsWater.Add(new Vector3(-1, 0, 0));
                normalsWater.Add(new Vector3(-1, 0, 0));
                normalsWater.Add(new Vector3(-1, 0, 0));

                indicesWater.Add(indexCountWater);
                indicesWater.Add(indexCountWater + 1);
                indicesWater.Add(indexCountWater + 2);

                indicesWater.Add(indexCountWater + 2);
                indicesWater.Add(indexCountWater + 3);
                indicesWater.Add(indexCountWater);

                indexCountWater += 4;
            }

            // Prepare block mesh container for upload in main thread

            blockContainer = new VertexBlockContainer(vertices.ToArray(), uvs.ToArray(), normals.ToArray(), uv2.ToArray(), col.ToArray(), light.ToArray());
            vertices.Clear();
            uvs.Clear();
            normals.Clear();
            uv2.Clear();
            col.Clear();
            light.Clear();

            // Prepare water mesh container for upload in main thread
            waterContainer = new VertexNormalContainer(verticesWater.ToArray(), uvsWater.ToArray(), normalsWater.ToArray());
            verticesWater.Clear();
            uvsWater.Clear();
            normalsWater.Clear();

            // Store block & water mesh indices
            this.indices = indices.ToArray();
            this.indicesWater = indicesWater.ToArray();
            indices.Clear();
            indicesWater.Clear();

            // Mark for rebuild
            shouldRebuildWaterMesh = true;
            shouldRebuildMesh = true;
        }


        /// <summary>
        /// Checks whether the working blocks face facing to given coordinates should be drawn
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <param name="z">Z</param>
        /// <param name="workingBlockID">The block id we're checking</param>
        /// <returns>True if the block face should be drawn, otherwise false</returns>
        public bool ShouldDrawBlockFacing(int x, int y, int z, int workingBlockID)
        {
            short block = GetBlockID(x, y, z);

            // Check if the block is air or not present
            if (block == 0 || block == -1)
                return true;

            // Get block metadata
            var theBlock = BlockDatabase.GetBlock(block);

            // If block is transparent do additional checks
            if (theBlock.IsTransparent)
            {
                //NOTE: Remove above return for faster trees (possible setting)
                if (block != workingBlockID) // Different transparent blocks next to each other
                    return true;

                // Same block - check for self culling
                if (theBlock.TransparencyCullsSelf)
                    return false;

                return true;
            }

            // When facing opaque block never draw
            return false;
        }
    }
}
