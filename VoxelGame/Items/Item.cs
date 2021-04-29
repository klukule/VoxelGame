using OpenTK;
using System.Collections.Generic;
using VoxelGame.Assets;
using VoxelGame.Worlds;
using VoxelGame.Rendering;
using VoxelGame.Rendering.Vertex;

namespace VoxelGame.Items
{
    /// <summary>
    /// Base for any item
    /// </summary>
    public abstract class Item
    {
        /// <summary>
        /// Item display name
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Item texture location
        /// </summary>
        public virtual string IconLocation { get; set; } = "Textures/Items/Def_Item.png";

        /// <summary>
        /// Item ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Unique item key
        /// </summary>
        public abstract string Key { get; }

        /// <summary>
        /// Item icon
        /// </summary>
        public Texture Icon { get; private set; }

        /// <summary>
        /// Item mesh
        /// </summary>
        public Mesh Mesh { get; private set; }

        /// <summary>
        /// Maximum item stack
        /// </summary>
        public int MaxStackSize { get; protected set; } = 64;

        public virtual void OnInteract(Vector3 position, Chunk chunk)
        {

        }

        /// <summary>
        /// Loads or creates model for the item
        /// </summary>
        public void GenerateGraphics()
        {
            // Load icon
            Icon = AssetDatabase.GetAsset<Texture>(IconLocation);
            if (Icon != null)
            {
                // Try load mesh for the item 
                string path = $"Item/Model/{Name}.obj";
                if (AssetDatabase.ContainsAssetOfType(path, typeof(Mesh)))
                    Mesh = AssetDatabase.GetAsset<Mesh>(path);
                else // If not found, generate one
                {
                    Mesh = CreateModel();
                    if (!AssetDatabase.RegisterAsset(Mesh, path)) // If VFS registration failed
                        Mesh = null;
                }
            }
        }

