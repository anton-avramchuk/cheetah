using Cheetah.Core.Exceptions;
using Cheetah.Core.Extensions.Collections;

namespace Cheetah.Core.DataAccess;

public class CrmDatabaseInfoDictionary : Dictionary<string, CrmDatabaseInfo>
{
    private Dictionary<string, CrmDatabaseInfo> ConnectionIndex { get; set; }

    public CrmDatabaseInfoDictionary()
    {
        ConnectionIndex = new Dictionary<string, CrmDatabaseInfo>();
    }

    public CrmDatabaseInfo? GetMappedDatabaseOrNull(string connectionStringName)
    {
        return ConnectionIndex.GetOrDefault(connectionStringName);
    }

    public CrmDatabaseInfoDictionary Configure(string databaseName, Action<CrmDatabaseInfo> configureAction)
    {
        var databaseInfo = this.GetOrAdd(
            databaseName,
            () => new CrmDatabaseInfo(databaseName)
        );

        configureAction(databaseInfo);

        return this;
    }

    /// <summary>
    /// This method should be called if this dictionary changes.
    /// It refreshes indexes for quick access to the connection informations.
    /// </summary>
    public void RefreshIndexes()
    {
        ConnectionIndex = new Dictionary<string, CrmDatabaseInfo>();

        foreach (var databaseInfo in Values)
        {
            foreach (var mappedConnection in databaseInfo.MappedConnections)
            {
                if (ConnectionIndex.ContainsKey(mappedConnection))
                {
                    throw new CrmException(
                        $"A connection name can not map to multiple databases: {mappedConnection}."
                    );
                }

                ConnectionIndex[mappedConnection] = databaseInfo;
            }
        }
    }
}