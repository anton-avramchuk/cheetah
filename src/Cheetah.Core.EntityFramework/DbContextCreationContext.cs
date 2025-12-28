using System.Data.Common;

namespace Cheetah.Core.EntityFramework;

public class DbContextCreationContext(string connectionStringName, string connectionString)
{
    public static DbContextCreationContext? Current => _current.Value;
    private static readonly AsyncLocal<DbContextCreationContext?> _current = new();

    public string ConnectionStringName { get; } = connectionStringName;

    public string ConnectionString { get; } = connectionString;

    public DbConnection? ExistingConnection { get; internal set; }

    public static IDisposable Use(DbContextCreationContext context)
    {
        var previousValue = Current;
        _current.Value = context;
        return new DisposeAction(() => _current.Value = previousValue);
    }
}