using MenuSystem;

// Demo of the MenuSystem library: four levels of nesting, labels that change with state
// (TitleProvider) and a label that is changed from another menu (settable Title).

var darkTheme = false;
var counter = 0;

var greetingItem = new MenuItem("G", "Greeting", () => Pause("Hello! This item only prints a message."));

var level4 = new Menu("Level 4 - the bottom", EMenuLevel.Deeper)
    .AddItem("H", "Say hello", () => Pause("Hello from level 4. Try M to jump straight back to the main menu."));

var level3 = new Menu("Level 3 - advanced", EMenuLevel.Deeper)
    .AddItem(new MenuItem("C", "Counter", () =>
    {
        counter++;
        return MenuResult.None;
    })
    {
        TitleProvider = () => $"Increase counter (now {counter})"
    })
    .AddItem("D", "Go one level deeper", level4.Run);

var settings = new Menu("Level 2 - settings", EMenuLevel.Second)
    .AddItem(new MenuItem("T", "Theme", () =>
    {
        darkTheme = !darkTheme;
        return MenuResult.None;
    })
    {
        TitleProvider = () => $"Theme: {(darkTheme ? "dark" : "light")}"
    })
    .AddItem("R", "Rename the greeting item in the main menu", () =>
    {
        greetingItem.Title = "Greeting (renamed in settings)";
        return MenuResult.None;
    })
    .AddItem("A", "Advanced", level3.Run);

var mainMenu = new Menu("Menu demo", EMenuLevel.Main)
    .AddItem(greetingItem)
    .AddItem("S", "Settings", settings.Run);

mainMenu.Run();
Console.WriteLine();
Console.WriteLine("Bye!");

static string Pause(string message)
{
    Console.WriteLine(message);
    Console.Write("Press Enter to continue...");
    Console.ReadLine();
    return MenuResult.None;
}
