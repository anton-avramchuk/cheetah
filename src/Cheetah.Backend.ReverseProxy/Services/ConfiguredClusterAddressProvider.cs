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

        // Group route prefixes by cluster, also track upstream base path from PathPattern transform
        var routesByCluster = new Dictionary<string, (List<string> Prefixes, string UpstreamBasePath)>();

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

            // Read PathPattern transform to determine what upstream base path YARP prepends,
            // so the OpenAPI aggregator can strip it when building combined paths.
            // e.g. PathPattern "/api/{**catch-all}" → upstreamBasePath "/api"
            var upstreamBasePath = "";
            foreach (var transform in route.GetSection("Transforms").GetChildren())
            {
                var pathPattern = transform.GetValue<string>("PathPattern");
                if (string.IsNullOrEmpty(pathPattern)) continue;

                var patternCatchAllIdx = pathPattern.IndexOf("{**catch-all}", StringComparison.Ordinal);
                if (patternCatchAllIdx >= 0)
                    upstreamBasePath = pathPattern[..patternCatchAllIdx].TrimEnd('/');
                break;
            }

            if (!routesByCluster.TryGetValue(clusterId, out var entry))
            {
                entry = ([], upstreamBasePath);
                routesByCluster[clusterId] = entry;
            }

            entry.Prefixes.Add(routePrefix);
        }

        // Yield one entry per cluster with the common prefix
        foreach (var (clusterId, (prefixes, upstreamBasePath)) in routesByCluster)
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

            // Include the upstream base path in the address so the OpenAPI aggregator
            // strips it from upstream paths before prepending the route prefix.
            // e.g. "http://crm-masterdata-api" + "/api" → aggregator strips "/api"
            // from "/api/industries" → combined becomes "/api/dictionary/industries".
            // Note: relative URI resolution in the aggregator (address + "openapi/v1.json")
            // still resolves to the correct OpenAPI endpoint on the upstream host.
            var effectiveAddress = string.IsNullOrEmpty(upstreamBasePath)
                ? address
                : address.TrimEnd('/') + upstreamBasePath;

            yield return (effectiveAddress, commonPrefix);
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