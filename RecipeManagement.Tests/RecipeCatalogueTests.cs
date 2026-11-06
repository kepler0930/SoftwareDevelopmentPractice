using RecipeManagement.Core;

namespace RecipeManagement.Tests;

[Trait("Part", "A.1")]
[Trait("Feature", "Catalogue")]
public sealed class RecipeCatalogueTests
{
    [Fact]
    public void Constructor_WithEmptyCollection_CreatesEmptyCatalogue()
    {
        IRecipeManager manager = new RecipeManager(new List<Recipe>());

        Assert.Equal(0, manager.RecipeCount);
        Assert.Null(manager.FindRecipe(10));
    }

    [Fact]
    public void Constructor_WithNullCollection_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RecipeManager(null!));
    }

    [Theory]
    [InlineData(0, "Recipe")]
    [InlineData(-1, "Recipe")]
    [InlineData(10, null)]
    [InlineData(10, "")]
    [InlineData(10, " \t ")]
    public void Constructor_WithInvalidRecipe_ThrowsArgumentException(int id, string? title)
    {
        Recipe recipe = new Recipe();
        recipe.Id = id;
        recipe.Title = title!;

        Assert.Throws<ArgumentException>(() => new RecipeManager(new[] { recipe }));
    }

    [Fact]
    public void Constructor_WithDuplicateIds_ThrowsArgumentException()
    {
        List<Recipe> recipes = new List<Recipe>();
        recipes.Add(CreateRecipe(10, "First"));
        recipes.Add(CreateRecipe(10, "Second"));

        Assert.Throws<ArgumentException>(() => new RecipeManager(recipes));
    }

    [Fact]
    public void Constructor_WithNullRecipeEntry_ThrowsArgumentException()
    {
        Recipe[] recipes = new Recipe[] { null! };

        Assert.Throws<ArgumentException>(() => new RecipeManager(recipes));
    }

    [Fact]
    public void Constructor_MakesAllSuppliedRecipesAvailableById()
    {
        Recipe first = CreateRecipe(10, "First");
        Recipe second = CreateRecipe(20, "Second");
        IRecipeManager manager = new RecipeManager(new[] { first, second });

        Assert.Equal(2, manager.RecipeCount);
        Assert.Same(first, manager.FindRecipe(10));
        Assert.Same(second, manager.FindRecipe(20));
    }

    [Fact]
    public void AddRecipe_WithValidRecipe_AddsItToCatalogue()
    {
        IRecipeManager manager = CreateManager();
        Recipe recipe = CreateRecipe(30, "Third");

        Assert.True(manager.AddRecipe(recipe));
        Assert.Equal(3, manager.RecipeCount);
        Assert.Same(recipe, manager.FindRecipe(30));
    }

    [Fact]
    public void AddRecipe_WithNull_ThrowsWithoutChangingCatalogue()
    {
        IRecipeManager manager = CreateManager();

        Assert.Throws<ArgumentNullException>(() => manager.AddRecipe(null!));
        Assert.Equal(2, manager.RecipeCount);
    }

    [Theory]
    [InlineData(0, "Recipe")]
    [InlineData(-1, "Recipe")]
    [InlineData(30, null)]
    [InlineData(30, "")]
    [InlineData(30, " \t ")]
    public void AddRecipe_WithInvalidRecipe_ReturnsFalse(int id, string? title)
    {
        IRecipeManager manager = CreateManager();
        Recipe recipe = new Recipe();
        recipe.Id = id;
        recipe.Title = title!;

        Assert.False(manager.AddRecipe(recipe));
        Assert.Equal(2, manager.RecipeCount);
        Assert.Null(manager.FindRecipe(id));
    }

    [Fact]
    public void AddRecipe_WithDuplicateId_KeepsOriginalRecipe()
    {
        IRecipeManager manager = CreateManager();
        Recipe? original = manager.FindRecipe(10);

        Assert.False(manager.AddRecipe(CreateRecipe(10, "Replacement")));
        Assert.Equal(2, manager.RecipeCount);
        Assert.Same(original, manager.FindRecipe(10));
    }

    [Fact]
    public void AddRecipe_WithSameTitleAndDifferentId_IsAllowed()
    {
        IRecipeManager manager = CreateManager();

        Assert.True(manager.AddRecipe(CreateRecipe(30, "Recipe A")));
        Assert.Equal(3, manager.RecipeCount);
        Assert.Equal("Recipe A", manager.FindRecipe(30)?.Title);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    public void FindRecipe_WithMissingId_ReturnsNull(int recipeId)
    {
        IRecipeManager manager = CreateManager();

        Assert.Null(manager.FindRecipe(recipeId));
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void RemoveRecipe_WithExistingId_RemovesOnlyThatRecipe()
    {
        IRecipeManager manager = CreateManager();

        Assert.True(manager.RemoveRecipe(10));
        Assert.Null(manager.FindRecipe(10));
        Assert.NotNull(manager.FindRecipe(20));
        Assert.Equal(1, manager.RecipeCount);

        Assert.False(manager.RemoveRecipe(10));
        Assert.Equal(1, manager.RecipeCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    public void RemoveRecipe_WithMissingId_DoesNotChangeCatalogue(int recipeId)
    {
        IRecipeManager manager = CreateManager();

        Assert.False(manager.RemoveRecipe(recipeId));
        Assert.Equal(2, manager.RecipeCount);
        Assert.NotNull(manager.FindRecipe(10));
        Assert.NotNull(manager.FindRecipe(20));
    }

    [Fact]
    public void RemovedId_CanBeUsedForNewRecipe()
    {
        IRecipeManager manager = CreateManager();
        Recipe replacement = CreateRecipe(10, "New recipe");

        Assert.True(manager.RemoveRecipe(10));
        Assert.True(manager.AddRecipe(replacement));
        Assert.Equal(2, manager.RecipeCount);
        Assert.Same(replacement, manager.FindRecipe(10));
        Assert.NotNull(manager.FindRecipe(20));
    }

    private static IRecipeManager CreateManager()
    {
        List<Recipe> recipes = new List<Recipe>();
        recipes.Add(CreateRecipe(10, "Recipe A"));
        recipes.Add(CreateRecipe(20, "Recipe B"));

        return new RecipeManager(recipes);
    }

    private static Recipe CreateRecipe(int id, string title)
    {
        Recipe recipe = new Recipe();
        recipe.Id = id;
        recipe.Title = title;
        return recipe;
    }
}
