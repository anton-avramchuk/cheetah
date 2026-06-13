using Cheetah.Core.Modularity;

namespace Cheetah.Core.Dapper.Sqlite;

/// <summary>
/// SQLite provider for the Dapper data-access layer. Registers the SQLite connection provider
/// and the SQLite <c>ISqlDialect</c> on top of <see cref="CrmDapperModule"/>. Handy for tests
/// and local development.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDapperModule))]
public partial class CrmDapperSqliteModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
