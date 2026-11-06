# ICD0008 Programming in C# — Gomoku

| | |
| --- | --- |
| Name | TODO full name |
| Student code | TODO |
| School e-mail | TODO |
| Uni-ID | TODO |
| Assigned game | **Gomoku** (board 9–19, rectangles allowed, win length 4–6, overline rule toggle, optional pro opening) |

## Build, run, test

```bash
dotnet build
dotnet test
dotnet run --project MenuDemo     # A1 menu library demo
dotnet run --project ConsoleUI    # the game (from A2)
```

Run the console apps in a real terminal (or enable *Emulate terminal in output console* in Rider) — the menu clears the screen between redraws.

## Solution structure

| Project | Purpose |
| --- | --- |
| `MenuSystem` | A1 — reusable console menu library, knows nothing about the game |
| `MenuDemo` | A1 — small console app that demonstrates every menu feature |
| `GameEngine` | A2 — Gomoku rules, configurations, presets, game state; no console code |
| `ConsoleUI` | the Gomoku console app: board drawing, keyboard input, menus |
| `Tests` | xUnit tests for the menu and the game rules |

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

1. `dotnet run --project ConsoleUI`, then `N) New game` and pick a configuration by its number.
2. Type a move as `row,col` (or `row col`) using the numbers drawn around the board, starting from 1. Example: `8,8`.
3. `Q` leaves the game and returns to the menu.

The last move is shown in brackets, e.g. `[X]`. An illegal move (taken cell, outside the board, pro opening rule) shows the reason and the same player moves again.

### Configurations

`C) Configurations` lists, creates, edits and deletes configurations. Every value is asked and checked (board 9–19, win length 4–6); an existing name cannot be reused. The four presets cannot be edited or deleted.

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
- Configurations are behind `IConfigRepository`. In A2 they are kept in memory (`ConfigRepositoryInMemory`).

## AI usage log

| Date | What AI was used for | What it got wrong | What I changed |
| --- | --- | --- | --- |
| 2026-11-06 | Claude Code read the course lectures and the teacher's demo, then wrote the A1 menu library, demo app and menu tests step by step with explanations | TODO | TODO |
| 2026-11-06 | Claude Code wrote the A2 game engine (rules, overline, pro opening), engine tests, the console board, move input and configuration screens | TODO | TODO |
