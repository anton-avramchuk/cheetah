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
REST client ready:  ApiClientProof.PetStore.PetStoreClient
gRPC client ready:  Helloworld.Greeter+GreeterClient
gRPC request type:  Helloworld.HelloRequest (Name='Cheetah')
```

No network calls are made at runtime — the clients are only constructed to prove the generated types
compile and resolve. The OpenAPI/proto fetch happens at **build** time.

## What to look at

- `apiclients.json` — the two client definitions (one `rest`, one `grpc`).
- `ApiClientProof.csproj` — Kiota + gRPC package references, the literal `<Protobuf>` line required for
  gRPC, and the import of `Cheetah.ApiClientGen.targets`.
- `Program.cs` — constructs both generated clients.

Generated code is written to `obj/.../apiclients/` and is not committed. On a second build the spec is
unchanged (REST hash match / gRPC `304 Not Modified`), so generation is skipped.

See the [tool README](../../tools/Cheetah.ApiClientGen/README.md) for the full usage guide.
