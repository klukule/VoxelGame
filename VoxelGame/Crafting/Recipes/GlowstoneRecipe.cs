using System.Collections.Generic;
using VoxelGame.Items;

namespace VoxelGame.Crafting.Recipes
{
    /// <summary>
    /// Glowstone Recipe
    /// </summary>
    public class GlowstoneRecipe : CraftingRecipe
    {
        public override string[] RecipeLayouts { get; } = new[] { "#" };
        public override Dictionary<char, string> ItemsKey { get; } = new Dictionary<char, string>() { { '#', GameItems.DIRT.Key } };
        public override CraftingRecipeOutput Output { get; } = new CraftingRecipeOutput(GameItems.GLOWSTONE.Key, 4);
    }
}
