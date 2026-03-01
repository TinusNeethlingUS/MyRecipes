using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MyRecipes.ApplicationServices;
using MyRecipes.Core.Entities;
using MyRecipes.Core.Enumerations;
using MyRecipes.Core.Interfaces;
using MyRecipes.Data;
using Testcontainers.MsSql;

namespace MyRecipes.Tests
{
    [TestClass]
    public sealed class MyRecipeServiceTests : IAsyncDisposable
    {
        private MsSqlContainer? _dbContainer;
        private DbContextOptions<RecipeDbContext>? _options;
        private Mock<IDbContextFactory<RecipeDbContext>>? _mockFactory;
        private Mock<ILogger<RecipeService>>? _mockLogger;
        private IRecipeService? _sut;

        [TestInitialize]
        public async Task Setup()
        {
            _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();

            await _dbContainer.StartAsync();

            _options = new DbContextOptionsBuilder<RecipeDbContext>()
                .UseSqlServer(_dbContainer.GetConnectionString())
                .Options;

            await using (var context = new RecipeDbContext(_options))
            {
                await context.Database.MigrateAsync();
            }

            _mockFactory = new Mock<IDbContextFactory<RecipeDbContext>>();
            _mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new RecipeDbContext(_options));

            _mockLogger = new Mock<ILogger<RecipeService>>();
            _sut = new RecipeService(_mockFactory.Object, _mockLogger.Object);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            if (_dbContainer != null)
            {
                await _dbContainer.DisposeAsync();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_dbContainer != null)
            {
                await _dbContainer.DisposeAsync();
            }
        }

        [TestMethod]
        public async Task GetAllRecipesAsync_WithExistingRecipes_ReturnsList()
        {
            // Arrange
            await using (var context = new RecipeDbContext(_options!))
            {
                context.Recipes.Add(new Recipe { Id = Guid.NewGuid(), Title = "Pancakes" });
                context.Recipes.Add(new Recipe { Id = Guid.NewGuid(), Title = "Waffles" });
                await context.SaveChangesAsync();
            }

            // Act
            var result = await _sut!.GetAllRecipesAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(2, result);
        }

        [TestMethod]
        public async Task GetRecipeByIdAsync_WhenRecipeExists_ReturnsRecipeWithIngredients()
        {
            // Arrange
            var recipeId = Guid.NewGuid();
            await using (var context = new RecipeDbContext(_options!))
            {
                context.Recipes.Add(new Recipe
                {
                    Id = recipeId,
                    Title = "Omelette",
                    Ingredients = new List<Ingredient>
                    {
                        new() { Id = Guid.NewGuid(), Name = "Eggs" }
                    }
                });
                await context.SaveChangesAsync();
            }

            // Act
            var result = await _sut!.GetRecipeByIdAsync(recipeId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Omelette", result.Title);
            Assert.HasCount(1, result.Ingredients);
            Assert.AreEqual("Eggs", result.Ingredients.First().Name);
        }

        [TestMethod]
        public async Task AddOrUpdateRecipeAsync_WithNewRecipe_AddsToDatabase()
        {
            // Arrange
            var newRecipe = new Recipe
            {
                Id = Guid.NewGuid(),
                Title = "Chili",
                Ingredients = new List<Ingredient>()
            };

            // Act
            var result = await _sut!.AddOrUpdateRecipeAsync(newRecipe);

            // Assert
            Assert.IsTrue(result);
            await using var context = new RecipeDbContext(_options!);
            var savedRecipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == newRecipe.Id);
            Assert.IsNotNull(savedRecipe);
            Assert.AreEqual("Chili", savedRecipe.Title);
        }

