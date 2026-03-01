using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyRecipes.Core.Entities;
using MyRecipes.Core.Interfaces;
using MyRecipes.Data;

namespace MyRecipes.ApplicationServices;

public class RecipeService : IRecipeService
{
    private readonly IDbContextFactory<RecipeDbContext> _dbContextFactory;

    private readonly ILogger<RecipeService> _logger;

    public RecipeService(IDbContextFactory<RecipeDbContext> dbContextFactory, ILogger<RecipeService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<List<Recipe>> GetAllRecipesAsync()
    {
        try
        {
            _logger.LogInformation("Loading all recipes");
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Recipes
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        return [];
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid recipeId)
    {
        try
        {
            _logger.LogInformation("Loading recipe");
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            return await context.Recipes
                .Include(r => r.Ingredients)
                .Where(x => x.Id == recipeId)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        return null;
    }

    public async Task<bool> AddOrUpdateRecipeAsync(Recipe recipe)
    {
        try
        {
            _logger.LogInformation("Adding or updating recipe");
            await using var context = await _dbContextFactory.CreateDbContextAsync();

            var existingRecipe = await context.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == recipe.Id);
            if (existingRecipe == null)
            {
                context.Recipes.Add(recipe);
            }
            else
            {
                existingRecipe.Title = recipe.Title;
                existingRecipe.Description = recipe.Description;
                existingRecipe.Image = recipe.Image;
                existingRecipe.Instructions = recipe.Instructions;

                foreach (var existingIngredient in existingRecipe.Ingredients.ToList().Where(existingIngredient =>
                             recipe.Ingredients.All(x => x.Id != existingIngredient.Id)))
                    context.Ingredients.Remove(existingIngredient);

                foreach (var incomingIngredient in recipe.Ingredients)
                {
                    var existingIngredient = existingRecipe.Ingredients
                        .FirstOrDefault(i => i.Id == incomingIngredient.Id);

                    if (existingIngredient != null)
                    {
                        existingIngredient.Name = incomingIngredient.Name;
                        existingIngredient.Quantity = incomingIngredient.Quantity;
                        existingIngredient.QuantityType = incomingIngredient.QuantityType;
                    }
                    else
                    {
                        existingRecipe.Ingredients.Add(new Ingredient
                        {
                            Name = incomingIngredient.Name,
                            Quantity = incomingIngredient.Quantity,
                            QuantityType = incomingIngredient.QuantityType
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        return false;
    }

    public async Task<bool> DeleteRecipeAsync(Guid recipeId)
    {
        try
        {
            _logger.LogInformation("Deleting recipe");
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var recipe = await context.Recipes
                .Include(r => r.Ingredients)
                .Where(x => x.Id == recipeId)
                .FirstOrDefaultAsync();

            if (recipe != null)
            {
                context.Recipes.Remove(recipe);
                await context.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
        }

        return false;
    }
}