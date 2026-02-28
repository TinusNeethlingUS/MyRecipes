using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyRecipes.Core.Entities;

namespace MyRecipes.Data.Rules;

public class RecipeRules : EntityBaseRules<Recipe>
{
    public override void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.Property(x => x.Title).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Instructions).IsRequired();
        builder.Property(x => x.Image).HasColumnType("varbinary(max)").IsRequired(false).HasDefaultValue(null);
        builder.HasMany(x => x.Ingredients).WithOne(x => x.Recipe).HasForeignKey(x => x.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}