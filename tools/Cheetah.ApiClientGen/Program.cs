using System.Text.Json;
using Cheetah.ApiClientGen;

// Minimal CLI:  Cheetah.ApiClientGen gen --config <apiclients.json> --out <dir>
// Fetches each REST client's OpenAPI spec (conditional GET + cache) and regenerates with Kiota only
// when the spec changed or the output is missing.

if (args.Length == 0 || args[0] != "gen")
{
    Console.Error.WriteLine("Usage: Cheetah.ApiClientGen gen --config <apiclients.json> --out <dir>");
    return 2;
}

var options = ParseOptions(args);
if (!options.TryGetValue("config", out var configPath) || !options.TryGetValue("out", out var outRoot))
{
    Console.Error.WriteLine("Both --config and --out are required.");
    return 2;
}

if (!File.Exists(configPath))
{
    Console.Error.WriteLine($"Config not found: {configPath}");
    return 2;
}

var config = JsonSerializer.Deserialize<ApiClientsConfig>(
    await File.ReadAllTextAsync(configPath),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

if (config is null || config.Clients.Count == 0)
{
    Console.WriteLine("[apiclientgen] No clients configured; nothing to do.");
    return 0;
}

var cacheDir = Path.Combine(outRoot, "_specs");
var fetcher = new SpecFetcher();
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

foreach (var client in config.Clients)
{
    var clientOutput = Path.Combine(outRoot, client.Name);
    var protocol = client.Protocol.ToLowerInvariant();

    switch (protocol)
    {
        case "rest":
        {
            var fetch = await fetcher.FetchAsync(client.Name, client.Url, cacheDir, ".openapi.json", cts.Token);
            Console.WriteLine($"[apiclientgen] {client.Name} (rest): {fetch.Note}");

            var hasOutput = Directory.Exists(clientOutput)
                && Directory.EnumerateFiles(clientOutput, "*.cs", SearchOption.AllDirectories).Any();
            if (!fetch.Changed && hasOutput)
            {
                Console.WriteLine($"[apiclientgen] {client.Name}: spec unchanged and client present — skipping generation.");
                continue;
            }

            Console.WriteLine($"[apiclientgen] {client.Name}: generating client into {clientOutput} ...");
            var exit = await KiotaRunner.GenerateAsync(client, fetch.SpecPath, clientOutput, cts.Token);
            if (exit != 0)
            {
                Console.Error.WriteLine($"[apiclientgen] {client.Name}: Kiota failed with exit code {exit}.");
                return exit;
            }

            Console.WriteLine($"[apiclientgen] {client.Name}: done.");
            break;
        }

        case "grpc":
        {
            // The tool only places the .proto; Grpc.Tools (via the <Protobuf> item the targets add)
            // performs C# codegen during the build.
            var fetch = await fetcher.FetchAsync(client.Name, client.Url, cacheDir, ".proto", cts.Token);
            Console.WriteLine($"[apiclientgen] {client.Name} (grpc): {fetch.Note}");

            Directory.CreateDirectory(clientOutput);
            var protoPath = Path.Combine(clientOutput, $"{client.Name}.proto");
            if (fetch.Changed || !File.Exists(protoPath))
            {
                File.Copy(fetch.SpecPath, protoPath, overwrite: true);
                Console.WriteLine($"[apiclientgen] {client.Name}: proto placed at {protoPath} (Grpc.Tools will compile it).");
            }
            else
            {
                Console.WriteLine($"[apiclientgen] {client.Name}: proto unchanged — leaving in place.");
            }

            break;
        }

        default:
            Console.WriteLine($"[apiclientgen] Skipping '{client.Name}': unknown protocol '{client.Protocol}'.");
            break;
    }
}

return 0;

static Dictionary<string, string> ParseOptions(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (var i = 1; i < args.Length - 1; i++)
    {
        if (args[i].StartsWith("--", StringComparison.Ordinal))
            result[args[i][2..]] = args[i + 1];
    }
    return result;
}
