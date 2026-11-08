using RecipeManagement.Core;

namespace RecipeManagement.Tests;

[Trait("Part", "A.2")]
[Trait("Feature", "InstructionQueue")]
public sealed class InstructionQueueTests
{
    [Fact]
    public void EmptyQueue_ReturnsNullForPeekAndComplete()
    {
        IRecipeManager manager = CreateManager();

        Assert.Null(manager.PeekNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void PeekNextInstruction_DoesNotRemoveFirstInstruction()
    {
        IRecipeManager manager = CreateManager();

        Assert.True(manager.StartCooking(10));
        Assert.Equal(3, manager.PendingInstructionCount);
        Assert.Equal("Mix.", manager.PeekNextInstruction());
        Assert.Equal("Mix.", manager.PeekNextInstruction());
        Assert.Equal(3, manager.PendingInstructionCount);
    }

    [Fact]
    public void CompleteNextInstruction_ReturnsEachInstructionInOriginalOrder()
    {
        IRecipeManager manager = CreateManager();
        manager.StartCooking(10);

        Assert.Equal("Mix.", manager.CompleteNextInstruction());
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("Bake.", manager.PeekNextInstruction());
        Assert.Equal("Bake.", manager.CompleteNextInstruction());
        Assert.Equal(1, manager.PendingInstructionCount);
        Assert.Equal("Serve.", manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
        Assert.Null(manager.PeekNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(999)]
    public void StartCooking_WithMissingRecipe_PreservesActiveQueue(int recipeId)
    {
        IRecipeManager manager = CreateManager();
        manager.StartCooking(10);
        manager.CompleteNextInstruction();

        Assert.False(manager.StartCooking(recipeId));
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("Bake.", manager.CompleteNextInstruction());
        Assert.Equal("Serve.", manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void StartCooking_WithNoInstructions_ReturnsFalseAndLeavesQueueEmpty()
    {
        IRecipeManager manager = CreateManager();

        Assert.False(manager.StartCooking(20));
        Assert.Equal(0, manager.PendingInstructionCount);
        Assert.Null(manager.PeekNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
    }

    [Fact]
    public void StartCooking_WithNoInstructions_PreservesActiveQueue()
    {
        IRecipeManager manager = CreateManager();
        manager.StartCooking(10);
        manager.CompleteNextInstruction();

        Assert.False(manager.StartCooking(20));
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("Bake.", manager.CompleteNextInstruction());
        Assert.Equal("Serve.", manager.CompleteNextInstruction());
    }

    [Fact]
    public void StartCooking_WithAnotherValidRecipe_ReplacesPendingInstructions()
    {
        IRecipeManager manager = CreateManager();
        manager.StartCooking(10);
        manager.CompleteNextInstruction();

        Assert.True(manager.StartCooking(30));
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("Pour.", manager.CompleteNextInstruction());
        Assert.Equal("Chill.", manager.CompleteNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void StartCooking_WithSameRecipe_RestartsFromFirstInstruction()
    {
        IRecipeManager manager = CreateManager();
        manager.StartCooking(10);
        manager.CompleteNextInstruction();

        Assert.True(manager.StartCooking(10));
        Assert.Equal(3, manager.PendingInstructionCount);
        Assert.Equal("Mix.", manager.PeekNextInstruction());
    }

    [Fact]
    public void StartCooking_AfterQueueIsCompleted_LoadsNewInstructions()
    {
        IRecipeManager manager = CreateManager();
        manager.StartCooking(30);
        manager.CompleteNextInstruction();
        manager.CompleteNextInstruction();

        Assert.True(manager.StartCooking(10));
        Assert.Equal(3, manager.PendingInstructionCount);
        Assert.Equal("Mix.", manager.PeekNextInstruction());
    }

    [Fact]
    public void StartCooking_CopiesOriginalStringsIncludingRepeatedAndBlankSteps()
    {
        Recipe recipe = new Recipe();
        recipe.Id = 40;
        recipe.Title = "Recipe D";
        recipe.Instructions.Add("  Stir gently.  ");
        recipe.Instructions.Add("Stir again.");
        recipe.Instructions.Add("Stir again.");
        recipe.Instructions.Add("");
        IRecipeManager manager = new RecipeManager(new[] { recipe });

        Assert.True(manager.StartCooking(40));
        recipe.Instructions.Clear();

        Assert.Equal(4, manager.PendingInstructionCount);
        Assert.Equal("  Stir gently.  ", manager.CompleteNextInstruction());
        Assert.Equal("Stir again.", manager.CompleteNextInstruction());
        Assert.Equal("Stir again.", manager.CompleteNextInstruction());
        Assert.Equal("", manager.CompleteNextInstruction());
        Assert.Null(manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    [Fact]
    public void CompletingInstructions_DoesNotRemoveOriginalRecipeInstructions()
    {
        IRecipeManager manager = CreateManager();
        manager.StartCooking(10);
        manager.CompleteNextInstruction();
        manager.CompleteNextInstruction();
        manager.CompleteNextInstruction();

        Recipe? recipe = manager.FindRecipe(10);
        Assert.NotNull(recipe);
        Assert.Equal(new[] { "Mix.", "Bake.", "Serve." }, recipe.Instructions);
        Assert.Equal(3, manager.RecipeCount);
    }

    [Fact]
    public void RecipeWorkflow_KeepsPlanHistoryShoppingAndQueueConsistent()
    {
        IRecipeManager manager = CreateManager();
        Assert.Equal(1, manager.AddIngredientsToShoppingList(10));
        Assert.True(manager.AddRecipeToCookingPlan(10));
        Assert.True(manager.StartCooking(10));
        Assert.Equal("Mix.", manager.CompleteNextInstruction());

        Assert.True(manager.RemoveRecipeFromCookingPlan(10));
        Assert.Equal(10, manager.PeekLastRemovedRecipe());
        Assert.True(manager.RestoreLastRemovedRecipe());
        Assert.Equal(new[] { 10 }, manager.GetCookingPlan());
        Assert.False(manager.RemoveRecipe(10));

        Assert.Equal(0, manager.RemovedRecipeCount);
        Assert.Equal(new[] { "1 apple" }, manager.GetShoppingList());
        Assert.Equal(1, manager.ShoppingItemCount);
        Assert.Equal(2, manager.PendingInstructionCount);
        Assert.Equal("Bake.", manager.CompleteNextInstruction());
        Assert.Equal("Serve.", manager.CompleteNextInstruction());
        Assert.Equal(0, manager.PendingInstructionCount);
    }

    private static IRecipeManager CreateManager()
    {
        List<Recipe> recipes = new List<Recipe>();

        Recipe first = new Recipe();
        first.Id = 10;
        first.Title = "Recipe A";
        first.Ingredients.Add("1 apple");
        first.Instructions.Add("Mix.");
        first.Instructions.Add("Bake.");
        first.Instructions.Add("Serve.");
        recipes.Add(first);

        Recipe second = new Recipe();
        second.Id = 20;
        second.Title = "Recipe B";
        recipes.Add(second);

        Recipe third = new Recipe();
        third.Id = 30;
        third.Title = "Recipe C";
        third.Instructions.Add("Pour.");
        third.Instructions.Add("Chill.");
        recipes.Add(third);

        return new RecipeManager(recipes);
    }
}
