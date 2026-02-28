using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MyRecipes.UI.Components.Dialogs;

public partial class AddEditRecipeDialog
{
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public Guid? RecipeId { get; set; }
}