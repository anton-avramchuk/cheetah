namespace Cheetah.OpenApi.Services;

public interface IClusterAddressProvider
{
    IEnumerable<(string Address, string RoutePrefix)> GetClusterAddresses();
}