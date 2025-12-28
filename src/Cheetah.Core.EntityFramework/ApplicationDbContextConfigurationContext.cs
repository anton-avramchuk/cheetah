using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework;

public class ApplicationDbContextConfigurationContext<TDbContext> : CrmDbContextConfigurationContext
    where TDbContext : DbContext, ICrmDbContext
{
    public new DbContextOptionsBuilder<TDbContext> DbContextOptions => (DbContextOptionsBuilder<TDbContext>)base.DbContextOptions;

    public ApplicationDbContextConfigurationContext(string connectionString, IServiceProvider serviceProvider, string connectionStringName, DbConnection existingConnection) : base(connectionString, serviceProvider, connectionStringName, existingConnection)
    {
        base.DbContextOptions = new DbContextOptionsBuilder<TDbContext>()
            .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
            .UseApplicationServiceProvider(serviceProvider); ;
    }
}