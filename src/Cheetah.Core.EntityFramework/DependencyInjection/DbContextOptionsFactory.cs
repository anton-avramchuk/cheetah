using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.Exceptions;
using Cheetah.Core.Extensions.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.EntityFramework.DependencyInjection;

public static class DbContextOptionsFactory
{
    public static DbContextOptions<TDbContext> Create<TDbContext>(IServiceProvider serviceProvider)
        where TDbContext : DbContext, ICrmDbContext
    {
        var creationContext = GetCreationContext<TDbContext>(serviceProvider);

        var context = new ApplicationDbContextConfigurationContext<TDbContext>(
            creationContext.ConnectionString,
            serviceProvider,
            creationContext.ConnectionStringName,
            creationContext.ExistingConnection!
        );

        var options = GetDbContextOptions<TDbContext>(serviceProvider);

        //PreConfigure(options, context);
        Configure(options, context);

        return context.DbContextOptions.Options;
    }

    private static CrmDbContextOptions GetDbContextOptions<TDbContext>(IServiceProvider serviceProvider)
        where TDbContext : DbContext, ICrmDbContext
    {
        return serviceProvider.GetRequiredService<IOptions<CrmDbContextOptions>>().Value;
    }

    private static DbContextCreationContext GetCreationContext<TDbContext>(IServiceProvider serviceProvider)
        where TDbContext : DbContext, ICrmDbContext
    {
        var context = DbContextCreationContext.Current;
        if (context != null)
        {
            return context;
        }

        var connectionStringName = ConnectionStringNameAttribute.GetConnStringName<TDbContext>();
        var connectionString = ResolveConnectionString<TDbContext>(serviceProvider, connectionStringName);

        return new DbContextCreationContext(
            connectionStringName,
            connectionString
        );
    }

    private static void Configure<TDbContext>(
        CrmDbContextOptions options,
        ApplicationDbContextConfigurationContext<TDbContext> context)
        where TDbContext : DbContext, ICrmDbContext
    {
        var configureAction = options.ConfigureActions.GetOrDefault(typeof(TDbContext));
        if (configureAction != null)
        {
            ((Action<ApplicationDbContextConfigurationContext<TDbContext>>)configureAction).Invoke(context);
        }
        else if (options.DefaultConfigureAction != null)
        {
            options.DefaultConfigureAction.Invoke(context);
        }
        else
        {
            throw new CrmException(
                $"No configuration found for {typeof(DbContext).AssemblyQualifiedName}! Use services.Configure<CrmDbContextOptions>(...) to configure it.");
        }
    }

    private static string ResolveConnectionString<TDbContext>(
        IServiceProvider serviceProvider,
        string connectionStringName)
    {
        // Use DefaultConnectionStringResolver.Resolve when we remove IConnectionStringResolver.Resolve
        var connectionStringResolver = serviceProvider.GetRequiredService<IConnectionStringResolver>();
        //        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenant>();

        //// Multi-tenancy unaware contexts should always use the host connection string
        //if (typeof(TDbContext).IsDefined(typeof(IgnoreMultiTenancyAttribute), false))
        //{
        //    using (currentTenant.Change(null))
        //    {
        //        return connectionStringResolver.Resolve(connectionStringName);
        //    }
        //}

        return connectionStringResolver.ResolveAsync(connectionStringName).Result;
    }

}