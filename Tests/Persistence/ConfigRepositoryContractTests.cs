using GameEngine;

namespace Tests.Persistence;

/// <summary>
/// The IConfigRepository contract. Every test runs twice, with "Json" and with "Db": both implementations
/// must pass exactly the same tests. Each test gets its own empty storage (xUnit creates a new instance per test).
/// </summary>
public class ConfigRepositoryContractTests : IDisposable
{
    private readonly TestStorage _storage = new();

    public void Dispose() => _storage.Dispose();

    public static TheoryData<string> Storages => TestStorage.Kinds;

    public static TheoryData<string, GameConfiguration> StoragesAndConfigs()
    {
        var data = new TheoryData<string, GameConfiguration>();
        foreach (var storage in TestStorage.Kinds)
        {
            foreach (var config in TestStorage.EveryExtension)
            {
                data.Add(storage, config);
            }
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void List_NothingStored_IsEmpty(string storage)
    {
        Assert.Empty(_storage.Configs(storage).List());
    }

    [Theory]
    [MemberData(nameof(StoragesAndConfigs))]
    public void SaveThenGet_ReturnsEqualConfiguration(string storage, GameConfiguration config)
    {
        var repository = _storage.Configs(storage);

        repository.Save(config);

        // records compare every value, ExactWinLength and ProOpening included
        Assert.Equal(config, repository.Get(config.Name));
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void List_IsAlphabetical_IgnoringCase_InTheSameOrderEverywhere(string storage)
    {
        var repository = _storage.Configs(storage);
        foreach (var name in new[] { "charlie", "a_b", "Alpha", "b-board" })
        {
            repository.Save(Presets.Small with { Name = name });
        }

        // "Alpha" before "a_b": SQLite's own NOCASE order would put "a_b" first
        Assert.Equal(["Alpha", "a_b", "b-board", "charlie"], repository.List());
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Save_SameNameTwice_OverwritesInsteadOfDuplicating(string storage)
    {
        var repository = _storage.Configs(storage);

        repository.Save(Presets.Small with { Name = "Tiny" });
        repository.Save(Presets.Small with { Name = "TINY", BoardWidth = 19 });

        Assert.Equal(["TINY"], repository.List());
        Assert.Equal(19, repository.Get("tiny").BoardWidth);
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Save_InvalidConfiguration_ThrowsAndStoresNothing(string storage)
    {
        var repository = _storage.Configs(storage);

        Assert.Throws<ArgumentException>(() => repository.Save(Presets.Small with { Name = "Huge", BoardWidth = 50 }));
        Assert.Empty(repository.List());
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Delete_RemovesConfiguration(string storage)
    {
        var repository = _storage.Configs(storage);
        repository.Save(Presets.Small);

        repository.Delete("Small");

        Assert.Empty(repository.List());
        Assert.Throws<KeyNotFoundException>(() => repository.Get("Small"));
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Delete_Missing_DoesNothing(string storage)
    {
        var repository = _storage.Configs(storage);

        repository.Delete("Nothing here");
        repository.Delete("../not even a valid name");

        Assert.Empty(repository.List());
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Get_Missing_ThrowsKeyNotFound(string storage)
    {
        var repository = _storage.Configs(storage);

        Assert.Throws<KeyNotFoundException>(() => repository.Get("Missing"));
        Assert.Throws<KeyNotFoundException>(() => repository.Get("../outside"));
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void SeedMissing_EmptyStorage_AddsAllPresets(string storage)
    {
        var repository = _storage.Configs(storage);

        Presets.SeedMissing(repository);

        Assert.Equal(["Free-style", "Pro", "Small", "Standard"], repository.List());
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void SeedMissing_RecreatesDeletedPreset_AndKeepsCustomOnes(string storage)
    {
        var repository = _storage.Configs(storage);
        Presets.SeedMissing(repository);
        repository.Save(Presets.Small with { Name = "Mine" });
        repository.Delete("Pro");

        Presets.SeedMissing(repository);

        Assert.Equal(["Free-style", "Mine", "Pro", "Small", "Standard"], repository.List());
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void SeedMissing_NeverOverwritesStoredPreset(string storage)
    {
        var repository = _storage.Configs(storage);
        repository.Save(Presets.Small with { WinLength = 5 });

        Presets.SeedMissing(repository);

        Assert.Equal(5, repository.Get("Small").WinLength);
    }
}
