using RecipeManagement.Core;

namespace RecipeManagement.Tests;

[Trait("Part", "A.2")]
[Trait("Feature", "RemovedHistory")]
public sealed class RemovedRecipeHistoryTests
{
    [Fact]
    public void PeekLastRemovedRecipe_WithEmptyHistory_ReturnsNull()
    {
        IRecipeManager manager = CreateManager();

        Assert.Null(manager.PeekLastRemovedRecipe());
        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Empty(manager.GetCookingPlan());
    }

    [Fact]
    public void RestoreLastRemovedRecipe_WithEmptyHistory_DoesNotChangePlan()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);

        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void PeekLastRemovedRecipe_DoesNotRemoveHistoryEntry()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.Equal(10, manager.PeekLastRemovedRecipe());
        Assert.Equal(10, manager.PeekLastRemovedRecipe());
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Empty(manager.GetCookingPlan());
    }

    [Fact]
    public void RestoreLastRemovedRecipe_AppendsRecipeToEndOfPlan()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.AddRecipeToCookingPlan(30);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 20, 30, 10 }, manager.GetCookingPlan());
        Assert.Equal(3, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Null(manager.PeekLastRemovedRecipe());
        Assert.Equal(3, manager.RecipeCount);
    }

    [Fact]
    public void RestoreLastRemovedRecipe_UsesLastInFirstOutOrder()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.AddRecipeToCookingPlan(30);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(20);

        Assert.Equal(20, manager.PeekLastRemovedRecipe());
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 30, 20 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Equal(10, manager.PeekLastRemovedRecipe());

        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 30, 20, 10 }, manager.GetCookingPlan());
        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Null(manager.PeekLastRemovedRecipe());
        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Equal(3, manager.CookingPlanCount);
    }

    [Fact]
    public void FailedPlanRemoval_DoesNotChangeLastRemovedRecipe()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.False(manager.RemoveRecipeFromCookingPlan(20));
        Assert.False(manager.RemoveRecipeFromCookingPlan(999));
        Assert.Equal(10, manager.PeekLastRemovedRecipe());
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
    }

    [Fact]
    public void RestoreLastRemovedRecipe_WithDeletedRecipe_DiscardsHistoryEntry()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);
        Assert.True(manager.RemoveRecipe(10));

        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Null(manager.PeekLastRemovedRecipe());
        Assert.Empty(manager.GetCookingPlan());
        Assert.Equal(0, manager.CookingPlanCount);
        Assert.Null(manager.FindRecipe(10));
        Assert.Equal(2, manager.RecipeCount);
    }

    [Fact]
    public void RestoreLastRemovedRecipe_WithAlreadyPlannedRecipe_DoesNotAddDuplicate()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.AddRecipeToCookingPlan(10);

        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 20, 10 }, manager.GetCookingPlan());
        Assert.Equal(2, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Null(manager.PeekLastRemovedRecipe());
    }

    [Fact]
    public void RestoreLastRemovedRecipe_WithInvalidTopEntry_DoesNotSkipToOlderEntry()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.AddRecipeToCookingPlan(20);
        manager.AddRecipeToCookingPlan(30);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(20);
        manager.RemoveRecipe(20);

        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 30 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Equal(10, manager.PeekLastRemovedRecipe());

        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 30, 10 }, manager.GetCookingPlan());
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void SameRecipeRemovedTwice_CannotBeRestoredTwiceIntoPlan()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.Equal(2, manager.RemovedRecipeCount);
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(1, manager.RemovedRecipeCount);
        Assert.Equal(10, manager.PeekLastRemovedRecipe());

        Assert.False(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(1, manager.CookingPlanCount);
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RestoredRecipe_IsProtectedFromCatalogueDeletionWhilePlanned()
    {
        IRecipeManager manager = CreateManager();
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);
        manager.RestoreLastRemovedRecipe();

        Assert.False(manager.RemoveRecipe(10));
        Assert.NotNull(manager.FindRecipe(10));
        Assert.Equal(3, manager.RecipeCount);
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.Equal(0, manager.RemovedRecipeCount);
    }

    [Fact]
    public void RestoringRecipe_DoesNotChangeCopiedShoppingItems()
    {
        IRecipeManager manager = CreateManager();
        manager.AddIngredientsToShoppingList(10);
        manager.AddRecipeToCookingPlan(10);
        manager.RemoveRecipeFromCookingPlan(10);

        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { "1 apple" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
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
