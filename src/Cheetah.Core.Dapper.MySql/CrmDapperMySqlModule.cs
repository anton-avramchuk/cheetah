using Cheetah.Core.Modularity;

namespace Cheetah.Core.Dapper.MySql;

/// <summary>
/// MySQL/MariaDB provider for the Dapper data-access layer. Registers the MySqlConnector
/// connection provider and the MySQL <c>ISqlDialect</c> on top of <see cref="CrmDapperModule"/>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDapperModule))]
public partial class CrmDapperMySqlModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
