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
            List<float> light = new List<float>();

            List<Vector3> verticesWater = new List<Vector3>();
            List<Vector2> uvsWater = new List<Vector2>();
            List<Vector3> normalsWater = new List<Vector3>();

            List<uint> indices = new List<uint>();
            List<uint> indicesWater = new List<uint>();

            uint indexCount = 0;
            uint indexCountWater = 0;

            Block workingBlock = null;

            List<Vector3> toPropagate = new List<Vector3>();
            int w = WIDTH * 3;
            byte[,,] lightmap = new byte[w, HEIGHT, w];

            /////////////////////////////////////////
            /// LIGHT PROPAGATION
            /////////////////////////////////////////

            // Calculate light propagation seeds in 3x3 chunk range (to propagate light across chunk boundaries
            for (int x = 0; x < w; ++x)
            {
                for (int z = 0; z < w; ++z)
                {
                    if ((x % 47) * (z % 47) == 0) //filters outer edges
                    {
                        //Debug.Log($"these should at least 0 or 47  ->  {x} {z}"); 
                        for (int yy = 0; yy < HEIGHT; yy++) //dont do outer edges
                        {
                            lightmap[x, yy, z] = 15; //set all edges to 15 to stop tracing at edges
                        }
                        continue;
                    }
                    int worldX = x - WIDTH;
                    int worldZ = z - WIDTH;
                    int height = Math.Max(0, GetHeightAtBlock(worldX, worldZ));

                    // Set air to max brightness
                    for (int y = height; y < HEIGHT; y++)
                    {
                        //Do manually here!
                        lightmap[x, y, z] = 15; //set all edges to 15 to stop tracing at edges
                    }

                    // Get maximum height with neighbors
                    if (x < w - 2) height = Math.Max(height, GetHeightAtBlock(worldX + 1, worldZ));
                    if (x > 1) height = Math.Max(height, GetHeightAtBlock(worldX - 1, worldZ));
                    if (z < w - 2) height = Math.Max(height, GetHeightAtBlock(worldX, worldZ + 1));
                    if (z > 1) height = Math.Max(height, GetHeightAtBlock(worldX, worldZ - 1));

                    // Get minimum fully lit height
                    height = Math.Min(height + 1, HEIGHT - 1);
                    if (height < 2) continue; // Skip seeding below certain height

                    // Lighting seed
                    toPropagate.Add(new Vector3(x, height, z));
                }
            }

            // Propagate each seed
            while (toPropagate.Count > 0)
            {
                Vector3 position = toPropagate.Last();
                toPropagate.RemoveAt(toPropagate.Count - 1);
                int x = (int)position.X;
                int y = (int)position.Y;
                int z = (int)position.Z;
                int worldX = x - WIDTH;
                int worldZ = z - WIDTH;

                byte lightVal = lightmap[x, y, z];
                byte adjLightVal = 0;
                short adjBlockId = 0;

                // Propagate to X+ (Right)
                if (x < w - 1)
                {
                    adjLightVal = lightmap[x + 1, y, z];
                    if (adjLightVal < lightVal - 1) // If new light value is more than current
                    {
                        adjBlockId = GetBlockID(worldX + 1, y, worldZ);
                        if (adjBlockId == 0) // If block is air, propagate and set as seed
                        {
                            lightmap[x + 1, y, z] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x + 1, y, z));
                        }
                    }
                }

                // Propagate to X- (Left)
                if (x > 0)
                {
                    adjLightVal = lightmap[x - 1, y, z];
                    if (adjLightVal < lightVal - 1) // If new light value is more than current
                    {
                        adjBlockId = GetBlockID(worldX - 1, y, worldZ);
                        if (adjBlockId == 0) // If block is air, propagate and set as seed
                        {
                            lightmap[x - 1, y, z] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x - 1, y, z));
                        }
                    }
                }

                // Propagate to Y- (Down)
                if (y > 0)
                {
                    adjBlockId = GetBlockID(worldX, y - 1, worldZ);
                    if (adjBlockId == 0) // If Air
                    {
                        if (lightVal == 15) // If maximum light propagate further
                        {
                            lightmap[x, y - 1, z] = (byte)(lightVal);
                            toPropagate.Add(new Vector3(x, y - 1, z));
                        }
                        else // Else propagate only if new vlaue is bigger
                        {
                            adjLightVal = lightmap[x, y - 1, z];
                            if (adjLightVal < lightVal - 1)
                            {
                                lightmap[x, y - 1, z] = (byte)(lightVal - 1);
                                toPropagate.Add(new Vector3(x, y - 1, z));
                            }
                        }
                    }
                    else if (adjBlockId != -1) // Else if any non-air and existing block
                    {
                        sbyte op = BlockDatabase.GetBlock(adjBlockId).Opacity; // Get opacity
                        if (op < 15) // If (semi-)transparent block
                        {
                            // Propagate light value lowered by the about of transparency and seed
                            op = (sbyte)(lightVal - op);
                            op = Math.Max(op, (sbyte)0);
                            lightmap[x, y - 1, z] = (byte)op;
                            toPropagate.Add(new Vector3(x, y - 1, z));
                        }
                    }
                }

                // Propagate to Y+ (Up)
                if (y < HEIGHT - 1)
                {
                    adjLightVal = lightmap[x, y + 1, z];
                    if (adjLightVal < lightVal - 1) // If new light value is more than current
                    {
                        adjBlockId = GetBlockID(worldX, y + 1, worldZ);
                        if (adjBlockId == 0) // If block is air, propagate and set as seed
                        {
                            lightmap[x, y + 1, z] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x, y + 1, z));
                        }
                    }
                }

                // Propagate Z+ (Front)
                if (z < w - 1)
                {
                    adjLightVal = lightmap[x, y, z + 1];
                    if (adjLightVal < lightVal - 1) // If new light value is more than current
                    {
                        adjBlockId = GetBlockID(worldX, y, worldZ + 1);
                        if (adjBlockId == 0) // If block is air, propagate and set as seed
                        {
                            lightmap[x, y, z + 1] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x, y, z + 1));
                        }
                    }
                }

                // Propagate Z- (Back)
                if (z > 0)
                {
                    adjLightVal = lightmap[x, y, z - 1];
                    if (adjLightVal < lightVal - 1) // If new light value is more than current
                    {
                        adjBlockId = GetBlockID(worldX, y, worldZ - 1);
                        if (adjBlockId == 0)  // If block is air, propagate and set as seed
                        {
                            lightmap[x, y, z - 1] = (byte)(lightVal - 1);
                            toPropagate.Add(new Vector3(x, y, z - 1));
                        }
                    }
                }
            }


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

                int lx = x + WIDTH;
                int lz = z + WIDTH;
                int ly = y;

                byte lightR = lightmap[lx + 1, ly, lz];
                byte lightL = lightmap[lx - 1, ly, lz];
                byte lightF = lightmap[lx, ly, lz + 1];
                byte lightB = lightmap[lx, ly, lz - 1];
                byte lightU = (ly == 255 ? (byte)15 : lightmap[lx, ly + 1, lz]);
                byte lightD = (ly == 0 ? (byte)15 : lightmap[lx, ly - 1, lz]);
                int b = (ly == 0 ? 0 : 1);
                int t = (ly == 255 ? 0 : 1);
                byte br = (byte)((lightmap[lx, ly, lz + 1] + lightmap[lx - 1, ly, lz + 1] + lightmap[lx, ly - b, lz + 1] + lightmap[lx - 1, ly - b, lz + 1]) / 4);
                byte tr = (byte)((lightmap[lx, ly, lz + 1] + lightmap[lx - 1, ly, lz + 1] + lightmap[lx, ly + t, lz + 1] + lightmap[lx - 1, ly + t, lz + 1]) / 4);
                byte tl = (byte)((lightmap[lx, ly, lz + 1] + lightmap[lx + 1, ly, lz + 1] + lightmap[lx, ly + t, lz + 1] + lightmap[lx + 1, ly + t, lz + 1]) / 4);
                byte bl = (byte)((lightmap[lx, ly, lz + 1] + lightmap[lx + 1, ly, lz + 1] + lightmap[lx, ly - b, lz + 1] + lightmap[lx + 1, ly - b, lz + 1]) / 4);

                //float lightVal = lightmap[x + WIDTH, y, z + WIDTH + 1];//GetBlockLight(x, y, z + 1);
                light.Add(tl);
                light.Add(tr);
                light.Add(br);
                light.Add(bl);

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

                int lx = x + WIDTH;
                int lz = z + WIDTH;
                int ly = y;

                byte lightR = lightmap[lx + 1, ly, lz];
                byte lightL = lightmap[lx - 1, ly, lz];
                byte lightF = lightmap[lx, ly, lz + 1];
                byte lightB = lightmap[lx, ly, lz - 1];
                byte lightU = (ly == 255 ? (byte)15 : lightmap[lx, ly + 1, lz]);
                byte lightD = (ly == 0 ? (byte)15 : lightmap[lx, ly - 1, lz]);
                int b = (ly == 0 ? 0 : 1);
                int t = (ly == 255 ? 0 : 1);
                byte bl = (byte)((lightmap[lx, ly, lz - 1] + lightmap[lx - 1, ly, lz - 1] + lightmap[lx, ly - b, lz - 1] + lightmap[lx - 1, ly - b, lz - 1]) / 4);
                byte tl = (byte)((lightmap[lx, ly, lz - 1] + lightmap[lx - 1, ly, lz - 1] + lightmap[lx, ly + t, lz - 1] + lightmap[lx - 1, ly + t, lz - 1]) / 4);
                byte tr = (byte)((lightmap[lx, ly, lz - 1] + lightmap[lx + 1, ly, lz - 1] + lightmap[lx, ly + t, lz - 1] + lightmap[lx + 1, ly + t, lz - 1]) / 4);
                byte br = (byte)((lightmap[lx, ly, lz - 1] + lightmap[lx + 1, ly, lz - 1] + lightmap[lx, ly - b, lz - 1] + lightmap[lx + 1, ly - b, lz - 1]) / 4);

                //float lightVal = lightmap[x + WIDTH, y, z + WIDTH + 1];//GetBlockLight(x, y, z + 1);
                light.Add(tl);
                light.Add(tr);
                light.Add(br);
                light.Add(bl);

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

                int lx = x + WIDTH;
                int lz = z + WIDTH;
                int ly = y;

                byte lightR = lightmap[lx + 1, ly, lz];
                byte lightL = lightmap[lx - 1, ly, lz];
                byte lightF = lightmap[lx, ly, lz + 1];
                byte lightB = lightmap[lx, ly, lz - 1];
                byte lightU = (ly == 255 ? (byte)15 : lightmap[lx, ly + 1, lz]);
                byte lightD = (ly == 0 ? (byte)15 : lightmap[lx, ly - 1, lz]);
                int b = (ly == 0 ? 0 : 1);
                int t = (ly == 255 ? 0 : 1);
                byte bl = (byte)((lightmap[lx, ly + t, lz] + lightmap[lx - 1, ly + t, lz] + lightmap[lx, ly + t, lz - 1] + lightmap[lx - 1, ly + t, lz - 1]) / 4);
                byte tl = (byte)((lightmap[lx, ly + t, lz] + lightmap[lx - 1, ly + t, lz] + lightmap[lx, ly + t, lz + 1] + lightmap[lx - 1, ly + t, lz + 1]) / 4);
                byte tr = (byte)((lightmap[lx, ly + t, lz] + lightmap[lx + 1, ly + t, lz] + lightmap[lx, ly + t, lz + 1] + lightmap[lx + 1, ly + t, lz + 1]) / 4);
                byte br = (byte)((lightmap[lx, ly + t, lz] + lightmap[lx + 1, ly + t, lz] + lightmap[lx, ly + t, lz - 1] + lightmap[lx + 1, ly + t, lz - 1]) / 4);

                //float lightVal = lightmap[x + WIDTH, y, z + WIDTH + 1];//GetBlockLight(x, y, z + 1);
                light.Add(br);
                light.Add(bl);
                light.Add(tl);
                light.Add(tr);

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

                byte lightR = lightmap[lx + 1, ly, lz];
                byte lightL = lightmap[lx - 1, ly, lz];
                byte lightF = lightmap[lx, ly, lz + 1];
                byte lightB = lightmap[lx, ly, lz - 1];
                byte lightU = (ly == 255 ? (byte)15 : lightmap[lx, ly + 1, lz]);
                byte lightD = (ly == 0 ? (byte)15 : lightmap[lx, ly - 1, lz]);
                int b = (ly == 0 ? 0 : 1);
                int t = (ly == 255 ? 0 : 1);
                byte tl = (byte)((lightmap[lx, ly - b, lz] + lightmap[lx - 1, ly - b, lz] + lightmap[lx, ly - b, lz - 1] + lightmap[lx - 1, ly - b, lz - 1]) / 4);
                byte bl = (byte)((lightmap[lx, ly - b, lz] + lightmap[lx - 1, ly - b, lz] + lightmap[lx, ly - b, lz + 1] + lightmap[lx - 1, ly - b, lz + 1]) / 4);
                byte br = (byte)((lightmap[lx, ly - b, lz] + lightmap[lx + 1, ly - b, lz] + lightmap[lx, ly - b, lz + 1] + lightmap[lx + 1, ly - b, lz + 1]) / 4);
                byte tr = (byte)((lightmap[lx, ly - b, lz] + lightmap[lx + 1, ly - b, lz] + lightmap[lx, ly - b, lz - 1] + lightmap[lx + 1, ly - b, lz - 1]) / 4);

                //float lightVal = lightmap[x + WIDTH, y, z + WIDTH + 1];//GetBlockLight(x, y, z + 1);
                light.Add(br);
                light.Add(bl);
                light.Add(tl);
                light.Add(tr);

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

                byte lightR = lightmap[lx + 1, ly, lz];
                byte lightL = lightmap[lx - 1, ly, lz];
                byte lightF = lightmap[lx, ly, lz + 1];
                byte lightB = lightmap[lx, ly, lz - 1];
                byte lightU = (ly == 255 ? (byte)15 : lightmap[lx, ly + 1, lz]);
                byte lightD = (ly == 0 ? (byte)15 : lightmap[lx, ly - 1, lz]);
                int b = (ly == 0 ? 0 : 1);
                int t = (ly == 255 ? 0 : 1);
                byte bl = (byte)((lightmap[lx + 1, ly, lz] + lightmap[lx + 1, ly, lz - 1] + lightmap[lx + 1, ly - b, lz] + lightmap[lx + 1, ly - b, lz - 1]) / 4);
                byte tl = (byte)((lightmap[lx + 1, ly, lz] + lightmap[lx + 1, ly, lz - 1] + lightmap[lx + 1, ly + t, lz] + lightmap[lx + 1, ly + t, lz - 1]) / 4);
                byte tr = (byte)((lightmap[lx + 1, ly, lz] + lightmap[lx + 1, ly, lz + 1] + lightmap[lx + 1, ly + t, lz] + lightmap[lx + 1, ly + t, lz + 1]) / 4);
                byte br = (byte)((lightmap[lx + 1, ly, lz] + lightmap[lx + 1, ly, lz + 1] + lightmap[lx + 1, ly - b, lz] + lightmap[lx + 1, ly - b, lz + 1]) / 4);

                //float lightVal = lightmap[x + WIDTH, y, z + WIDTH + 1];//GetBlockLight(x, y, z + 1);
                light.Add(tl);
                light.Add(tr);
                light.Add(br);
                light.Add(bl);

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

                int lx = x + WIDTH;
                int lz = z + WIDTH;
                int ly = y;

                byte lightR = lightmap[lx + 1, ly, lz];
                byte lightL = lightmap[lx - 1, ly, lz];
                byte lightF = lightmap[lx, ly, lz + 1];
                byte lightB = lightmap[lx, ly, lz - 1];
                byte lightU = (ly == 255 ? (byte)15 : lightmap[lx, ly + 1, lz]);
                byte lightD = (ly == 0 ? (byte)15 : lightmap[lx, ly - 1, lz]);
                int b = (ly == 0 ? 0 : 1);
                int t = (ly == 255 ? 0 : 1);
                byte br = (byte)((lightmap[lx - 1, ly, lz] + lightmap[lx - 1, ly, lz - 1] + lightmap[lx - 1, ly - b, lz] + lightmap[lx - 1, ly - b, lz - 1]) / 4);
                byte tr = (byte)((lightmap[lx - 1, ly, lz] + lightmap[lx - 1, ly, lz - 1] + lightmap[lx - 1, ly + t, lz] + lightmap[lx - 1, ly + t, lz - 1]) / 4);
                byte tl = (byte)((lightmap[lx - 1, ly, lz] + lightmap[lx - 1, ly, lz + 1] + lightmap[lx - 1, ly + t, lz] + lightmap[lx - 1, ly + t, lz + 1]) / 4);
                byte bl = (byte)((lightmap[lx - 1, ly, lz] + lightmap[lx - 1, ly, lz + 1] + lightmap[lx - 1, ly - b, lz] + lightmap[lx - 1, ly - b, lz + 1]) / 4);

                //float lightVal = lightmap[x + WIDTH, y, z + WIDTH + 1];//GetBlockLight(x, y, z + 1);
                light.Add(tl);
                light.Add(tr);
                light.Add(br);
                light.Add(bl);

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
