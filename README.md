# ICD0008 Programming in C# — Gomoku

| | |
| --- | --- |
| Name | Albert Piikov |
| Student code | 233436IADB |
| School e-mail | alpiik@taltech.ee |
| Uni-ID | alpiik |
| Assigned game | **Gomoku** (board 9–19, rectangles allowed, win length 4–6, overline rule toggle, optional pro opening) |

## Build, run, test

```bash
dotnet build
dotnet test
dotnet run --project MenuDemo     # A1 menu library demo
dotnet run --project ConsoleUI    # the game (from A2)
```

Run the console apps in a real terminal such as Windows Terminal. The menu and the board clear the screen before every redraw, and Rider's Run window can draw that wrongly: the new text lands on top of the old menu. The key that stops an AI vs AI game (A5) is read straight from the keyboard, so it never fires when the input is piped in (`dotnet run --project ConsoleUI < moves.txt`). To debug, start the app in the terminal and attach Rider to it (*Run → Attach to Process*).

## Solution structure

| Project | Purpose |
| --- | --- |
| `MenuSystem` | A1 — reusable console menu library, knows nothing about the game |
| `MenuDemo` | A1 — small console app that demonstrates every menu feature |
| `GameEngine` | A2 — Gomoku rules, configurations, presets, game state, repository and move provider interfaces; A5 — AI players in `Ai/`; no console or file code |
| `DAL.Json` | A3 — saves configurations and games as JSON files in the user's home folder |
| `DAL.EF` | A4 — saves the same data in a SQLite database with EF Core; `Migrations/` folder |
| `ConsoleUI` | the Gomoku console app: board drawing, keyboard input, menus; the composition root (`Program.cs`) |
| `Tests` | xUnit tests for the menu, the game rules, both storages (contract tests) and the AI players |

## A1 — Menu system

- A menu is built from `MenuItem`s (shortcut, title, action as `Func<string>`). A submenu is an item whose action is `subMenu.Run`, so nesting depth is unlimited.
- Mandatory items are added automatically by `EMenuLevel`:

  | Level | Mandatory items |
  | --- | --- |
  | `Main` | `X) Exit` |
  | `Second` | `B) Return to previous`, `X) Exit` |
  | `Deeper` (third level and below) | `B) Return to previous`, `M) Return to main`, `X) Exit` |

- **Reserved shortcuts:** `X`, `B` and `M` (constants in `MenuShortcuts`) are reserved on the levels where the matching mandatory item exists. `AddItem` throws `ArgumentException` for a duplicate shortcut, including a clash with a mandatory item. Shortcuts are case-insensitive.
- Actions return a `MenuResult`: `Exit` closes every menu, `ReturnToMain` closes every menu above the main menu (without re-running anything in between), `Back` closes the current menu, anything else redraws the same menu.
- Labels can change: set `MenuItem.Title`, or give the item a `TitleProvider` that is evaluated on every redraw.
- Input is typed and validated: an unknown shortcut shows a message and asks again.

## A2 — Console game

### How to play

1. `dotnet run --project ConsoleUI`, then `N) New game`, pick a configuration by its number, choose who plays X and O (see A5) and name the game (Enter keeps the suggested name).
2. Type a move as `row,col` (or `row col`) using the numbers drawn around the board, starting from 1. Example: `8,8`.
3. `Q` leaves the game and returns to the menu. The game is saved after every move, so `L) Load a saved game` continues it later.

The last move is shown in brackets, e.g. `[X]`. An illegal move (taken cell, outside the board, pro opening rule) shows the reason and the same player moves again.

### Configurations

`C) Configurations` lists, creates, edits and deletes configurations. Every value is asked and checked (board 9–19, win length 4–6); an existing name cannot be reused (names ignore upper/lower case). Names may contain letters, digits, spaces, `-` and `_`. The four presets cannot be edited or deleted.

| Preset | Board | Win | Overline | Pro opening |
| --- | --- | --- | --- | --- |
| Free-style | 15×15 | 5 | longer lines also win | no |
| Standard | 15×15 | 5 | only exactly 5 wins | no |
| Small | 9×9 | 4 | longer lines also win | no |
| Pro | 19×19 | 5 | longer lines also win | yes |

### Gomoku extensions and where they are implemented

