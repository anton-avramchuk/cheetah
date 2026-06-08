using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cheetah.Core.EntityFramework.Sqlite.Extensions;

public static class CrmDbContextConfigurationContextSqliteExtensions
{
    public static DbContextOptionsBuilder UseSqlite(
        this CrmDbContextConfigurationContext context,
        Action<SqliteDbContextOptionsBuilder>? sqliteOptionsAction = null)
    {
        if (context.ExistingConnection != null)
        {
            return context.DbContextOptions.UseSqlite(context.ExistingConnection, optionsBuilder =>
            {
                optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqliteOptionsAction?.Invoke(optionsBuilder);
            });
        }
        else
        {
            return context.DbContextOptions.UseSqlite(context.ConnectionString, optionsBuilder =>
            {
                optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqliteOptionsAction?.Invoke(optionsBuilder);
            });
        }
    }
}
