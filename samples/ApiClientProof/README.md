# ApiClientProof

Runnable proof for [`Cheetah.ApiClientGen`](../../tools/Cheetah.ApiClientGen/README.md): generates
**two** typed clients from public URLs in a single build, then constructs them.

- **REST** — Swagger Petstore (`https://petstore3.swagger.io/api/v3/openapi.json`) → Kiota client.
- **gRPC** — gRPC's `helloworld.proto` → Grpc.Tools client.

## Run

```bash
dotnet run --project samples/ApiClientProof
```

Expected output:

```
REST client ready (bearer + retry): ApiClientProof.PetStore.PetStoreClient
gRPC client ready (bearer + retry): Helloworld.Greeter+GreeterClient
```

No network calls are made at runtime — the clients are only constructed to prove the generated types
compile and resolve. The OpenAPI/proto fetch happens at **build** time.

## Bearer auth + retry

`Program.cs` shows the real-world wiring of **bearer authentication** and **retry** for both clients —
configured at construction time, not in the generated code or `apiclients.json`:

- **REST (Kiota):** `BaseBearerTokenAuthenticationProvider` (with an `IAccessTokenProvider`) for auth,
  and a `RetryHandler` (+ `HttpClient.Timeout`) in the HTTP pipeline.
- **gRPC:** per-call `CallCredentials` that add `Authorization: Bearer …`, plus the built-in gRPC
  retry policy via `GrpcChannelOptions.ServiceConfig`.

See the [tool README](../../tools/Cheetah.ApiClientGen/README.md#configuring-the-generated-clients-auth-retry-timeouts)
for the DI/production variants (`IHttpClientFactory` + resilience, `AddGrpcClient`).

## What to look at

- `apiclients.json` — the two client definitions (one `rest`, one `grpc`).
- `ApiClientProof.csproj` — Kiota + gRPC package references and the import of
  `Cheetah.ApiClientGen.targets`. Note there is **no** `<Protobuf>` line — the generator registers the
  fetched proto with Grpc.Tools automatically.
- `Program.cs` — constructs both generated clients with bearer auth + retry (see below).

Generated code is written to `obj/.../apiclients/` and is not committed. On a second build the spec is
unchanged (REST hash match / gRPC `304 Not Modified`), so generation is skipped.

See the [tool README](../../tools/Cheetah.ApiClientGen/README.md) for the full usage guide.
