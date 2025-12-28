using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Cheetah.Core.EntityFramework.PostgreSql.Extensions;

public static class CrmDbContextOptionsPostgreSqlExtensions
{
    public static void UseNpgsql(
        this CrmDbContextOptions options,
        Action<NpgsqlDbContextOptionsBuilder>? postgreSqlOptionsAction = null)
    {
        options.Configure(context => { context.UseNpgsql(postgreSqlOptionsAction); });
    }

    public static void UseNpgsql<TDbContext>(
        this CrmDbContextOptions options,
        Action<NpgsqlDbContextOptionsBuilder>? postgreSqlOptionsAction = null)
        where TDbContext : CrmDbContext<TDbContext>
    {
        options.Configure<TDbContext>(context => { context.UseNpgsql(postgreSqlOptionsAction); });
    }
}