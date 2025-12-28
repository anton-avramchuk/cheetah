using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Extensions.Common;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.DataAccess;

[Export(LifetimeType.Transient, typeof(IConnectionStringResolver))]
public class DefaultConnectionStringResolver : IConnectionStringResolver
{
    protected CrmDbConnectionOptions Options { get; }

    public DefaultConnectionStringResolver(
        IOptionsMonitor<CrmDbConnectionOptions> options)
    {
        Options = options.CurrentValue;
    }

    [Obsolete("Use ResolveAsync method.")]
    public virtual string Resolve(string? connectionStringName = null)
    {
        return ResolveInternal(connectionStringName)!;
    }

    public virtual Task<string> ResolveAsync(string? connectionStringName = null)
    {
        return Task.FromResult(ResolveInternal(connectionStringName))!;
    }

    private string? ResolveInternal(string? connectionStringName)
    {
        if (connectionStringName == null)
        {
            return Options.ConnectionStrings.Default;
        }

        var connectionString = Options.GetConnectionStringOrNull(connectionStringName);

        if (!connectionString.IsNullOrEmpty())
        {
            return connectionString;
        }

        return null;
    }
}