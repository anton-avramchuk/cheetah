using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cheetah.Core.EntityFramework.Sqlite.Extensions;

public static class CrmDbContextOptionsSqliteExtensions
{
    public static void UseSqlite(
        this CrmDbContextOptions options,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
    {
        options.Configure(context =>
        {
            context.UseSqlite(sqliteOptionsAction);
        });
    }

    public static void UseSqlite<TDbContext>(
        this CrmDbContextOptions options,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
        where TDbContext : CrmDbContext<TDbContext>
    {
        options.Configure<TDbContext>(context =>
        {
            context.UseSqlite(sqliteOptionsAction);
        });
    }
}
