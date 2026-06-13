using System.Diagnostics;

namespace Cheetah.ApiClientGen;

/// <summary>
/// Invokes the Kiota CLI (restored as a local dotnet tool) to generate a C# client from a local
/// OpenAPI document. The wrapper owns fetching/caching/change-detection; Kiota owns codegen.
/// </summary>
public static class KiotaRunner
{
    public static async Task<int> GenerateAsync(ClientConfig client, string specPath, string outputDir, CancellationToken ct)
    {
        Directory.CreateDirectory(outputDir);
        var className = client.ClassName ?? $"{client.Name}Client";

        var args = new[]
        {
            "kiota", "generate",
            "--language", "CSharp",
            "--openapi", specPath,
            "--output", outputDir,
            "--namespace-name", client.Namespace,
            "--class-name", className,
            "--clean-output",
            "--exclude-backward-compatible",
            "--log-level", "Warning",
        };

        var psi = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var arg in args)
            psi.ArgumentList.Add(arg);

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start 'dotnet kiota'.");

        var stdout = await process.StandardOutput.ReadToEndAsync(ct);
        var stderr = await process.StandardError.ReadToEndAsync(ct);
        await process.WaitForExitAsync(ct);

        if (!string.IsNullOrWhiteSpace(stdout))
            Console.WriteLine(stdout.TrimEnd());
        if (!string.IsNullOrWhiteSpace(stderr))
            Console.Error.WriteLine(stderr.TrimEnd());

        return process.ExitCode;
    }
}
