using ConsoleUI;
using DAL.EF;
using DAL.Json;
using GameEngine;
using MenuSystem;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static ConsoleUI.StorageErrors;

// Composition root: the only place that decides which classes are used. Everything else gets what it
// needs through its constructor and sees only the interfaces.

// "Persistence" is read from appsettings.json; an environment variable with the same name overrides it
var settings = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();
var persistence = settings["Persistence"];

var services = new ServiceCollection();
switch (persistence)
{
    case "Json":
        // they hold nothing but a folder name, so one instance for the whole run is fine
        services.AddSingleton<IConfigRepository>(_ => new ConfigRepositoryJson());
        services.AddSingleton<IGameRepository>(_ => new GameRepositoryJson());
        break;
    case "Db":
        // the DbContext and the repositories that use it are scoped, never singletons
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(DbPaths.DefaultConnectionString()));
        services.AddScoped<IConfigRepository, ConfigRepositoryEf>();
        services.AddScoped<IGameRepository, GameRepositoryEf>();
        break;
    default:
        throw new InvalidOperationException(
            $"appsettings.json: \"Persistence\" must be \"Json\" or \"Db\", not \"{persistence}\".");
}

// one Random for the whole run (Random.Shared is also safe if an AI later thinks on another thread);
// creating a new Random for every move is what the assignment warns against
services.AddSingleton(Random.Shared);
services.AddSingleton<ConsoleMoveProvider>();
services.AddSingleton<MoveProviderFactory>();
services.AddScoped<ConfigurationUi>();
services.AddScoped<GamesUi>();

// a singleton that needs a scoped service (the DbContext) is reported at startup instead of misbehaving later
using var provider = services.BuildServiceProvider(new ServiceProviderOptions
{
    ValidateScopes = true,
    ValidateOnBuild = true,
});

// one scope for the whole run; every repository method is its own unit of work
using var scope = provider.CreateScope();
var resolve = scope.ServiceProvider;

if (persistence == "Db")
{
    // creates the database file on the first run and applies new migrations: no manual dotnet ef needed
    resolve.GetRequiredService<AppDbContext>().Database.Migrate();
}

Presets.SeedMissing(resolve.GetRequiredService<IConfigRepository>());

var configUi = resolve.GetRequiredService<ConfigurationUi>();
var gamesUi = resolve.GetRequiredService<GamesUi>();

var configMenu = new Menu("Gomoku - configurations", EMenuLevel.Second)
    .AddItem("L", "List configurations", Guard(configUi.ListAll))
    .AddItem("C", "Create a configuration", Guard(configUi.Create))
    .AddItem("E", "Edit a configuration", Guard(configUi.Edit))
    .AddItem("D", "Delete a configuration", Guard(configUi.Delete));

var savedGamesMenu = new Menu("Gomoku - saved games", EMenuLevel.Second)
    .AddItem("L", "List saved games", Guard(gamesUi.ListAll))
    .AddItem("D", "Delete a saved game", Guard(gamesUi.Delete));

var storageName = persistence == "Db" ? "database" : "JSON files";
var mainMenu = new Menu($"Gomoku (saving to {storageName})", EMenuLevel.Main)
    .AddItem("N", "New game", Guard(gamesUi.NewGame))
    .AddItem("L", "Load a saved game", Guard(gamesUi.Continue))
    .AddItem("S", "Saved games", savedGamesMenu.Run)
    .AddItem("C", "Configurations", configMenu.Run);

mainMenu.Run();
Console.WriteLine("Bye!");
