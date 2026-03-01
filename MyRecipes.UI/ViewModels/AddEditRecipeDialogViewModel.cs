using Blazing.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using MyRecipes.Core.Entities;
using MyRecipes.Core.Interfaces;

namespace MyRecipes.UI.ViewModels;

public partial class RecipeDialogViewModel : ViewModelBase
{
    private readonly ILogger<RecipeDialogViewModel> _logger;
    private readonly IRecipeService _recipeService;
    private readonly ISnackbar _snackBar;

    [ObservableProperty] 
    private bool _isLoading;

    [ObservableProperty] 
    private bool _isValid;

    [ObservableProperty] 
    private Recipe _recipe = new();

    public RecipeDialogViewModel(IRecipeService recipeService, ISnackbar snackBar,
        ILogger<RecipeDialogViewModel> logger)
    {
        _recipeService = recipeService;
        _snackBar = snackBar;
        _logger = logger;
    }

    [property: ViewParameter] 
    public IMudDialogInstance MudDialog { get; set; } = null!;

    [property: ViewParameter] 
    public Guid? RecipeId { get; set; }

    public override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            IsLoading = true;
            if (RecipeId.HasValue)
            {
                var recipe = await _recipeService.GetRecipeByIdAsync(RecipeId.Value);
                if (recipe != null)
                    Recipe = recipe;
                else
                    _snackBar.Add("Failed to load recipe.");
            }

            IsLoading = false;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        IsLoading = true;
        try
        {
            var success = await _recipeService.AddOrUpdateRecipeAsync(Recipe);
            if (success)
            {
                _snackBar.Add("Recipe saved.", Severity.Success);
                MudDialog.Close(DialogResult.Ok(Recipe));
            }
            else
            {
                _snackBar.Add("Save failed.", Severity.Error);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void Cancel()
    {
        MudDialog.Cancel();
    }

    [RelayCommand]
    public void AddIngredient()
    {
        var newList = Recipe.Ingredients.ToList();
        newList.Add(new Ingredient
        {
            Id = Guid.NewGuid(),
            RecipeId = Recipe.Id,
            Name = string.Empty,
            Quantity = 1
        });
        Recipe.Ingredients = newList;
        OnPropertyChanged(nameof(Recipe));
    }

    [RelayCommand]
    public void RemoveIngredient(Ingredient ingredient)
    {
        var list = Recipe.Ingredients.ToList();
        list.Remove(ingredient);
        Recipe.Ingredients = list;
        OnPropertyChanged(nameof(Recipe));
    }

    [RelayCommand]
    public async Task UploadImage(IBrowserFile? file)
    {
        if (file == null) return;

        try
        {
            await using var stream = file.OpenReadStream(5120000);
            await using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);

            Recipe.Image = memoryStream.ToArray();

            OnPropertyChanged(nameof(Recipe));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            _snackBar.Add("Could not process image.", Severity.Error);
        }
    }
}