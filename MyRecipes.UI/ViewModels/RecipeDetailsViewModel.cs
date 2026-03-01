using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MyRecipes.Core.Entities;
using MyRecipes.Core.Interfaces;

namespace MyRecipes.UI.ViewModels;

public partial class RecipeDetailsViewModel : ViewModelBase
{
    private readonly IRecipeService _recipeService;

    [ObservableProperty] 
    private bool _isLoading;

    [ObservableProperty] 
    private Recipe? _recipe;

    public RecipeDetailsViewModel(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [property: ViewParameter]
    public Guid? RecipeId { get; set; }

    public override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await RefreshAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    public async Task RefreshAsync()
    {
        if (RecipeId != null)
        {
            IsLoading = true;
            Recipe = await _recipeService.GetRecipeByIdAsync(RecipeId.Value);
            IsLoading = false;
        }
    }
}