using Cheetah.Core.Modularity;

namespace Cheetah.Core.Dapper.MsSql;

/// <summary>
/// SQL Server provider for the Dapper data-access layer. Registers the SQL Server connection
/// provider and the SQL Server <c>ISqlDialect</c> on top of <see cref="CrmDapperModule"/>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDapperModule))]
public partial class CrmDapperMsSqlModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
