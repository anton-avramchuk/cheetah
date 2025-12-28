namespace Cheetah.Core.DataAccess.Abstractions;

public interface IConnectionStringChecker
{
    Task<CrmConnectionStringCheckResult> CheckAsync(string connectionString);
}