        /// <summary>
        /// Generates new dynamic item mesh
        /// </summary>
        /// <returns>Generated mesh</returns>
        private Mesh CreateModel()
        {
            List<Vector3> verts = new List<Vector3>();
            List<uint> indices = new List<uint>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();

            // TODO: Fix 3D icons... the normals and sides are totally wrong
            // NOTE: Fix consist of skrit/side having separate vertices because of non-smooth normals
            int w = Icon.Width;
            int h = Icon.Height;

            float wDiff = 1f / w;
            float hDiff = 1f / h;

            //float depth = 1 / 32f; // half of the fixed 16px thickness
            float depth = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = h - 1; y >= 0; y--)
                {
                    var col = Icon.GetPixel(x, y);
                    if (col.A == 0) continue;

                    // Calculate pixel UVs
                    var uvTl = new Vector2((float)x / w, (float)(y - 1) / h);
                    var uvTr = new Vector2((float)(x + 1) / w, (float)(y - 1) / h);
                    var uvBl = new Vector2((float)x / w, (float)y / h);
                    var uvBr = new Vector2((float)(x + 1) / w, (float)y / h);

                    // FRONT FACE
                    Vector3 tl = new Vector3(x * wDiff, y * hDiff, -depth);
                    if (!verts.Contains(tl))
                    {
                        verts.Add(tl);
                        normals.Add(new Vector3(-1, 0, 0));
                        uvs.Add(uvTl);
                    }

                    uint tli = (uint)verts.IndexOf(tl);

                    Vector3 tr = new Vector3((x * wDiff) + wDiff, y * hDiff, -depth);
                    if (!verts.Contains(tr))
                    {
                        verts.Add(tr);
                        normals.Add(new Vector3(-1, 0, 0));
                        uvs.Add(uvTr);
                    }

                    uint tri = (uint)verts.IndexOf(tr);

                    Vector3 bl = new Vector3(x * wDiff, (y * hDiff) + wDiff, -depth);
                    if (!verts.Contains(bl))
                    {
                        verts.Add(bl);
                        normals.Add(new Vector3(-1, 0, 0));
                        uvs.Add(uvBl);
                    }

                    uint bli = (uint)verts.IndexOf(bl);

                    Vector3 br = new Vector3((x * wDiff) + wDiff, (y * hDiff) + wDiff, -depth);
                    if (!verts.Contains(br))
                    {
                        verts.Add(br);
                        normals.Add(new Vector3(-1, 0, 0));
                        uvs.Add(uvBr);
                    }

                    uint bri = (uint)verts.IndexOf(br);

                    indices.Add(tli);
                    indices.Add(tri);
                    indices.Add(bli);

                    indices.Add(tri);
                    indices.Add(bri);
                    indices.Add(bli);

                    // LEFT SKIRT
                    /*if ((x > 0 && Icon.GetPixel(x - 1, y).A == 1) || x == 0)
                    {
                        Vector3 tlD = new Vector3(x * wDiff, y * hDiff, depth);
                        Vector3 blD = new Vector3(x * wDiff, (y * hDiff) + hDiff, depth);

                        if (!verts.Contains(tlD))
                        {
                            verts.Add(tlD);
                            normals.Add(new Vector3(0, 0, -1));
                            uvs.Add(uvTl);
                        }

                        uint tlDi = (uint)verts.IndexOf(tlD);

                        if (!verts.Contains(blD))
                        {
                            verts.Add(blD);
                            normals.Add(new Vector3(0, 0, -1));
                            uvs.Add(uvBl);
                        }

                        uint blDi = (uint)verts.IndexOf(blD);

                        indices.Add(tlDi);
                        indices.Add(tli);
                        indices.Add(blDi);

                        indices.Add(bli);
                        indices.Add(blDi);
                        indices.Add(tli);
                    }*/

                    // RIGHT SKIRT
                    /*if ((x < w - 1 && Icon.GetPixel(x + 1, y).A == 1) || x == w - 1)
                    {
                        Vector3 trD = new Vector3((x * wDiff) + wDiff, y * hDiff, depth);
                        Vector3 brD = new Vector3((x * wDiff) + wDiff, (y * hDiff) + hDiff, depth);

                        if (!verts.Contains(trD))
                        {
                            verts.Add(trD);
                            normals.Add(new Vector3(0, 0, -1));
                            uvs.Add(uvTr);
                        }

                        uint trDi = (uint)verts.IndexOf(trD);

                        if (!verts.Contains(brD))
                        {
                            verts.Add(brD);
                            normals.Add(new Vector3(0, 0, -1));
                            uvs.Add(uvBr);
                        }

                        uint brDi = (uint)verts.IndexOf(brD);

                        indices.Add(tri);
                        indices.Add(trDi);
                        indices.Add(bri);

                        indices.Add(brDi);
                        indices.Add(bri);
                        indices.Add(trDi);

                    }*/

                    // TOP SKIRT
                    /*if ((y > 0 && Icon.GetPixel(x, y - 1).A == 1) || y == 0)
                    {
                        Vector3 tlD = new Vector3((x * wDiff), y * hDiff, depth);
                        Vector3 trD = new Vector3((x * wDiff) + wDiff, y * hDiff, depth);

                        if (!verts.Contains(trD))
                        {
                            verts.Add(trD);
                            normals.Add(new Vector3(0, -1, 0));
                            uvs.Add(uvTr);
                        }

                        uint trDi = (uint)verts.IndexOf(trD);

                        if (!verts.Contains(tlD))
                        {
                            verts.Add(tlD);
                            normals.Add(new Vector3(0, -1, 0));
                            uvs.Add(uvTl);
                        }

                        uint tlDi = (uint)verts.IndexOf(tlD);

                        indices.Add(tli);
                        indices.Add(tlDi);
                        indices.Add(trDi);

                        indices.Add(tri);
                        indices.Add(tli);
                        indices.Add(trDi);

                    }*/

                    // BOTTOM SKIRT
                    /*if (y == h - 1 || (y < h - 1 && Icon.GetPixel(x, y + 1).A == 1))
                    {
                        Vector3 blD = new Vector3((x * wDiff), (y * hDiff) + hDiff, depth);
                        Vector3 brD = new Vector3((x * wDiff) + wDiff, (y * hDiff) + hDiff, depth);

                        if (!verts.Contains(blD))
                        {
                            verts.Add(blD);
                            normals.Add(new Vector3(0, -1, 0));
                            uvs.Add(uvBl);
                        }

                        uint blDi = (uint)verts.IndexOf(blD);

                        if (!verts.Contains(brD))
                        {
                            verts.Add(brD);
                            normals.Add(new Vector3(0, -1, 0));
                            uvs.Add(uvBr);
                        }

                        uint brDi = (uint)verts.IndexOf(brD);

                        indices.Add(bri);
                        indices.Add(brDi);
                        indices.Add(blDi);

                        indices.Add(blDi);
                        indices.Add(bli);
                        indices.Add(bri);
                    }*/

                    // BACK FACE
                    Vector3 tlB = new Vector3(x * wDiff, y * hDiff, depth);
                    if (!verts.Contains(tlB))
                    {
                        verts.Add(tlB);
                        normals.Add(new Vector3(1, 0, 0));
                        uvs.Add(uvTl);
                    }

                    uint tlBi = (uint)verts.IndexOf(tlB);

                    Vector3 trB = new Vector3((x * wDiff) + wDiff, y * hDiff, depth);
                    if (!verts.Contains(trB))
                    {
                        verts.Add(trB);
                        normals.Add(new Vector3(1, 0, 0));
                        uvs.Add(uvTr);
                    }

                    uint trBi = (uint)verts.IndexOf(trB);

                    Vector3 blB = new Vector3(x * wDiff, (y * hDiff) + wDiff, depth);
                    if (!verts.Contains(blB))
                    {
                        verts.Add(blB);
                        normals.Add(new Vector3(1, 0, 0));
                        uvs.Add(uvBl);
                    }

                    uint blBi = (uint)verts.IndexOf(blB);

                    Vector3 brB = new Vector3((x * wDiff) + wDiff, (y * hDiff) + wDiff, depth);
                    if (!verts.Contains(brB))
                    {
                        verts.Add(brB);
                        normals.Add(new Vector3(1, 0, 0));
                        uvs.Add(uvBr);
                    }

                    uint brBi = (uint)verts.IndexOf(brB);

                    indices.Add(blBi);
                    indices.Add(trBi);
                    indices.Add(tlBi);

                    indices.Add(blBi);
                    indices.Add(brBi);
                    indices.Add(trBi);
                }
            }

            // Offset whole mesh to have pivot at the bottom middle
            for (var index = 0; index < verts.Count; index++)
                verts[index] += new Vector3(-.5f, -1, 0);

            // Create new mesh
            return new Mesh(new VertexNormalContainer(verts.ToArray(), uvs.ToArray(), normals.ToArray()), indices.ToArray());
        }
    }
}
