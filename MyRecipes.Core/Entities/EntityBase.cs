using CommunityToolkit.Mvvm.ComponentModel;

namespace MyRecipes.Core.Entities;

public partial class EntityBase : ObservableValidator
{
    [ObservableProperty] private Guid _id = Guid.Empty;
}