using RecipeManagement.Core;

namespace RecipeManagement.Tests;

[Trait("Part", "A.1")]
[Trait("Feature", "ShoppingList")]
public sealed class ShoppingListTests
{
    [Fact]
    public void NewManager_HasEmptyShoppingList()
    {
        IRecipeManager manager = CreateManager();

        Assert.Empty(manager.GetShoppingList());
        Assert.Equal(0, manager.ShoppingItemCount);
    }

    [Fact]
    public void AddIngredientsToShoppingList_PreservesOriginalStringsAndOrder()
    {
        Recipe recipe = new Recipe();
        recipe.Id = 10;
        recipe.Title = "Recipe";
        recipe.Ingredients.Add(" 1 apple ");
        recipe.Ingredients.Add("2 cups flour");
        recipe.Ingredients.Add("1 apple");
        IRecipeManager manager = new RecipeManager(new[] { recipe });

        Assert.Equal(3, manager.AddIngredientsToShoppingList(10));
        Assert.Equal(new[] { " 1 apple ", "2 cups flour", "1 apple" }, manager.GetShoppingList());
        Assert.Equal(3, manager.ShoppingItemCount);
    }

    [Fact]
    public void AddIngredientsToShoppingList_WithMultipleRecipes_AppendsInCallOrder()
    {
        IRecipeManager manager = CreateManager();

        Assert.Equal(1, manager.AddIngredientsToShoppingList(20));
        Assert.Equal(2, manager.AddIngredientsToShoppingList(10));
        Assert.Equal(new[] { "1 egg", "1 apple", "2 cups flour" }, manager.GetShoppingList());
        Assert.Equal(3, manager.ShoppingItemCount);
        Assert.Equal(3, manager.RecipeCount);
    }

    [Fact]
    public void AddIngredientsToShoppingList_WithSameRecipeTwice_KeepsBothCopies()
    {
        IRecipeManager manager = CreateManager();

        Assert.Equal(1, manager.AddIngredientsToShoppingList(20));
        Assert.Equal(1, manager.AddIngredientsToShoppingList(20));
        Assert.Equal(new[] { "1 egg", "1 egg" }, manager.GetShoppingList());
        Assert.Equal(2, manager.ShoppingItemCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    public void AddIngredientsToShoppingList_WithMissingRecipe_KeepsExistingItems(int recipeId)
    {
        IRecipeManager manager = CreateManager();
        manager.AddIngredientsToShoppingList(20);

        Assert.Equal(0, manager.AddIngredientsToShoppingList(recipeId));
        Assert.Equal(new[] { "1 egg" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
    }

    [Fact]
    public void AddIngredientsToShoppingList_WithNoIngredients_ReturnsZero()
    {
        IRecipeManager manager = CreateManager();
        manager.AddIngredientsToShoppingList(20);

        Assert.Equal(0, manager.AddIngredientsToShoppingList(30));
        Assert.Equal(new[] { "1 egg" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
    }

    [Fact]
    public void ClearShoppingList_RemovesItemsWithoutRemovingRecipes()
    {
        IRecipeManager manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);
        manager.AddIngredientsToShoppingList(20);

        manager.ClearShoppingList();

        Assert.Empty(manager.GetShoppingList());
        Assert.Equal(0, manager.ShoppingItemCount);
        Assert.Equal(3, manager.RecipeCount);
        Assert.NotNull(manager.FindRecipe(10));
        Assert.NotNull(manager.FindRecipe(20));
    }

    [Fact]
    public void ClearShoppingList_WithEmptyList_CanBeRepeated()
    {
        IRecipeManager manager = CreateManager();

        manager.ClearShoppingList();
        manager.ClearShoppingList();

        Assert.Empty(manager.GetShoppingList());
        Assert.Equal(0, manager.ShoppingItemCount);
    }

    [Fact]
    public void ClearShoppingList_AllowsAddingIngredientsAgain()
    {
        IRecipeManager manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);
        manager.ClearShoppingList();

        Assert.Equal(1, manager.AddIngredientsToShoppingList(20));
        Assert.Equal(new[] { "1 egg" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
    }

    [Fact]
    public void GetShoppingList_ReturnsCopyThatCannotChangeInternalItems()
    {
        IRecipeManager manager = CreateManager();
        manager.AddIngredientsToShoppingList(20);

        IList<string> returnedItems = (IList<string>)manager.GetShoppingList();
        try
        {
            returnedItems[0] = "Changed by caller";
        }
        catch (NotSupportedException)
        {
            // A read-only result also protects the internal list.
        }

        Assert.Equal(new[] { "1 egg" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
    }

    [Fact]
    public void GetShoppingList_CopyDoesNotChangeAfterLaterAdditionsOrClearing()
    {
        IRecipeManager manager = CreateManager();
        manager.AddIngredientsToShoppingList(20);
        IReadOnlyList<string> previousItems = manager.GetShoppingList();

        manager.AddIngredientsToShoppingList(10);
        manager.ClearShoppingList();

        Assert.Equal(new[] { "1 egg" }, previousItems);
        Assert.Empty(manager.GetShoppingList());
        Assert.Equal(0, manager.ShoppingItemCount);
    }

    [Fact]
    public void ShoppingList_CopiesIngredientsInsteadOfSharingRecipeList()
    {
        IRecipeManager manager = CreateManager();
        Recipe? recipe = manager.FindRecipe(20);
        Assert.NotNull(recipe);
        manager.AddIngredientsToShoppingList(20);

        recipe.Ingredients[0] = "Changed ingredient";
        recipe.Ingredients.Add("Extra ingredient");

        Assert.Equal(new[] { "1 egg" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
    }

    [Fact]
    public void RemovingRecipeFromCatalogue_DoesNotRemoveCopiedShoppingItems()
    {
        IRecipeManager manager = CreateManager();
        Assert.Equal(2, manager.AddIngredientsToShoppingList(10));

        Assert.True(manager.RemoveRecipe(10));
        Assert.Null(manager.FindRecipe(10));
        Assert.Equal(0, manager.AddIngredientsToShoppingList(10));
        Assert.Equal(new[] { "1 apple", "2 cups flour" }, manager.GetShoppingList());
        Assert.Equal(2, manager.ShoppingItemCount);
        Assert.Equal(2, manager.RecipeCount);
    }

    private static IRecipeManager CreateManager()
    {
        List<Recipe> recipes = new List<Recipe>();

        Recipe first = new Recipe();
        first.Id = 10;
        first.Title = "Recipe A";
        first.Ingredients.Add("1 apple");
        first.Ingredients.Add("2 cups flour");
        recipes.Add(first);

        Recipe second = new Recipe();
        second.Id = 20;
        second.Title = "Recipe B";
        second.Ingredients.Add("1 egg");
        recipes.Add(second);

        Recipe third = new Recipe();
        third.Id = 30;
        third.Title = "Recipe without ingredients";
        recipes.Add(third);

        return new RecipeManager(recipes);
    }
}
