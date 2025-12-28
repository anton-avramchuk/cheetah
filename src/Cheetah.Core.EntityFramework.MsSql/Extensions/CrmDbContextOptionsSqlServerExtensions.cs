using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cheetah.Core.EntityFramework.MsSql.Extensions;

public static class CrmDbContextOptionsSqlServerExtensions
{
    public static void UseSqlServer(
        this CrmDbContextOptions options,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
    {
        options.Configure(context =>
        {
            context.UseSqlServer(sqlServerOptionsAction);
        });
    }

    public static void UseSqlServer<TDbContext>(
        this CrmDbContextOptions options,
        Action<SqlServerDbContextOptionsBuilder>? sqlServerOptionsAction = null)
        where TDbContext : CrmDbContext<TDbContext>
    {
        options.Configure<TDbContext>(context =>
        {
            context.UseSqlServer(sqlServerOptionsAction);
        });
    }
}