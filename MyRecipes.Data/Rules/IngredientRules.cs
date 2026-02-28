using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyRecipes.Core.Entities;

namespace MyRecipes.Data.Rules;

public class IngredientRules : EntityBaseRules<Ingredient>
{
    public override void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.QuantityType).IsRequired();
        builder.Property(x => x.Quantity).HasPrecision(18, 2).IsRequired();
    }
}