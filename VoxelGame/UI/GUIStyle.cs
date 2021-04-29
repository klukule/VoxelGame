using OpenTK;
using OpenTK.Graphics;
using System;
using VoxelGame.Rendering;

namespace VoxelGame.UI
{
    /// <summary>
    /// Text vertical alignment
    /// </summary>
    public enum VerticalAlignment
    {
        Top,
        Middle,
        Bottom
    }

    /// <summary>
    /// Textu horizontal alignment
    /// </summary>
    public enum HorizontalAlignment
    {
        Left,
        Middle,
        Right
    }

    /// <summary>
    /// GUI Style option
    /// </summary>
    public class GUIStyleOption
    {
        /// <summary>
        /// Background texture
        /// </summary>
        public Texture Background;

        /// <summary>
        /// Text color
        /// </summary>
        public Color4 TextColor = Color4.White;
    }

    /// <summary>
    /// GUI Style
    /// </summary>
    public class GUIStyle : ICloneable
    {
        /// <summary>
        /// Normal style
        /// </summary>
        public GUIStyleOption Normal { get; set; } = new GUIStyleOption();

        /// <summary>
        /// Hover style
        /// </summary>
        public GUIStyleOption Hover { get; set; } = new GUIStyleOption();

        /// <summary>
        /// Active style
        /// </summary>
        public GUIStyleOption Active { get; set; } = new GUIStyleOption();

        /// <summary>
        /// Selected font
        /// </summary>
        public Font Font { get; set; }

        /// <summary>
        /// Selected font size
        /// </summary>
        public float FontSize { get; set; }

        /// <summary>
        /// "nine-patch" border size
        /// </summary>
        public float SlicedBorderSize { get; set; }

        /// <summary>
        /// Text vertical alignment
        /// </summary>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Text horizontal alignment
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Text offset
        /// </summary>
        public Vector2 AlignmentOffset { get; set; } = new Vector2(0, 0);

        /// <summary>
        /// Create copy of the style
        /// </summary>
        /// <returns></returns>
        public object Clone() => new GUIStyle()
        {
            Normal = Normal,
            Hover = Hover,
            Active = Active,
            Font = Font,
            FontSize = FontSize,
            VerticalAlignment = VerticalAlignment,
            HorizontalAlignment = HorizontalAlignment,
            AlignmentOffset = AlignmentOffset,
            SlicedBorderSize = SlicedBorderSize
        };
    }
}
