using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
using MyRecipes.Core.Entities;
using MyRecipes.Core.Interfaces;
using MyRecipes.UI.Components.Dialogs;

namespace MyRecipes.UI.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly ILogger _logger;
    private readonly IRecipeService _recipeService;
    private readonly ISnackbar _snackBar;

    [ObservableProperty] 
    private bool _isLoading;

    [ObservableProperty] 
    private IEnumerable<Recipe> _recipes = [];

    [ObservableProperty] 
    private string? _searchString;

    [ObservableProperty] 
    private Recipe? _selectedRecipe;

    public HomeViewModel(IRecipeService recipeService, IDialogService dialogService, ILogger<HomeViewModel> logger, ISnackbar snackBar)
    {
        _recipeService = recipeService;
        _dialogService = dialogService;
        _logger = logger;
        _snackBar = snackBar;
    }

    public override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) await RefreshAsync();

        await base.OnAfterRenderAsync(firstRender);
    }

    public async Task RefreshAsync()
    {
        IsLoading = true;
        Recipes = await _recipeService.GetAllRecipesAsync();
        IsLoading = false;
    }

    [RelayCommand]
    public async Task AddRecipeAsync()
    {
        var parameters = new DialogParameters<AddEditRecipeDialog>
        {
            { x => x.RecipeId, null }
        };

        var dialog =
            await _dialogService.ShowAsync<AddEditRecipeDialog>("Add Recipe", parameters, GetDefaultDialogOptions());
        var result = await dialog.Result;
        if (result is { Canceled: false }) await RefreshAsync();
    }

    [RelayCommand]
    public async Task EditRecipeAsync()
    {
        var parameters = new DialogParameters<AddEditRecipeDialog>
        {
            { x => x.RecipeId, SelectedRecipe!.Id }
        };

        var dialog =
            await _dialogService.ShowAsync<AddEditRecipeDialog>("Edit Recipe", parameters, GetDefaultDialogOptions());
        var result = await dialog.Result;
        if (result is { Canceled: false }) await RefreshAsync();
    }

    [RelayCommand]
    public async Task DeleteRecipeAsync()
    {
        if (SelectedRecipe != null)
        {
            if (await _recipeService.DeleteRecipeAsync(SelectedRecipe.Id))
            {
                _snackBar.Add("Recipe deleted.", Severity.Success);
                await RefreshAsync();
            }
            else
            {
                _snackBar.Add("Failed to delete recipe.", Severity.Error);
            }
        }
    }

    private static DialogOptions GetDefaultDialogOptions()
    {
        return new DialogOptions
        {
            CloseButton = true,
            BackdropClick = false,
            FullScreen = false,
            FullWidth = true,
            MaxWidth = MaxWidth.Large
        };
    }

    public bool FilterFunc(Recipe recipe)
    {
        return string.IsNullOrWhiteSpace(SearchString) ||
               recipe.Title.Contains(SearchString, StringComparison.OrdinalIgnoreCase);
    }

    public string SelectedRowClassFunc(Recipe recipe, int rowNumber)
    {
        return SelectedRecipe != null && recipe.Id == SelectedRecipe.Id ? "selected" : string.Empty;
    }
}