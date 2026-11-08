# Recipe Management System

C# console application for Part A of Software Development Practice.
It supports a recipe catalogue, shopping list, cooking plan, removal history
and cooking instructions using the five required collections.

## Build and run

Requires the .NET 10 SDK. Run these commands from the project root:

```bash
dotnet build
dotnet test
dotnet run --project RecipeManagement.Application -- data/recipes.json
```

Use an interactive terminal to run the console menu.

## Tests

All 90 xUnit tests pass, covering the five collections and component interactions.

Part B features will be completed in the later assignment.
