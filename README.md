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
| `ConsoleUI` | the Gomoku console app |
| `Tests` | xUnit tests |

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

## AI usage log

| Date | What AI was used for | What it got wrong | What I changed |
| --- | --- | --- | --- |
| 2026-11-06 | Claude Code read the course lectures and the teacher's demo, then wrote the A1 menu library, demo app and menu tests step by step with explanations | TODO | TODO |
