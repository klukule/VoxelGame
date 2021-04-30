using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoxelGame.Blocks;

namespace VoxelGame.Worlds
{
    /// <summary>
    /// Lightmap calculation and propagation functions
    /// </summary>
    public partial class Chunk
    {
        // Cached lightmap
        private ushort[,,] _lightmap = new ushort[WIDTH * 3, HEIGHT, WIDTH * 3];

        /// <summary>
        /// Update lighting
        /// </summary>
        public void GenerateLight()
        {
            var w = WIDTH * 3;
            ushort[,,] lightmap = new ushort[w, HEIGHT, w];         // Lightmap for 3x3 chunks to propagate light across chunks... TODO: Use checked lightmap from other chunks instead?
            Stack<Vector3> stack = new Stack<Vector3>();            // List of blocks to evaluate light propagation from

            //////////////////////////////////////////
            // Seed the lightmap - sun & light sources
            //////////////////////////////////////////
            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < w; z++)
                {

                    if ((x % 47) * (z % 47) == 0)                   // Create bright boundary around the 3x3 chunk map to stop light propagation further
                    {
                        for (int yy = 0; yy < HEIGHT; yy++)
                            lightmap[x, yy, z] = lightmap[x, yy, z].SetSun(15);
                        continue;
                    }

                    int worldX = x - WIDTH;
                    int worldZ = z - WIDTH;
                    int height = Math.Max(0, GetHeightAtBlock(worldX, worldZ));
                    for (int y = height; y < HEIGHT; y++)           // Above maximum block everything is sun
                        lightmap[x, y, z] = lightmap[x, y, z].SetSun(15);

                    for (int y = 0; y < HEIGHT; y++)                // Seed light sources
                    {
                        var blockId = GetBlockID(worldX, y, worldZ);
                        if (blockId > 0)
                        {
                            var block = BlockDatabase.GetBlock(blockId);
                            if (block.IsEmissive)
                            {
                                lightmap[x, y, z] = lightmap[x, y, z].SetRed((byte)(block.Emission.R * 15)).SetGreen((byte)(block.Emission.G * 15)).SetBlue((byte)(block.Emission.B * 15));
                                stack.Push(new Vector3(x, y, z));
                            }
                        }
                    }

                    // Get maximum height around current block 
                    /*if (x < w - 2) height = Math.Max(height, GetHeightAtBlock(worldX + 1, worldZ));
                    if (x > 1) height = Math.Max(height, GetHeightAtBlock(worldX - 1, worldZ));
                    if (z < w - 2) height = Math.Max(height, GetHeightAtBlock(worldX, worldZ + 1));
                    if (z > 1) height = Math.Max(height, GetHeightAtBlock(worldX, worldZ - 1));

                    // Clamp height
                    height = Math.Min(height + 1, HEIGHT - 1);
                    if (height < 2) continue;*/

                    // Lighting seed
                    stack.Push(new Vector3(x, height, z));
                }
            }

            //////////////////////////////////////////
            // Propagate
            //////////////////////////////////////////