| Extension | Where |
| --- | --- |
| Rectangular boards (width and height set separately) | `GameConfiguration.BoardWidth` / `BoardHeight`, board is `EGamePiece[row][col]` in `GameState` |
| Win length 4–6 | `GameConfiguration.WinLength`, `GameBrain.CheckWin` counts in 4 directions from the last move |
| Overline rule toggle | `GameConfiguration.ExactWinLength`, `GameBrain.IsWinningLength` |
| Pro opening (first move in the centre, X's second move at least 3 cells from the centre) | `GameConfiguration.ProOpening`, `GameBrain.GetProOpeningError` |

### Design

- `GameBrain` is the only place that knows the rules: legal moves, making a move, the game status. The console only draws (`BoardView`) and reads input (`ConsoleMoveProvider`).
- `GameRunner` asks an `IMoveProvider` for each move, so AI players can be plugged in later without changing the loop.
- The game status is never stored; it is calculated from the board, so it can't get out of sync.
- Configurations are behind `IConfigRepository` and games behind `IGameRepository` (see A3).

## A3 — JSON persistence

### Where the files are

`~/.icd0008/gomoku/` — on Windows `C:\Users\<you>\.icd0008\gomoku\`. The folders are created on first use.

```text
~/.icd0008/gomoku/
├── configs/
│   ├── free-style.config.json     {name in lower case}.config.json
│   └── ...
└── games/
    └── 3f2b...-....game.json      {game id}.game.json
```

### Menus

| Menu | What it does |
| --- | --- |
| `N) New game` | choose a configuration, the players (A5), name the game, play. Autosaved after every move |
| `L) Load a saved game` | pick from the saved games (name, configuration, players, time of last save, newest first) and continue |
| `S) Saved games` | list and delete saved games |
| `C) Configurations` | list, create, edit and delete configurations (A2), now stored as files |

### Design decisions

- **Interfaces in the engine.** `IConfigRepository` and `IGameRepository` are in `GameEngine`; `DAL.Json` implements them. `ConfigurationUi`, `GamesUi` and `GameRunner` only see the interfaces. `Program.cs` is the only file that names `ConfigRepositoryJson` / `GameRepositoryJson`.
- **Same contract for every implementation** (written in the interfaces' XML comments): `List` is sorted (configs alphabetically, games newest first), `Get` throws `KeyNotFoundException` when missing, `Save` inserts or overwrites (never duplicates), `Delete` does nothing when missing.
- **Keys.** A configuration's key is its name, compared ignoring case. The file name is the name in lower case, so `Small` and `small` are the same file on Windows, macOS and Linux. Only letters, digits, spaces, `-` and `_` are allowed in names (`GameConfiguration.IsValidName`), so every name is a legal file name on every OS and can't point outside the folder. A game's key is its `Guid`.
- **What a game file stores:** a format version and the `GameState`: id, name, a copy of the configuration (all extension fields), board, whose turn, who plays X and O (since A5), move history, creation time and time of the last save. The game status is not stored; it is calculated from the board. A saved game keeps its own copy of the configuration, so editing a configuration never breaks a saved game.
- **Presets** are seeded by `Presets.SeedMissing` at every start: on the first run all four are created, and a preset file removed by hand comes back on the next start. The UI does not allow editing or deleting presets.
- **One shared `JsonSerializerOptions`** (`GameStateJson.Options` in `GameEngine`, since A4 also used by the database): indented, enums as text (`"X"`, never numbers), `null` and missing values are errors instead of silent defaults.
- **Safe writing.** Files are written to `*.tmp` and then renamed (`FileHelpers.WriteAtomic`), so a crash never leaves half a file.
- **Damaged files are reported, not hidden.** A file that can't be read, breaks the rules (`GameState.Validate`, `GameConfiguration.Validate`) or comes from a newer format version throws an exception with the file name. `StorageErrors.Guard` turns those storage errors into a message in the menu instead of crashing the program; other exceptions (real bugs) are not caught.
- **Format version** (`GameStateJson.CurrentVersion`). When the layout changes, the version goes up and the reader still accepts older text. Version 1 (A3, A4) has no player types; version 2 (A5) adds them, and a version 1 file loads as human vs human.

## A4 — EF Core database and dependency injection

### Switching the storage

One value in [`ConsoleUI/appsettings.json`](ConsoleUI/appsettings.json):

```json
{ "Persistence": "Db" }      // or "Json"
```

An environment variable with the same name overrides it for one run, nothing to edit:

```powershell
$env:Persistence = "Json"; dotnet run --project ConsoleUI   # PowerShell
```

```bash
Persistence=Json dotnet run --project ConsoleUI              # bash
```

The main menu title shows which storage is used: `Gomoku (saving to database)` or `Gomoku (saving to JSON files)`. The two storages are separate: a game saved in one is not in the other.

### The database

- SQLite file `~/.icd0008/gomoku/gomoku.db` (Windows: `C:\Users\<you>\.icd0008\gomoku\gomoku.db`), built from the user's home folder (`DbPaths`), never a path from one laptop. To look inside: Rider → Database → `+` → Data Source → SQLite → choose the file.
- `Database.Migrate()` runs at startup: a fresh clone creates the database and its tables by itself, no `dotnet ef` commands needed.
- Tables, from the migrations `InitialCreate` (A4) and `AddPlayerTypes` (A5):

  | Table | Columns |
  | --- | --- |
  | `Configurations` | `Id` (key), `Name` (unique, ignores upper/lower case), `BoardWidth`, `BoardHeight`, `WinLength`, `ExactWinLength`, `ProOpening` |
  | `SavedGames` | `Id` (the engine's `Guid`), `Name`, `SavedAtUtc`, `PlayerX`, `PlayerO` (as text, e.g. `RandomAi`), `StateJson`, `ConfigurationId` (foreign key → `Configurations`) |

- **Relationship:** a configuration has many saved games, each saved game belongs to one configuration. Delete behaviour is `Restrict`: the database refuses to delete a configuration that saved games use (`Cascade` would silently delete the games). The configuration screen checks this first and tells the user to delete those games, so both storages behave the same.
- **`StateJson`** holds the whole game, written by the same code as the JSON files (`GameStateJson`), so a file and a row contain exactly the same text and the game keeps its `Guid` in both. The other columns exist so the saved games list is one small query (with a join for the configuration name) that never reads the JSON.

### Creating a migration (only when the entities change)

```bash
dotnet tool install --global dotnet-ef --version 10.0.14   # once; same major version as the EF Core packages
dotnet ef migrations add <WhatChanged> --project DAL.EF --startup-project ConsoleUI --output-dir Migrations
```

The program applies new migrations itself on the next start.

### Design decisions

- **Same interfaces, same behaviour.** `ConfigRepositoryEf` and `GameRepositoryEf` implement `IConfigRepository` / `IGameRepository` exactly like the JSON versions: same sort order (configurations are sorted in C# with the same comparer, because SQLite's own case-insensitive order puts `_` before letters), same `KeyNotFoundException`, same insert-or-overwrite `Save`, same silent `Delete`.
- **Contract tests.** `ConfigRepositoryContractTests` and `GameRepositoryContractTests` use `[Theory]` + `[MemberData]`: every test runs once with `"Json"` (a temporary folder) and once with `"Db"` (in-memory SQLite with the real migration applied). Things only one storage has are tested in `JsonStorageTests` and `DbStorageTests`.
- **Dependency injection** (`Microsoft.Extensions.DependencyInjection`) in `Program.cs`, the only place that names concrete classes. The `switch` on `Persistence` registers either the JSON or the EF repositories; `ConfigurationUi`, `GamesUi` and the move provider factory get theirs through their constructors. The `DbContext` and EF repositories are *scoped* (never singletons); `ValidateScopes` and `ValidateOnBuild` report a wrong lifetime at startup.
- **One unit of work per repository method:** one `SaveChanges`, then the change tracker is cleared (`SaveChangesAndForget`), so a refused operation can't stay queued and break the next one. Read-only queries use `AsNoTracking`.
- **Entities are not domain objects.** `Configuration` and `SavedGame` are table rows with settable properties; the engine's `GameConfiguration` record and `GameState` know nothing about EF. The repositories map between them.
- **UTC times.** SQLite stores dates as text and forgets they are UTC; a value converter on `SavedAtUtc` puts the `Kind` back, so the list shows the right local time.

## A5 — Play modes and the random AI

### Starting and stopping each mode

`N) New game` → configuration → `Player X:` and `Player O:` (1 Human, 2 Random AI, 3 Greedy AI) → game name.

| Mode | Choose | How to stop |
| --- | --- | --- |
| Human vs human | Human, Human | `Q` at the move prompt |
| Human vs AI | one Human, one AI (either side; an AI as X opens the game) | `Q` at your move prompt. The AI answers right after your move |
| AI vs AI | two AIs | **any key** (in a real terminal, see "Build, run, test"). There is a half-second pause before every move so the game can be watched |

The game is saved after every move in every mode, so a stopped or left game continues with `L) Load a saved game`, and the AI keeps playing. The board header (`X: Random AI   O: Greedy AI`) and the saved games list show who plays.

### The AI players

- **Random AI** (`GameEngine/Ai/RandomAiMoveProvider`): a uniformly random move from `GameBrain.GetLegalMoves()`, so it follows the pro opening rule and never plays a taken cell.
- **Greedy AI** (bonus, `GreedyAiMoveProvider`): asks `GameBrain.WouldWin(row, col)` about every legal move, which checks the line without placing the piece. It takes a win when there is one, otherwise it plays like the random AI.
- **Neither changes the state.** They read it and return a `Move`; only the game loop calls `MakeMove`.
- **One `Random` for the whole run**, registered in dependency injection (`Random.Shared`) and passed to the constructors; never `new Random()` per move. The tests pass `new Random(seed)` and get the same game every time.

### Design decisions

- **The game loop doesn't know who plays.** `GameRunner.RunAsync` gets one `IMoveProvider` for X and one for O and calls `GetMoveAsync`; there is no `if` on the player type. `MoveProviderFactory.Create(EPlayerType)` (ConsoleUI) is the only place that turns a type into a class; `GamesUi` uses it when a game starts or is loaded.
- **Async with a cancellation token** (lecture 10.2): `Task<Move?> GetMoveAsync(GameState, CancellationToken)`. While an AI move or the pause is pending, `GameRunner` looks at `Console.KeyAvailable` every 50 ms. A key cancels the token; the AI or the `Task.Delay` then throws `OperationCanceledException`, and the loop shows "Stopped" and returns to the menu. The human's provider returns an already finished task (`Console.ReadLine` can't be cancelled), so the loop never takes the human's keystrokes.
- **The pause** (500 ms) is only used for AI vs AI. `GamesUi` decides it and passes it to the loop as a `TimeSpan`.
- **Player types are saved:** `GameState.PlayerX` / `PlayerO`, JSON format version 2, and the `SavedGames.PlayerX` / `PlayerO` columns (migration `AddPlayerTypes`). Old saves load as human vs human. EF suggested `""` as the default value for existing rows; that is not a player type, so it was changed by hand to `"Human"` (`DbStorageTests` checks the real column value).
- **Synchronous menu, asynchronous game:** the menu library stays synchronous, and `GamesUi.Play` waits for the game with `GetAwaiter().GetResult()`. A console app has no `SynchronizationContext`, so this can't deadlock.
- **Where minimax (A6) plugs in:** a new `IMoveProvider` in `GameEngine/Ai/` that reads the state like the random AI and checks the token while it searches, a new `EPlayerType` value, and one more line in `MoveProviderFactory`. `GameRunner` stays the same, and the stop key already works for an AI that thinks for a long time.

### Tests

- `RandomAiMoveProviderTests` and `GreedyAiMoveProviderTests`: for every preset with 4 seeds, a full AI vs AI game where every move is checked against `GetLegalMoves` and the game must end within width × height moves. The same seed gives the same game; the state is unchanged after `GetMoveAsync`; a cancelled token throws; the greedy AI takes a win (also as O, and not an overline under the exact rule).
- `WouldWinTests`; player types in the repository contract tests; a version 1 JSON text; an A4 database upgraded by the migration.

## AI usage log

| Date | What AI was used for | What it got wrong | What I changed |
| --- | --- | --- | --- |
| 2026-11-06 | Claude Code read the course lectures and the teacher's demo, then wrote the A1 menu library, demo app and menu tests step by step with explanations | TODO | TODO |
| 2026-11-06 | Claude Code wrote the A2 game engine (rules, overline, pro opening), engine tests, the console board, move input and configuration screens | TODO | TODO |
| 2026-11-06 | Claude Code wrote the A3 JSON storage (`DAL.Json`), the game repository interface, autosave, saved games screens and the storage tests | TODO | TODO |
| 2026-11-06 | Claude Code wrote the A4 EF Core storage (`DAL.EF`, migration), dependency injection with the `appsettings.json` switch, and turned the storage tests into contract tests for both storages | TODO | TODO |
| 2026-11-06 | Claude Code wrote the A5 player types, the async move provider with the stop key, the random and greedy AI, the `AddPlayerTypes` migration and the AI tests | TODO | TODO |
