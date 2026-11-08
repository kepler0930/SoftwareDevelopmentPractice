# Recipe Management System

Part A implementation based on the supplied starter repository. The Application
menu, recipe models, public interface and JSON loader are supplied.

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
Options 7, 8 and 11 add, remove and display recipes in the cooking plan.
Options 9 and 10 inspect the most recent removal and restore it to the plan.
Options 12–14 start a cooking session, peek at the next step and complete it.
Part B options 15–21 still print a **Not implemented** message.

## Part A implementation

The recipe catalogue uses `Dictionary<int, Recipe>` inside `RecipeManager`.
The constructor checks the initial recipes, and the catalogue supports add,
find and remove operations. Recipe IDs must be positive and unique, and titles
must not be blank. Duplicate additions do not replace existing recipes.

The shopping list uses `List<string>`. Adding a recipe appends its original
ingredient strings in order, including repeated ingredients. Reading returns a
copy of the list. Clearing removes the shopping items without changing recipes.
No quantity parsing or ingredient merging is performed.

The cooking plan uses `LinkedList<int>` to keep recipe IDs in the order added.
Only existing recipes can be added, and duplicate IDs are rejected. Successful
removals push the ID onto the history stack; failed removals leave the history
unchanged. Reading the plan returns a copy. Planned recipes cannot be removed
from the catalogue.

The removed-recipe history uses `Stack<int>`. Peeking reads the top ID without
removing it. Each restore attempt pops one ID and appends that recipe to the
plan if it still exists and is not already planned. An invalid entry is consumed
without trying an older entry in the same call. Empty history returns no ID
when peeking and `false` when restoring.

The active cooking session uses `Queue<string>`. Starting an existing recipe
with instructions replaces the queue with the original strings in order.
A missing recipe or one with no instructions returns `false` and leaves the
current queue unchanged. Peeking leaves the next instruction in place;
completing removes exactly one instruction. Both return `null` for an empty
queue, and the pending count reflects the number of remaining instructions.

## Validation

Run all Part A tests:

```bash
dotnet test
```

The solution builds with no warnings or errors. All 28 catalogue tests, 15
shopping-list tests, 19 cooking-plan tests, 13 removed-history tests and 15
instruction-queue tests pass, including all three supplied examples.
The full suite reports 90 passing tests, no failures and no skipped tests.
The workflow test checks a recipe across the catalogue, shopping list,
plan, removal history and instruction queue.

Console checks loaded all 670 supplied recipes and verified menu options 1–3:
find an existing recipe, add and find a new recipe, reject a duplicate ID,
handle a missing ID and remove the added recipe.

Shopping-list console checks verified options 4–6: add and display the original
ingredient lines, leave existing items unchanged for a missing recipe, clear
the list and add ingredients again after clearing.

Cooking-plan console checks verified options 7, 8 and 11: display an empty
plan, preserve insertion order, reject duplicate or missing IDs, remove a
planned recipe and handle repeated removal safely. Catalogue deletion is
blocked while the recipe is planned and succeeds after plan removal.

Removed-history console checks verified options 9 and 10 with an empty history,
then removed recipe 1 followed by recipe 2. Peeking showed recipe 2 first,
and successive restores produced plan order 2, 1. Peeking after both restores
reported an empty history.

Instruction-queue console checks verified options 12–14: empty handling,
repeated peeking without removal, completing one step at a time, preserving
the current session for a missing recipe, and replacing it with another valid
recipe. Completing all steps leaves an empty queue.

## Submission

Repository: [SoftwareDevelopmentPractice](https://github.com/kepler0930/SoftwareDevelopmentPractice)

The final Part A version is identified by Git tag `part-a-submission`.
The submission ZIP contains the solution, source, tests, supplied data and
documentation. Local tools, build output and Git metadata are excluded;
development history remains available in the repository.

Part B implementation and the Part B write-up are for the later assignment.
The supplied Part B placeholders remain in this project.

## Local SDK

This workspace has a local SDK in `.local/dotnet`, excluded from Git. In
PowerShell, it can be used directly:

```powershell
& .\.local\dotnet\dotnet.exe build
& .\.local\dotnet\dotnet.exe test
& .\.local\dotnet\dotnet.exe run --project RecipeManagement.Application -- data/recipes.json
```

Run the console application in an interactive terminal; the supplied menu uses
`Console.Clear()`.