            while (stack.Count > 0)
            {
                var position = stack.Pop();

                int x = (int)position.X;                // Block X position (in 3x3 chunk local space)
                int y = (int)position.Y;                // Block Y position (in 3x3 chunk local space)
                int z = (int)position.Z;                // Block Z position (in 3x3 chunk local space)
                int worldX = x - WIDTH;                 // Block X position relative to current chunk
                int worldZ = z - WIDTH;                 // Block Z position relative to current chunk

                ushort lightVal = lightmap[x, y, z];    // Current total light value
                byte sVal = lightVal.GetSun();
                byte rVal = lightVal.GetRed();
                byte gVal = lightVal.GetGreen();
                byte bVal = lightVal.GetBlue();

                ///////////////////////////////////////////////////////////////////////////////////////
                // Propagate along X+
                if (x < w - 1)
                {
                    int adjBlockId = GetBlockID(worldX + 1, y, worldZ);
                    sbyte adjBlockOpacity = adjBlockId > 0 ? BlockDatabase.GetBlock(adjBlockId).Opacity : 15;
                    ushort adjLightValue = lightmap[x + 1, y, z];
                    byte adjsVal = adjLightValue.GetSun();
                    byte adjrVal = adjLightValue.GetRed();
                    byte adjgVal = adjLightValue.GetGreen();
                    byte adjbVal = adjLightValue.GetBlue();

                    bool enqueue = false;
                    if (adjsVal < sVal - 1)             // Propagate sun
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x + 1, y, z] = lightmap[x + 1, y, z].SetSun((byte)(sVal - 1));
                            enqueue = true;
                        }
                    }

                    if (adjrVal < rVal - 1)             // Propagate red emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x + 1, y, z] = lightmap[x + 1, y, z].SetRed((byte)(rVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(rVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x + 1, y, z] = lightmap[x + 1, y, z].SetRed((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjgVal < gVal - 1)             // Propagate green emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x + 1, y, z] = lightmap[x + 1, y, z].SetGreen((byte)(gVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(gVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x + 1, y, z] = lightmap[x + 1, y, z].SetGreen((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjbVal < bVal - 1)             // Propagate blue emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x + 1, y, z] = lightmap[x + 1, y, z].SetBlue((byte)(bVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(bVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x + 1, y, z] = lightmap[x + 1, y, z].SetBlue((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (enqueue)
                        stack.Push(new Vector3(x + 1, y, z));
                }
                ///////////////////////////////////////////////////////////////////////////////////////
                // Propagate along X-
                if (x > 0)
                {
                    int adjBlockId = GetBlockID(worldX - 1, y, worldZ);
                    sbyte adjBlockOpacity = adjBlockId > 0 ? BlockDatabase.GetBlock(adjBlockId).Opacity : 15;
                    ushort adjLightValue = lightmap[x - 1, y, z];
                    byte adjsVal = adjLightValue.GetSun();
                    byte adjrVal = adjLightValue.GetRed();
                    byte adjgVal = adjLightValue.GetGreen();
                    byte adjbVal = adjLightValue.GetBlue();

                    bool enqueue = false;
                    if (adjsVal < sVal - 1)             // Propagate sun
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x - 1, y, z] = lightmap[x - 1, y, z].SetSun((byte)(sVal - 1));
                            enqueue = true;
                        }
                    }

                    if (adjrVal < rVal - 1)             // Propagate red emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x - 1, y, z] = lightmap[x - 1, y, z].SetRed((byte)(rVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(rVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x - 1, y, z] = lightmap[x - 1, y, z].SetRed((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjgVal < gVal - 1)             // Propagate green emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x - 1, y, z] = lightmap[x - 1, y, z].SetGreen((byte)(gVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(gVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x - 1, y, z] = lightmap[x - 1, y, z].SetGreen((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjbVal < bVal - 1)             // Propagate blue emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x - 1, y, z] = lightmap[x - 1, y, z].SetBlue((byte)(bVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(bVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x - 1, y, z] = lightmap[x - 1, y, z].SetBlue((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (enqueue)
                        stack.Push(new Vector3(x - 1, y, z));
                }
                ///////////////////////////////////////////////////////////////////////////////////////
                // Propagate along Z+
                if (z < w - 1)
                {
                    int adjBlockId = GetBlockID(worldX, y, worldZ + 1);
                    sbyte adjBlockOpacity = adjBlockId > 0 ? BlockDatabase.GetBlock(adjBlockId).Opacity : 15;
                    ushort adjLightValue = lightmap[x, y, z + 1];
                    byte adjsVal = adjLightValue.GetSun();
                    byte adjrVal = adjLightValue.GetRed();
                    byte adjgVal = adjLightValue.GetGreen();
                    byte adjbVal = adjLightValue.GetBlue();

                    bool enqueue = false;
                    if (adjsVal < sVal - 1)             // Propagate sun
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y, z + 1] = lightmap[x, y, z + 1].SetSun((byte)(sVal - 1));
                            enqueue = true;
                        }
                    }

                    if (adjrVal < rVal - 1)             // Propagate red emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y, z + 1] = lightmap[x, y, z + 1].SetRed((byte)(rVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(rVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y, z + 1] = lightmap[x, y, z + 1].SetRed((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjgVal < gVal - 1)             // Propagate green emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y, z + 1] = lightmap[x, y, z + 1].SetGreen((byte)(gVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(gVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y, z + 1] = lightmap[x, y, z + 1].SetGreen((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjbVal < bVal - 1)             // Propagate blue emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y, z + 1] = lightmap[x, y, z + 1].SetBlue((byte)(bVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(bVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y, z + 1] = lightmap[x, y, z + 1].SetBlue((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (enqueue)
                        stack.Push(new Vector3(x, y, z + 1));
                }
                ///////////////////////////////////////////////////////////////////////////////////////
                // Propagate along Z-
                if (z > 0)
                {
                    int adjBlockId = GetBlockID(worldX, y, worldZ - 1);
                    sbyte adjBlockOpacity = adjBlockId > 0 ? BlockDatabase.GetBlock(adjBlockId).Opacity : 15;
                    ushort adjLightValue = lightmap[x, y, z - 1];
                    byte adjsVal = adjLightValue.GetSun();
                    byte adjrVal = adjLightValue.GetRed();
                    byte adjgVal = adjLightValue.GetGreen();
                    byte adjbVal = adjLightValue.GetBlue();

                    bool enqueue = false;
                    if (adjsVal < sVal - 1)             // Propagate sun
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y, z - 1] = lightmap[x, y, z - 1].SetSun((byte)(sVal - 1));
                            enqueue = true;
                        }
                    }

                    if (adjrVal < rVal - 1)             // Propagate red emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y, z - 1] = lightmap[x, y, z - 1].SetRed((byte)(rVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(rVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y, z - 1] = lightmap[x, y, z - 1].SetRed((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjgVal < gVal - 1)             // Propagate green emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y, z - 1] = lightmap[x, y, z - 1].SetGreen((byte)(gVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(gVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y, z - 1] = lightmap[x, y, z - 1].SetGreen((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjbVal < bVal - 1)             // Propagate blue emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y, z - 1] = lightmap[x, y, z - 1].SetBlue((byte)(bVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(bVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y, z - 1] = lightmap[x, y, z - 1].SetBlue((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (enqueue)
                        stack.Push(new Vector3(x, y, z - 1));
                }
                ///////////////////////////////////////////////////////////////////////////////////////
                // Propagate along Y+
                if (y < HEIGHT - 1)
                {
                    int adjBlockId = GetBlockID(worldX, y + 1, worldZ);
                    sbyte adjBlockOpacity = adjBlockId > 0 ? BlockDatabase.GetBlock(adjBlockId).Opacity : 15;
                    ushort adjLightValue = lightmap[x, y + 1, z];
                    byte adjsVal = adjLightValue.GetSun();
                    byte adjrVal = adjLightValue.GetRed();
                    byte adjgVal = adjLightValue.GetGreen();
                    byte adjbVal = adjLightValue.GetBlue();

                    bool enqueue = false;
                    if (adjsVal < sVal - 1)             // Propagate sun
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y + 1, z] = lightmap[x, y + 1, z].SetSun((byte)(sVal - 1));
                            enqueue = true;
                        }
                    }

                    if (adjrVal < rVal - 1)             // Propagate red emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y + 1, z] = lightmap[x, y + 1, z].SetRed((byte)(rVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(rVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y + 1, z] = lightmap[x, y + 1, z].SetRed((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjgVal < gVal - 1)             // Propagate green emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y + 1, z] = lightmap[x, y + 1, z].SetGreen((byte)(gVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(gVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y + 1, z] = lightmap[x, y + 1, z].SetGreen((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjbVal < bVal - 1)             // Propagate blue emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y + 1, z] = lightmap[x, y + 1, z].SetBlue((byte)(bVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(bVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y + 1, z] = lightmap[x, y + 1, z].SetBlue((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (enqueue)
                        stack.Push(new Vector3(x, y + 1, z));
                }
                ///////////////////////////////////////////////////////////////////////////////////////
                // Propagate along Y-
                if (y > 0)
                {
                    int adjBlockId = GetBlockID(worldX, y - 1, worldZ);
                    sbyte adjBlockOpacity = adjBlockId > 0 ? BlockDatabase.GetBlock(adjBlockId).Opacity : 15;
                    ushort adjLightValue = lightmap[x, y - 1, z];
                    byte adjsVal = adjLightValue.GetSun();
                    byte adjrVal = adjLightValue.GetRed();
                    byte adjgVal = adjLightValue.GetGreen();
                    byte adjbVal = adjLightValue.GetBlue();

                    bool enqueue = false;
                    if (adjsVal < sVal - 1)             // Propagate sun
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            if (sVal == 15)
                            {
                                lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetSun(sVal);
                                enqueue = true;
                            }
                            else if (adjsVal < sVal - 1)
                            {

                                lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetSun((byte)(sVal - 1));
                                enqueue = true;
                            }
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(sVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetSun((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjrVal < rVal - 1)             // Propagate red emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetRed((byte)(rVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(rVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetRed((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjgVal < gVal - 1)             // Propagate green emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetGreen((byte)(gVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(gVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetGreen((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (adjbVal < bVal - 1)             // Propagate blue emissive
                    {
                        if (adjBlockId == 0)            // If air - propagate
                        {
                            lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetBlue((byte)(bVal - 1));
                            enqueue = true;
                        }
                        else if (adjBlockId != -1)
                        {
                            if (adjBlockOpacity < 15)    // Propagate through transparent
                            {
                                // Calculate amount of light comming through the block
                                sbyte val = (sbyte)(bVal - adjBlockOpacity);
                                val = Math.Max(val, (sbyte)0);
                                lightmap[x, y - 1, z] = lightmap[x, y - 1, z].SetBlue((byte)val);
                                enqueue = true;
                            }
                        }
                    }

                    if (enqueue)
                        stack.Push(new Vector3(x, y - 1, z));
                }
                ///////////////////////////////////////////////////////////////////////////////////////
            }

            //////////////////////////////////////////
            // Swap
            //////////////////////////////////////////
            _lightmap = lightmap;
        }

    }

    // TODO: Move to separate file
    internal static class UshortExtensions
    {
        // 16 bit
        // SSSS RRRR GGGG BBBB
        public static ushort SetSun(this ushort val, byte raw) => (ushort)((val & 0x0FFF) | ((raw & 0xF) << 12));
        public static ushort SetRed(this ushort val, byte raw) => (ushort)((val & 0xF0FF) | ((raw & 0xF) << 8));
        public static ushort SetGreen(this ushort val, byte raw) => (ushort)((val & 0xFF0F) | ((raw & 0xF) << 4));
        public static ushort SetBlue(this ushort val, byte raw) => (ushort)((val & 0xFFF0) | (raw & 0xF));

        public static byte GetSun(this ushort val) => (byte)((val >> 12) & 0xF);
        public static byte GetRed(this ushort val) => (byte)((val >> 8) & 0xF);
        public static byte GetGreen(this ushort val) => (byte)((val >> 4) & 0xF);
        public static byte GetBlue(this ushort val) => (byte)(val & 0xF);
    }
}
