using MyRecipes.ApplicationServices;
using MyRecipes.Core.Interfaces;

namespace MyRecipes.UI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IRecipeService, RecipeService>();
        return serviceCollection;
    }
}