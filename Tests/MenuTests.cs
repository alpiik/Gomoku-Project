using MenuSystem;

namespace Tests;

public class MenuTests
{
    [Fact]
    public void AddItem_DuplicateShortcut_Throws()
    {
        var menu = new Menu("Test", EMenuLevel.Main).AddItem("A", "First", () => MenuResult.None);

        Assert.Throws<ArgumentException>(() => menu.AddItem("A", "Second", () => MenuResult.None));
    }

    [Fact]
    public void AddItem_ShortcutsDifferOnlyInCase_Throws()
    {
        var menu = new Menu("Test", EMenuLevel.Main).AddItem("n", "New", () => MenuResult.None);

        Assert.Throws<ArgumentException>(() => menu.AddItem("N", "Another new", () => MenuResult.None));
    }

    [Theory]
    [InlineData(EMenuLevel.Main, MenuShortcuts.Exit)]
    [InlineData(EMenuLevel.Second, MenuShortcuts.Back)]
    [InlineData(EMenuLevel.Deeper, MenuShortcuts.ReturnToMain)]
    public void AddItem_ClashWithMandatoryItem_Throws(EMenuLevel level, string shortcut)
    {
        var menu = new Menu("Test", level);

        Assert.Throws<ArgumentException>(() => menu.AddItem(shortcut, "Clash", () => MenuResult.None));
    }

    [Fact]
    public void AddItem_ShortcutOfMandatoryItemFromDeeperLevel_IsAllowedInMainMenu()
    {
        var menu = new Menu("Test", EMenuLevel.Main);

        menu.AddItem(MenuShortcuts.ReturnToMain, "Music", () => MenuResult.None);
    }

    [Fact]
    public void Run_ReturnToMainFromThirdLevel_ClosesEveryLevelInBetween()
    {
        var (main, counters) = BuildThreeLevels();

        // into level 2, into level 3, back to main, run the main-only item, exit
        RunWithInput(main, "S", "D", "M", "C", "X");

        Assert.Equal(1, counters.SecondOpened);
        Assert.Equal(1, counters.ThirdOpened);
        Assert.Equal(1, counters.MainCounter);   // "C" reached the main menu, so level 2 really closed
    }

    [Fact]
    public void Run_ExitFromThirdLevel_ClosesEveryMenu()
    {
        var (main, counters) = BuildThreeLevels();

        var result = RunWithInput(main, "S", "D", "X", "C");

        Assert.Equal(MenuResult.Exit, result);
        Assert.Equal(0, counters.MainCounter);   // "C" was never read
    }

    [Fact]
    public void Run_BackFromSecondLevel_ReturnsToMainMenu()
    {
        var (main, counters) = BuildThreeLevels();

        RunWithInput(main, "S", "B", "C", "X");

        Assert.Equal(1, counters.MainCounter);
    }

    [Fact]
    public void Run_UnknownShortcut_AsksAgain()
    {
        var (main, counters) = BuildThreeLevels();

        RunWithInput(main, "nonsense", "c", "X");

        Assert.Equal(1, counters.MainCounter);
    }

    private class Counters
    {
        public int MainCounter { get; set; }
        public int SecondOpened { get; set; }
        public int ThirdOpened { get; set; }
    }

    private static (Menu Main, Counters Counters) BuildThreeLevels()
    {
        var counters = new Counters();
        var third = new Menu("Third", EMenuLevel.Deeper);
        var second = new Menu("Second", EMenuLevel.Second)
            .AddItem("D", "Deeper", () =>
            {
                counters.ThirdOpened++;
                return third.Run();
            });
        var main = new Menu("Main", EMenuLevel.Main)
            .AddItem("C", "Count", () =>
            {
                counters.MainCounter++;
                return MenuResult.None;
            })
            .AddItem("S", "Second", () =>
            {
                counters.SecondOpened++;
                return second.Run();
            });
        return (main, counters);
    }

    private static string RunWithInput(Menu menu, params string[] lines)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        try
        {
            Console.SetIn(new StringReader(string.Join(Environment.NewLine, lines)));
            Console.SetOut(new StringWriter());
            return menu.Run();
        }
        finally
        {
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }
}
