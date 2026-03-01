using MyRecipes.Core.Entities;

namespace MyRecipes.Core.Interfaces;

public interface IRecipeService
{
    Task<List<Recipe>> GetAllRecipesAsync();

    Task<Recipe?> GetRecipeByIdAsync(Guid recipeId);

    Task<bool> AddOrUpdateRecipeAsync(Recipe recipe);

    Task<bool> DeleteRecipeAsync(Guid recipeId);
}