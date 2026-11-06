using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

public static class DbContextExtensions
{
    /// <summary>
    /// Every repository method is one unit of work: one SaveChanges, then the context forgets the rows it loaded.
    /// The program uses one context for its whole run, so without this a failed operation (e.g. a delete the
    /// database refused) would stay queued and fail again inside the next, unrelated SaveChanges.
    /// </summary>
    public static void SaveChangesAndForget(this DbContext db)
    {
        try
        {
            db.SaveChanges();
        }
        finally
        {
            db.ChangeTracker.Clear();
        }
    }
}
