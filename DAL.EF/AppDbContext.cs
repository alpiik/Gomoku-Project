using GameEngine;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

/// <summary>
/// The database session. The options (SQLite and the file to use) come from outside: from dependency
/// injection in the program, from <see cref="AppDbContextFactory"/> for the dotnet ef tool, from the tests.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Configuration> Configurations => Set<Configuration>();
    public DbSet<SavedGame> SavedGames => Set<SavedGame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Configuration>(entity =>
        {
            // NOCASE: the unique index and every == on Name ignore upper/lower case, like the JSON file names
            entity.Property(c => c.Name).HasMaxLength(GameConfiguration.MaxNameLength).UseCollation("NOCASE");
            entity.HasIndex(c => c.Name).IsUnique();
        });

        modelBuilder.Entity<SavedGame>(entity =>
        {
            entity.Property(g => g.Id).ValueGeneratedNever(); // the engine creates the Guid
            entity.Property(g => g.Name).HasMaxLength(GameState.MaxNameLength);

            // SQLite stores dates as text and forgets that they are UTC; this puts the Kind back when reading
            entity.Property(g => g.SavedAtUtc)
                .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

            // Restrict: a configuration can't be deleted while saved games use it (Cascade would silently
            // delete the games too). The console UI checks this first and explains it to the user.
            entity.HasOne(g => g.Configuration)
                .WithMany(c => c.SavedGames)
                .HasForeignKey(g => g.ConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
