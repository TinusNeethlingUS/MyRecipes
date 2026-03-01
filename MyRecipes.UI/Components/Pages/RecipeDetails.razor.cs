using Microsoft.AspNetCore.Components;

namespace MyRecipes.UI.Components.Pages;

public partial class RecipeDetails
{
    [Parameter]
    public Guid? RecipeId { get; set; }
}
