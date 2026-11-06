using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL.EF;

/// <summary>
/// Used only by the dotnet ef tool ("migrations add"), which needs a context but doesn't run the program.
/// The program itself gets its context from dependency injection.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(DbPaths.DefaultConnectionString()).Options);
}
