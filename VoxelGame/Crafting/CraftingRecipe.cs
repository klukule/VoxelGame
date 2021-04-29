using System.Collections.Generic;
using VoxelGame.Containers;

namespace VoxelGame.Crafting
{
    /// <summary>
    /// Base for any crafting recipe
    /// </summary>
    public abstract class CraftingRecipe
    {
        /// <summary>
        /// Recipe layout
        /// </summary>
        /// <remarks>
        /// Each line is one row
        /// 1x1 recipe is specified as:
        /// ["X"]
        /// 
        /// 2x2 recipe is specified as:
        /// ["XX", "XX"]
        /// 
        /// etc..
        /// </remarks>
        public abstract string[] RecipeLayouts { get; }

        /// <summary>
        /// Translation table of character to item key for the recipe layout
        /// </summary>
        public abstract Dictionary<char, string> ItemsKey { get; }

        /// <summary>
        /// Output of the recipe
        /// </summary>
        public abstract CraftingRecipeOutput Output { get; }

        /// <summary>
        /// Recipe layout width
        /// </summary>
        public int Width => RecipeLayouts.Length;

        /// <summary>
        /// Recipe layout height
        /// </summary>
        public int Height => RecipeLayouts[0].Length;

        /// <summary>
        /// Checks whether the input in crafting container matches the recipe
        /// </summary>
        /// <param name="container">The container</param>
        /// <returns>True if recipe matches; otherwise false</returns>
        public bool Matches(CraftingContainer container)
        {
            // Try match layout offset by [x,y], allow for flipping the x axis for mirrored templates
            bool CheckMatch(int x, int y, bool flipX)
            {
                for (int i = 0; i < container.ContainerSize.X; i++)
                {
                    for (int j = 0; j < container.ContainerSize.Y; j++)
                    {
                        int k = i - x;
                        int l = j - y;

                        char ingredientKey = ' ';

                        // Get ingredient key from layout
                        if (k >= 0 && l >= 0 && k < Width && l < Height)
                        {
                            // Get key from layout
                            if (flipX)
                                ingredientKey = RecipeLayouts[l][Width - k - 1];
                            else
                                ingredientKey = RecipeLayouts[l][k];
                        }

                        // If ingredient empty (either outside the offset or specified in template) and slot is also empty 
                        if (ingredientKey == ' ' && container.GetItemStackByLocation(i, (int)container.ContainerSize.Y - j - 1) == null) continue;

                        // If ingrediant empty and shouldn't be or template doesn't match the item in the container exit
                        if (ingredientKey == ' ' ||
                            container.GetItemStackByLocation(i, (int)container.ContainerSize.Y - j - 1) == null ||
                            container.GetItemStackByLocation(i, (int)container.ContainerSize.Y - j - 1).ItemKey != ItemsKey[ingredientKey])
                            return false;
                    }
                }

                // Template matches
                return true;
            }

            // Check for each possible position in the container
            for (int i = 0; i <= container.ContainerSize.X - Width; i++)
            {
                for (int y = 0; y <= container.ContainerSize.Y - Height; y++)
                {
                    // Try and match the templates
                    if (CheckMatch(i, y, true))
                        return true;

                    if (CheckMatch(i, y, false))
                        return true;
                }
            }

            // Not matched
            return false;
        }
    }

    /// <summary>
    /// Output of the crafting recipe
    /// </summary>
    public struct CraftingRecipeOutput
    {
        /// <summary>
        /// Item to output
        /// </summary>
        public string ItemKey { get; }

        /// <summary>
        /// The amount to output
        /// </summary>
        public byte Count { get; }

        public CraftingRecipeOutput(string itemKey, byte count)
        {
            ItemKey = itemKey;
            Count = count;
        }
    }
}
