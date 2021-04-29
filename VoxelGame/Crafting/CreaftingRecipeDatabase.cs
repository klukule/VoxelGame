using System.Collections.Generic;
using VoxelGame.Containers;
using VoxelGame.Crafting.Recipes;

namespace VoxelGame.Crafting
{
    /// <summary>
    /// Database containing all crafting recipes
    /// </summary>
    public class CraftingRecipeDatabase
    {
        private static readonly List<CraftingRecipe> _recipes = new List<CraftingRecipe>();

        /// <summary>
        /// Initialize built-in recipes
        /// </summary>
        public static void Init()
        {
            RegisterRecipe(new WoodenPlanksRecipe());
        }

        /// <summary>
        /// Register new crafting recipe
        /// </summary>
        /// <param name="recipe">The recipe to register</param>
        public static void RegisterRecipe(CraftingRecipe recipe)
        {
            if (_recipes.Contains(recipe))
            {
                Debug.Log($"Recipe: {recipe.GetType().Name} has already been registered. Skipping.", DebugLevel.Warning);
                return;
            }
            _recipes.Add(recipe);
        }

        /// <summary>
        /// Get crafting recipe matching input in the crafting container
        /// </summary>
        /// <param name="container">The container</param>
        /// <returns>The crafting recipe if found; otherwise null</returns>
        public static CraftingRecipe GetMatchingRecipe(CraftingContainer container)
        {
            for (int i = 0; i < _recipes.Count; i++)
            {
                CraftingRecipe recipe = _recipes[i];
                if (recipe.Matches(container))
                    return recipe;
            }

            return null;
        }
    }
}
