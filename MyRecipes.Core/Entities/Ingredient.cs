using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using MyRecipes.Core.Enumerations;

namespace MyRecipes.Core.Entities;

public partial class Ingredient : EntityBase
{
    [ObservableProperty] [Required] [StringLength(100)]
    private string _name = string.Empty;

    [ObservableProperty] [Range(0.01, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    private decimal _quantity;

    [ObservableProperty] private QuantityType _quantityType;

    [ObservableProperty] private Guid? _recipeId;

    public Recipe? Recipe { get; set; }
}