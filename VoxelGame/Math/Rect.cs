using OpenTK;

namespace VoxelGame
{
    /// <summary>
    /// 2D rectangle structure
    /// </summary>
    public class Rect
    {
        /// <summary>
        /// X position
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Y position
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Width
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// Height
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Create new rectangle
        /// </summary>
        public Rect(float x, float y, float w, float h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        /// <summary>
        /// Point x rectangle intersection
        /// </summary>
        /// <param name="mousePos">Mouse position</param>
        /// <returns>True if point is inside the rectangle; otherwise false</returns>
        public bool IsPointInside(Vector2 mousePos)
        {
            return mousePos.X > X && mousePos.X < X + Width &&
                   mousePos.Y > Y && mousePos.Y < Y + Height;
        }

        /// <summary>
        /// Expands the rectangle by given amount to all four sides
        /// </summary>
        /// <param name="size">Amount to expand</param>
        /// <returns>Expanded rectangle</returns>
        public Rect Expand(float size) => Expand(size, size);


        /// <summary>
        /// Expands the rectangle by given amount to all four sides
        /// </summary>
        /// <param name="x">Amount to expand horizontally</param>
        /// <param name="y">Amount to expand vertically</param>
        /// <returns>Expanded rectangle</returns>
        public Rect Expand(float x, float y) => new Rect(X - x, Y - y, Width + 2 * x, Height + 2 * y);

        /// <summary>
        /// Shirnks the rectangle by given amount on all four sides
        /// </summary>
        public Rect Shrink(float size) => Expand(-size, -size);
    }
}
