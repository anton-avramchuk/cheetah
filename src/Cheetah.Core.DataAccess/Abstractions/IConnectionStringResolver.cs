namespace Cheetah.Core.DataAccess.Abstractions;

public interface IConnectionStringResolver
{
    Task<string> ResolveAsync(string? connectionStringName = null);
}