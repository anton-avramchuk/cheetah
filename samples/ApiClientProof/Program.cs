using ApiClientProof.PetStore;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Helloworld;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;
using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

// Demonstrates wiring BEARER AUTH + RETRY onto the generated clients. These are transport concerns:
// they are configured where the client is constructed — never in the generated code or apiclients.json.
// No network calls are made here; the clients are only built to prove the configuration compiles.

const string bearerToken = "demo-token"; // real code: fetch from your OAuth/secret store

var restClient = BuildRestClient(bearerToken);
Console.WriteLine($"REST client ready (bearer + retry): {restClient.GetType().FullName}");

var grpcClient = BuildGrpcClient(bearerToken);
Console.WriteLine($"gRPC client ready (bearer + retry): {grpcClient.GetType().FullName}");

// ---- REST (Kiota): IAuthenticationProvider for auth + a RetryHandler in the HTTP pipeline ----
static PetStoreClient BuildRestClient(string token)
{
    var auth = new BaseBearerTokenAuthenticationProvider(new StaticBearerTokenProvider(token));

    // Kiota's default handlers already include retry/redirect; swap in a configured RetryHandler.
    var handlers = KiotaClientFactory.CreateDefaultHandlers()
        .Where(h => h is not RetryHandler)
        .ToList();
    handlers.Add(new RetryHandler(new RetryHandlerOption
    {
        MaxRetry = 3,
        Delay = 2, // seconds; the handler applies exponential backoff
        ShouldRetry = (delay, attempt, response) =>
            response.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable
                               or System.Net.HttpStatusCode.TooManyRequests,
    }));

    var httpClient = KiotaClientFactory.Create(finalHandler: null, handlers: handlers);
    httpClient.Timeout = TimeSpan.FromSeconds(30);

    // DefaultRequestAdapter (from the Kiota bundle) keeps the bundled serializers registered.
    var adapter = new DefaultRequestAdapter(auth, httpClient: httpClient);
    return new PetStoreClient(adapter);
}

// ---- gRPC: per-call bearer credentials + built-in retry policy via ServiceConfig ----
static Greeter.GreeterClient BuildGrpcClient(string token)
{
    // CallCredentials require TLS; they add the Authorization header on every call.
    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
    {
        metadata.Add("Authorization", $"Bearer {token}");
        return Task.CompletedTask;
    });

    var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions
    {
        Credentials = ChannelCredentials.Create(ChannelCredentials.SecureSsl, credentials),
        ServiceConfig = new ServiceConfig
        {
            MethodConfigs =
            {
                new MethodConfig
                {
                    Names = { MethodName.Default }, // applies to every method
                    RetryPolicy = new RetryPolicy
                    {
                        MaxAttempts = 3,
                        InitialBackoff = TimeSpan.FromSeconds(1),
                        MaxBackoff = TimeSpan.FromSeconds(5),
                        BackoffMultiplier = 1.5,
                        RetryableStatusCodes = { StatusCode.Unavailable },
                    },
                },
            },
        },
    });
    return new Greeter.GreeterClient(channel);
}

// Supplies a static bearer token. In production implement token acquisition/refresh here.
sealed class StaticBearerTokenProvider(string token) : IAccessTokenProvider
{
    public AllowedHostsValidator AllowedHostsValidator { get; } = new(); // empty = all hosts allowed

    public Task<string> GetAuthorizationTokenAsync(
        Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default) => Task.FromResult(token);
}
