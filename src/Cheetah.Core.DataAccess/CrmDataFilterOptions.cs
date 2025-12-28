namespace Cheetah.Core.DataAccess;

public class CrmDataFilterOptions
{
    public Dictionary<Type, DataFilterState> DefaultStates { get; }

    public CrmDataFilterOptions()
    {
        DefaultStates = new Dictionary<Type, DataFilterState>();
    }
}

public class CrmDataSeedOptions
{
    public DataSeedContributorList Contributors { get; }

    public CrmDataSeedOptions()
    {
        Contributors = new DataSeedContributorList();
    }
}