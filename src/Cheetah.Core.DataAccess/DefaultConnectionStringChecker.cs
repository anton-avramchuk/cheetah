using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Core.DataAccess;

[Export(LifetimeType.Transient,typeof(IConnectionStringChecker))]
public class DefaultConnectionStringChecker : IConnectionStringChecker
{
    public Task<CrmConnectionStringCheckResult> CheckAsync(string connectionString)
    {
        return Task.FromResult(new CrmConnectionStringCheckResult
        {
            Connected = false,
            DatabaseExists = false
        });
    }
}