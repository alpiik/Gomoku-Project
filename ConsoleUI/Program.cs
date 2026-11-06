using ConsoleUI;
using DAL.Json;
using GameEngine;
using MenuSystem;
using static ConsoleUI.StorageErrors;

// composition root: the only place that decides which classes are used.
// Everything below these lines sees only the interfaces.
IConfigRepository configRepository = new ConfigRepositoryJson();
IGameRepository gameRepository = new GameRepositoryJson();
Presets.SeedMissing(configRepository);

IMoveProvider human = new ConsoleMoveProvider();
var configUi = new ConfigurationUi(configRepository);
var gamesUi = new GamesUi(configUi, gameRepository, human);

var configMenu = new Menu("Gomoku - configurations", EMenuLevel.Second)
    .AddItem("L", "List configurations", Guard(configUi.ListAll))
    .AddItem("C", "Create a configuration", Guard(configUi.Create))
    .AddItem("E", "Edit a configuration", Guard(configUi.Edit))
    .AddItem("D", "Delete a configuration", Guard(configUi.Delete));

var savedGamesMenu = new Menu("Gomoku - saved games", EMenuLevel.Second)
    .AddItem("L", "List saved games", Guard(gamesUi.ListAll))
    .AddItem("D", "Delete a saved game", Guard(gamesUi.Delete));

var mainMenu = new Menu("Gomoku", EMenuLevel.Main)
    .AddItem("N", "New game (human vs human)", Guard(gamesUi.NewGame))
    .AddItem("L", "Load a saved game", Guard(gamesUi.Continue))
    .AddItem("S", "Saved games", savedGamesMenu.Run)
    .AddItem("C", "Configurations", configMenu.Run);

mainMenu.Run();
Console.WriteLine("Bye!");
