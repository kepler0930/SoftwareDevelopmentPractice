# Recipe Management System — Student Starter

Starter repository for Parts A and B. Implement `RecipeManager` in Core; the Application menu and JSON loader are supplied.

## What is supplied

- `RecipeManagement.Core/Models/` — recipe model classes
- `RecipeManagement.Core/RecipeLoader.cs` — reads `data/recipes.json`
- `RecipeManagement.Core/IRecipeManager.cs` — public API
- `RecipeManagement.Application/` — console menu (options labelled PartA / PartB)
- `RecipeManagement.Tests/` — example tests
- `data/recipes.json` — recipe dataset

## What you implement

**Part A** — `RecipeManager.cs` using:

- `Dictionary<int, Recipe>`
- `List<string>`
- `LinkedList<int>`
- `Stack<int>`
- `Queue<string>`

**Part B** — LINQ searches, protein report, saved-recipe collection, `Design.md`, and more tests.

## Build and run

Install the .NET 10 SDK and open `RecipeManagement.sln` from the repository root:

```bash
dotnet build
dotnet test
dotnet run --project RecipeManagement.Application -- data/recipes.json
```

Console menu options 1–6 now support the recipe catalogue and shopping list.
Other menu options still print a **Not implemented** message.

## Current stage

The recipe catalogue uses `Dictionary<int, Recipe>` inside `RecipeManager`.
The constructor checks the initial recipes, and the catalogue supports add,
find and remove operations. Recipe IDs must be positive and unique, and titles
must not be blank. Duplicate additions do not replace existing recipes.

The shopping list uses `List<string>`. Adding a recipe appends its original
ingredient strings in order, including repeated ingredients. Reading returns a
copy of the list. Clearing removes the shopping items without changing recipes.
No quantity parsing or ingredient merging is performed.

Cooking-plan operations, history and the instruction queue will be implemented
in later steps. The cooking-plan field is reserved so catalogue removal can
reject a recipe that is in the plan once plan operations are added.

Run the completed Part A.1 tests:

```bash
dotnet test --filter "Part=A.1"
```

The solution builds with no warnings or errors. All 28 catalogue tests and 15
shopping-list tests pass, including the supplied constructor example. The full
suite currently reports 43 passing tests and two failing examples for the
unimplemented cooking-plan history and instruction-queue features. No tests
are skipped.

Console checks loaded all 670 supplied recipes and verified menu options 1–3:
find an existing recipe, add and find a new recipe, reject a duplicate ID,
handle a missing ID and remove the added recipe.

Shopping-list console checks verified options 4–6: add and display the original
ingredient lines, leave existing items unchanged for a missing recipe, clear
the list and add ingredients again after clearing.

Repository: [SoftwareDevelopmentPractice](https://github.com/kepler0930/SoftwareDevelopmentPractice)

This workspace has a local SDK in `.local/dotnet`, excluded from Git. In
PowerShell, it can be used directly:

```powershell
& .\.local\dotnet\dotnet.exe build
& .\.local\dotnet\dotnet.exe test --filter "Part=A.1"
& .\.local\dotnet\dotnet.exe run --project RecipeManagement.Application -- data/recipes.json
```

Run the console application in an interactive terminal; the supplied menu uses
`Console.Clear()`.
