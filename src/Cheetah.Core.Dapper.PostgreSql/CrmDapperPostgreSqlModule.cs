using Cheetah.Core.Modularity;

namespace Cheetah.Core.Dapper.PostgreSql;

/// <summary>
/// PostgreSQL provider for the Dapper data-access layer. Registers the Npgsql connection
/// provider and the PostgreSQL <c>ISqlDialect</c> on top of <see cref="CrmDapperModule"/>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDapperModule))]
public partial class CrmDapperPostgreSqlModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
