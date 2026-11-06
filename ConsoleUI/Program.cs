using ConsoleUI;
using GameEngine;
using MenuSystem;

// composition root: the only place that decides which classes are used
IConfigRepository configRepository = new ConfigRepositoryInMemory();
var configUi = new ConfigurationUi(configRepository);
IMoveProvider human = new ConsoleMoveProvider();

var configMenu = new Menu("Gomoku - configurations", EMenuLevel.Second)
    .AddItem("L", "List configurations", configUi.ListAll)
    .AddItem("C", "Create a configuration", configUi.Create)
    .AddItem("E", "Edit a configuration", configUi.Edit)
    .AddItem("D", "Delete a configuration", configUi.Delete);

var mainMenu = new Menu("Gomoku", EMenuLevel.Main)
    .AddItem("N", "New game (human vs human)", NewGame)
    .AddItem("C", "Configurations", configMenu.Run);

mainMenu.Run();
Console.WriteLine("Bye!");
return;

string NewGame()
{
    var config = configUi.Choose("New game - choose a configuration");
    return config is null
        ? MenuResult.None
        : GameRunner.Run(GameState.New(config), human, human);
}
