using Cheetah.OpenApi.Services;

namespace Cheetah.Backend.ReverseProxy.Services;

public class ConfiguredClusterAddressProvider : IClusterAddressProvider
{
    private readonly IConfiguration _configuration;
    private readonly string _configKey;

    public ConfiguredClusterAddressProvider(IConfiguration configuration, string configKey)
    {
        _configuration = configuration;
        _configKey = configKey;
    }

    public IEnumerable<(string Address, string RoutePrefix)> GetClusterAddresses()
    {
        var routesSection = _configuration.GetSection($"{_configKey}:Routes");
        var clustersSection = _configuration.GetSection($"{_configKey}:Clusters");

        // Group route prefixes by cluster
        var routesByCluster = new Dictionary<string, List<string>>();

        foreach (var route in routesSection.GetChildren())
        {
            var clusterId = route.GetValue<string>("ClusterId");
            var path = route.GetSection("Match").GetValue<string>("Path");

            if (string.IsNullOrEmpty(clusterId) || string.IsNullOrEmpty(path))
                continue;

            var catchAllIndex = path.IndexOf("/{**catch-all}", StringComparison.Ordinal);
            var routePrefix = catchAllIndex >= 0
                ? path.Substring(0, catchAllIndex)
                : path;

            if (!routesByCluster.TryGetValue(clusterId, out var prefixes))
            {
                prefixes = [];
                routesByCluster[clusterId] = prefixes;
            }

            prefixes.Add(routePrefix);
        }

        // Yield one entry per cluster with the common prefix
        foreach (var (clusterId, prefixes) in routesByCluster)
        {
            var cluster = clustersSection.GetSection(clusterId);
            var destinations = cluster.GetSection("Destinations").GetChildren();
            var firstDestination = destinations.FirstOrDefault();

            if (firstDestination == null)
                continue;

            var address = firstDestination.GetValue<string>("Address");
            if (string.IsNullOrEmpty(address))
                continue;

            var commonPrefix = GetCommonPathPrefix(prefixes);
            yield return (address, commonPrefix);
        }
    }

    private static string GetCommonPathPrefix(List<string> prefixes)
    {
        if (prefixes.Count == 0) return "";
        if (prefixes.Count == 1) return prefixes[0];

        var common = prefixes[0];
        for (var i = 1; i < prefixes.Count; i++)
        {
            var other = prefixes[i];
            var minLen = Math.Min(common.Length, other.Length);
            var matchLen = 0;
            for (var j = 0; j < minLen; j++)
            {
                if (common[j] != other[j])
                    break;
                matchLen = j + 1;
            }

            common = common[..matchLen];
        }

        // Truncate to last complete path segment
        var lastSlash = common.LastIndexOf('/');
        if (lastSlash > 0)
            common = common[..lastSlash];

        return common;
    }
}