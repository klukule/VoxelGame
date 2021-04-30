using System.Collections.Generic;
using VoxelGame.Items;

namespace VoxelGame.Crafting.Recipes
{
    /// <summary>
    /// AStart Tester Recipe
    /// </summary>
    public class AStartTesterRecipe : CraftingRecipe
    {
        public override string[] RecipeLayouts { get; } = new[] { "#" };
        public override Dictionary<char, string> ItemsKey { get; } = new Dictionary<char, string>() { { '#', GameItems.PLANKS_OAK.Key } };
        public override CraftingRecipeOutput Output { get; } = new CraftingRecipeOutput(GameItems.ASTAR.Key, 64);
    }
}
