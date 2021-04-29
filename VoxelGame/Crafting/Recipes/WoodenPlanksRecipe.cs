using System.Collections.Generic;
using VoxelGame.Items;

namespace VoxelGame.Crafting.Recipes
{
    /// <summary>
    /// Oak wooden plans recipe
    /// </summary>
    public class WoodenPlanksRecipe : CraftingRecipe
    {
        public override string[] RecipeLayouts { get; } = new[] { "#" };
        public override Dictionary<char, string> ItemsKey { get; } = new Dictionary<char, string>() { { '#', GameItems.LOG_OAK.Key } };
        public override CraftingRecipeOutput Output { get; } = new CraftingRecipeOutput(GameItems.PLANKS_OAK.Key, 4);
    }
}
