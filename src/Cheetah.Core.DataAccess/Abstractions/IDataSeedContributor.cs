namespace Cheetah.Core.DataAccess.Abstractions;

public interface IDataSeedContributor
{
    Task SeedAsync(DataSeedContext context);
}