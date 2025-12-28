using System.Data.Common;
using Cheetah.Core.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework;

public abstract class CrmDbContextConfigurationContext : IServiceProviderAccessor
{
    public IServiceProvider ServiceProvider { get; }

    public string ConnectionString { get; }

    public string ConnectionStringName { get; }

    public DbConnection? ExistingConnection { get; }

    public DbContextOptionsBuilder DbContextOptions { get; protected set; }

    public CrmDbContextConfigurationContext(
        string connectionString,
        IServiceProvider serviceProvider,
        string connectionStringName,
        DbConnection? existingConnection)
    {
        ConnectionString = connectionString;
        ServiceProvider = serviceProvider;
        ConnectionStringName = connectionStringName;
        ExistingConnection = existingConnection;

        DbContextOptions = new DbContextOptionsBuilder()
            .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
            .UseApplicationServiceProvider(serviceProvider);
    }
}