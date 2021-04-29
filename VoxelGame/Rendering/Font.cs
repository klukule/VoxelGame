// Uses freetype under the hood, refer to freetype documentation for all things

using Ionic.Zip;
using OpenTK.Graphics.OpenGL4;
using SharpFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VoxelGame.Assets;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Font glyph metrics
    /// - Stores necessary information to render a glyph
    /// </summary>
    /// <remarks>
    /// Reference to variable meanings: https://www.freetype.org/freetype2/docs/glyphs/glyphs-3.html
    /// </remarks>
    public struct GlyphMetrics
    {
        public float AdvanceWidth;      // Horizontal character advance
        public float AdvanceHeight;     // Vertical character advance

        public float GlyphWidth;        // Character pixel width
        public float GlyphHeight;       // Character pixel height

        public float BearingLeft;       // Character offset from left
        public float BearingTop;        // Character offset from top

        public float U;                 // Texture atlas U coord
        public float V;                 // Texture atlas V coord
    }

    /// <summary>
    /// Bitmap font
    /// </summary>
    public class Font : ILoadable
    {
        private const float MAX_SIZE = 2048;                    // Maximum texture size
        private const uint FONT_SIZE = 32;                      // Font size to render
        private const uint FIRST_CHAR_KEYCODE = 32;             // First keycode to render
        private const uint LAST_CHAR_KEYCODE = 512;             // Last keycode to render

        private static Library _fontLibrary = null;             // SharpFont library
        private Face _fontFace;                                 // SharpFont fontface
        private Dictionary<char, GlyphMetrics> _glyphs;         // Cached glyph metrics
        private int _atlasWidth;                                // Generated atlas width
        private int _atlasHeight;                               // Generated atlas height

        /// <summary>
        /// Font atlas texture
        /// </summary>
        public Texture AtlasTexture { get; private set; }

        /// <summary>
        /// Font size
        /// </summary>
        public float FontSize { get; } = FONT_SIZE;

        /// <summary>
        /// Font line size
        /// </summary>
        public float LineHeight { get; private set; }

        /// <summary>
        /// Texture atlas width
        /// </summary>
        public float AtlasWidth => _atlasWidth;

        /// <summary>
        /// Texture atlas height
        /// </summary>
        public float AtlasHeight => _atlasHeight;

        /// <summary>
        /// Initialize SharpFont
        /// </summary>
        static Font() { if (_fontLibrary == null) _fontLibrary = new Library(); }

        /// <summary>
        /// Constructor for activator
        /// </summary>
        public Font() { }

        /// <summary>
        /// Create font from ttf data
        /// </summary>
        /// <param name="fontData">TTF Font data</param>
        private Font(byte[] fontData)
        {
            _atlasWidth = 0;
            _atlasHeight = 0;

            int roww = 0;   // Row width
            int rowh = 0;   // Row height

            // Load ttf font and set font height
            _fontFace = new Face(_fontLibrary, fontData, 0);
            _fontFace.SetPixelSizes(0, FONT_SIZE);

            // Go through character range
            for (uint i = FIRST_CHAR_KEYCODE; i < LAST_CHAR_KEYCODE; i++)
            {
                // Load the cahracter bitmap
                _fontFace.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);

                // Wrap row width if over maximum size
                if (roww + _fontFace.Glyph.Bitmap.Width + 1 >= MAX_SIZE)
                {
                    // Update atlas size
                    _atlasWidth = Math.Max(_atlasWidth, roww);
                    _atlasHeight += rowh;

                    // Reset row stats
                    roww = 0;
                    rowh = 0;
                }

                // Calculate row stats
                roww += _fontFace.Glyph.Bitmap.Width + 1;
                rowh = Math.Max(rowh, _fontFace.Glyph.Bitmap.Rows);
            }

            // Update atlas size
            _atlasWidth = Math.Max(_atlasWidth, roww);
            _atlasHeight += rowh;

            // Create new atlas texture
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            AtlasTexture = new Texture(_atlasWidth, _atlasHeight);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, AtlasTexture.Handle);
            int ox = 0, oy = 0; // horizontal and vertical atlas UV offsets
            rowh = 0;

            _glyphs = new Dictionary<char, GlyphMetrics>();

            // Render all characters
            for (uint i = FIRST_CHAR_KEYCODE; i < LAST_CHAR_KEYCODE; i++)
            {
                _fontFace.LoadChar(i, LoadFlags.Render, LoadTarget.Normal);

                // Wrap the rows
                if (ox + _fontFace.Glyph.Bitmap.Width + 1 >= MAX_SIZE)
                {
                    oy += rowh;
                    rowh = 0;
                    ox = 0;
                }

                // Update texture sub-region with glyph bitmap
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, ox, oy, _fontFace.Glyph.Bitmap.Width, _fontFace.Glyph.Bitmap.Rows, PixelFormat.Alpha, PixelType.UnsignedByte, _fontFace.Glyph.Bitmap.Buffer);

                // Create glyph metrics
                var metrics = new GlyphMetrics
                {
                    AdvanceWidth = _fontFace.Glyph.Advance.X.Value >> 6,
                    AdvanceHeight = _fontFace.Glyph.Advance.Y.Value >> 6,
                    GlyphWidth = _fontFace.Glyph.Bitmap.Width,
                    GlyphHeight = _fontFace.Glyph.Bitmap.Rows,
                    BearingLeft = _fontFace.Glyph.BitmapLeft,
                    BearingTop = _fontFace.Glyph.BitmapTop,
                    U = ox / (float)_atlasWidth,
                    V = oy / (float)_atlasHeight
                };

                // Update offsets
                rowh = Math.Max(rowh, _fontFace.Glyph.Bitmap.Rows);
                ox += _fontFace.Glyph.Bitmap.Width + 1;

                _glyphs.Add((char)i, metrics);

                // Update line size
                if (_fontFace.Glyph.Bitmap.Rows > LineHeight)
                    LineHeight = _fontFace.Glyph.Bitmap.Rows;
            }

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
        }

        /// <summary>
        /// Loads the font from asset pack
        /// </summary>
        public ILoadable Load(string path, ZipFile pack)
        {
            byte[] data = null;
            if (pack.ContainsEntry(path))
            {
                MemoryStream stream = new MemoryStream();
                pack[path].Extract(stream);
                data = stream.GetBuffer();
            }
            else
                data = File.ReadAllBytes(path);
            return new Font(data);
        }

        /// <summary>
        /// Gets glyph metrics for given character
        /// </summary>
        /// <param name="chara">The character</param>
        /// <returns>Glyph metrics</returns>
        public GlyphMetrics RequestGlyph(char chara)
        {
            if (ContainsCharacter(chara))
                return _glyphs[chara];

            return default;
        }

        /// <summary>
        /// Checks whether the character is loaded or not
        /// </summary>
        /// <param name="chara">The character</param>
        /// <returns>True if character is loaded; otherwise false</returns>
        public bool ContainsCharacter(char chara) => _glyphs.Keys.Contains(chara);

        /// <summary>
        /// Disposes of any assets
        /// </summary>
        public void Dispose()
        {
            _fontFace?.Dispose();
            _fontLibrary?.Dispose();
            AtlasTexture?.Dispose();
        }
    }
}
