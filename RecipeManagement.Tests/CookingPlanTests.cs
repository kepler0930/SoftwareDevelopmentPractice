using RecipeManagement.Core;

namespace RecipeManagement.Tests;

[Trait("Part", "A.2")]
[Trait("Feature", "CookingPlan")]
public sealed class CookingPlanTests
{
    [Fact]
    public void NewManager_HasEmptyCookingPlanAndRemovalHistory()
    {
        IRecipeManager manager = CreateManager();

        Assert.Empty(manager.GetCookingPlan());
        Assert.Equal(0, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void AddRecipeToCookingPlan_AppendsRecipesInAddedOrder()
    {
        IRecipeManager manager = CreateManager();

        Assert.True(manager.AddRecipeToCookingPlan(30));
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.True(manager.AddRecipeToCookingPlan(20));

        Assert.Equal(new[] { 30, 10, 20 }, manager.GetCookingPlan());
        Assert.Equal(3, manager.CookingPlanCount);
        Assert.Equal(3, manager.RecipeCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void AddRecipeToCookingPlan_WithDuplicateId_DoesNotChangePlan()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);

        Assert.False(manager.AddRecipeToCookingPlan(10));
        Assert.Equal(new[] { 10, 20 }, manager.GetCookingPlan());
        Assert.Equal(2, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    public void AddRecipeToCookingPlan_WithMissingId_DoesNotChangePlan(int recipeId)
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);

        Assert.False(manager.AddRecipeToCookingPlan(recipeId));
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RemoveRecipeFromCookingPlan_RemovesMiddleRecipeAndRecordsHistory()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.AddRecipeToCookingPlan(30);

        Assert.True(manager.RemoveRecipeFromCookingPlan(20));
        Assert.Equal(new[] { 10, 30 }, manager.GetCookingPlan());
        Assert.Equal(2, manager.CookingPlanCount);
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.NotNull(manager.FindRecipe(20));
        Assert.Equal(3, manager.RecipeCount);
    }

    [Theory]
    [InlineData(10, 20)]
    [InlineData(20, 10)]
    public void RemoveRecipeFromCookingPlan_CanRemoveFirstOrLastRecipe(int removedId, int remainingId)
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);

        Assert.True(manager.RemoveRecipeFromCookingPlan(removedId));
        Assert.Equal(new[] { remainingId }, manager.GetCookingPlan());
        Assert.Equal(1, manager.CookingPlanCount);
        Assert.Equal(1, manager.RemovedRecipeCount);
    }

    [Theory]
    [InlineData(20)]
    [InlineData(999)]
    public void RemoveRecipeFromCookingPlan_WithUnplannedId_DoesNotChangePlanOrHistory(int recipeId)
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(30);
        manager.RemoveRecipeFromCookingPlan(30);

        Assert.False(manager.RemoveRecipeFromCookingPlan(recipeId));
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.CookingPlanCount);
        Assert.Equal(1, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RemoveRecipeFromCookingPlan_WithEmptyPlan_ReturnsFalse()
    {
        IRecipeManager manager = CreateManager();

        Assert.False(manager.RemoveRecipeFromCookingPlan(10));
        Assert.Empty(manager.GetCookingPlan());
        Assert.Equal(0, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RemoveRecipeFromCookingPlan_Twice_RecordsOnlySuccessfulRemoval()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);

        Assert.True(manager.RemoveRecipeFromCookingPlan(10));
        Assert.False(manager.RemoveRecipeFromCookingPlan(10));
        Assert.Empty(manager.GetCookingPlan());
        Assert.Equal(0, manager.CookingPlanCount);
        Assert.Equal(1, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RemovedRecipe_CanBeAddedAgainAtEndOfPlan()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.Equal(new[] { 20, 10 }, manager.GetCookingPlan());
        Assert.Equal(2, manager.CookingPlanCount);
        Assert.Equal(1, manager.RemovedRecipeCount);
    }

    [Fact]
    public void GetCookingPlan_ReturnsCopyThatCannotChangeInternalPlan()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);

        IList<int> returnedPlan = (IList<int>)manager.GetCookingPlan();
        try
        {
            returnedPlan[0] = 999;
        }
        catch (NotSupportedException)
        {
            // A read-only result also protects the internal plan.
        }

        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.CookingPlanCount);
    }

    [Fact]
    public void GetCookingPlan_CopyDoesNotChangeAfterAddingOrRemovingRecipes()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        IReadOnlyList<int> previousPlan = manager.GetCookingPlan();

        manager.AddRecipeToCookingPlan(20);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.Equal(new[] { 10 }, previousPlan);
        Assert.Equal(new[] { 20 }, manager.GetCookingPlan());
    }

    [Fact]
    public void RemoveRecipe_WithPlannedRecipe_KeepsCatalogueAndPlanUnchanged()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);

        Assert.False(manager.RemoveRecipe(10));
        Assert.NotNull(manager.FindRecipe(10));
        Assert.Equal(3, manager.RecipeCount);
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RecipeRemovedFromPlan_CanThenBeRemovedFromCatalogue()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);

        Assert.True(manager.RemoveRecipeFromCookingPlan(10));
        Assert.True(manager.RemoveRecipe(10));
        Assert.Null(manager.FindRecipe(10));
        Assert.Equal(2, manager.RecipeCount);
        Assert.Empty(manager.GetCookingPlan());
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.False(manager.AddRecipeToCookingPlan(10));
    }

    [Fact]
    public void CookingPlanChanges_DoNotRemoveCopiedShoppingItems()
    {
        IRecipeManager manager = CreateManager();
        Assert.Equal(1, manager.AddIngredientsToShoppingList(10));
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.Equal(new[] { "1 apple" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
        Assert.Empty(manager.GetCookingPlan());
        Assert.Equal(1, manager.RemovedRecipeCount);
    }

    private static IRecipeManager CreateManager()
    {
        List<Recipe> recipes = new List<Recipe>();

        Recipe first = new Recipe();
        first.Id = 10;
        first.Title = "Recipe A";
        first.Ingredients.Add("1 apple");
        recipes.Add(first);

        Recipe second = new Recipe();
        second.Id = 20;
        second.Title = "Recipe B";
        recipes.Add(second);

        Recipe third = new Recipe();
        third.Id = 30;
        third.Title = "Recipe C";
        recipes.Add(third);

        return new RecipeManager(recipes);
    }
}
