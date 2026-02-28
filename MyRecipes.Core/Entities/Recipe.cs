using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MyRecipes.Core.Entities;

public partial class Recipe : EntityBase
{
    [ObservableProperty] private string? _description;

    [ObservableProperty] private byte[]? _image;

    [ObservableProperty] [Required] private string _instructions = string.Empty;

    [ObservableProperty] [Required] [StringLength(100)]
    private string _title = string.Empty;

    public virtual ICollection<Ingredient> Ingredients { get; set; } = [];

    [NotMapped]
    public string? ImageDataUrl => Image is not null
        ? $"data:image/png;base64,{Convert.ToBase64String(Image)}"
        : null;
}