        [TestMethod]
        public async Task AddOrUpdateRecipeAsync_WithExistingRecipe_UpdatesRecipeAndIngredients()
        {
            // Arrange
            var recipeId = Guid.NewGuid();
            var ingredient1Id = Guid.NewGuid();
            var ingredient2Id = Guid.NewGuid();
            await using (var context = new RecipeDbContext(_options!))
            {
                context.Recipes.Add(new Recipe
                {
                    Id = recipeId,
                    Title = "Original Recipe",
                    Description = "Original Description",
                    Instructions = "Original Instructions",
                    Image = new byte[1],
                    Ingredients = new List<Ingredient>
                    {
                        new() { Id = ingredient1Id, Name = "Original Ingredient 1", Quantity = 1, QuantityType = QuantityType.Count },
                        new() { Id = ingredient2Id, Name = "Original Ingredient 2", Quantity = 2, QuantityType = QuantityType.Gram }
                    }
                });
                await context.SaveChangesAsync();
            }

            var updatedRecipe = new Recipe
            {
                Id = recipeId,
                Title = "Updated Recipe",
                Description = "Updated Description",
                Instructions = "Updated Instructions",
                Image = new byte[2],
                Ingredients = new List<Ingredient>
                {
                    new() { Id = ingredient1Id, Name = "Updated Ingredient 1", Quantity = 0.5m, QuantityType = QuantityType.Gram },
                    new() { Name = "New Ingredient 3", Quantity = 3, QuantityType = QuantityType.Gram }
                }
            };

            // Act
            var result = await _sut!.AddOrUpdateRecipeAsync(updatedRecipe);

            // Assert
            Assert.IsTrue(result);
            await using var verifyContext = new RecipeDbContext(_options!);
            var savedRecipe = await verifyContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            Assert.IsNotNull(savedRecipe);
            Assert.AreEqual("Updated Recipe", savedRecipe.Title);
            Assert.AreEqual("Updated Description", savedRecipe.Description);
            Assert.AreEqual("Updated Instructions", savedRecipe.Instructions);
            Assert.HasCount(2, savedRecipe.Image!);
            Assert.HasCount(2, savedRecipe.Ingredients);

            var savedIngredient1 = savedRecipe.Ingredients.FirstOrDefault(i => i.Id == ingredient1Id);
            Assert.IsNotNull(savedIngredient1);
            Assert.AreEqual("Updated Ingredient 1", savedIngredient1.Name);
            Assert.AreEqual(0.5m, savedIngredient1.Quantity);
            Assert.AreEqual(QuantityType.Gram, savedIngredient1.QuantityType);

            var newIngredient = savedRecipe.Ingredients.FirstOrDefault(i => i.Name == "New Ingredient 3");
            Assert.IsNotNull(newIngredient);
            Assert.AreEqual(3, newIngredient.Quantity);
            Assert.AreEqual(QuantityType.Gram, newIngredient.QuantityType);

            var removedIngredient = savedRecipe.Ingredients.FirstOrDefault(i => i.Id == ingredient2Id);
            Assert.IsNull(removedIngredient);
        }

        [TestMethod]
        public async Task DeleteRecipeAsync_WhenRecipeExists_DeletesRecipeAndIngredients()
        {
            // Arrange
            var recipeId = Guid.NewGuid();

            await using (var context = new RecipeDbContext(_options!))
            {
                context.Recipes.Add(new Recipe
                {
                    Id = recipeId,
                    Title = "Delete Me",
                    Ingredients = new List<Ingredient>
                    {
                        new() { Id = Guid.NewGuid(), Name = "Salt" }
                    }
                });

                await context.SaveChangesAsync();
            }

            // Act
            var result = await _sut!.DeleteRecipeAsync(recipeId);

            // Assert
            Assert.IsTrue(result);

            await using var verifyContext = new RecipeDbContext(_options!);
            var deleted = await verifyContext.Recipes
                .Include(r => r.Ingredients)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            Assert.IsNull(deleted);
        }

        [TestMethod]
        public async Task DeleteRecipeAsync_WhenRecipeDoesNotExist_ReturnsFalse()
        {
            // Act
            var result = await _sut!.DeleteRecipeAsync(Guid.NewGuid());

            // Assert
            Assert.IsFalse(result);
        }
    }
}