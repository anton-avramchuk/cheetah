# Cheetah.ApiClientGen

Build-time generator of typed .NET clients for **external** services from their published contracts:

- **REST** → fetches an OpenAPI document by URL and generates a client + models via **Kiota**.
- **gRPC** → fetches a `.proto` by URL; **Grpc.Tools** compiles it into a client + messages.

The tool itself is a thin wrapper: it does the **fetch + cache + change-detection**, and delegates
code generation to the mature generators. Generated code lands in `obj/` (never committed) and is fed
into the consuming project's compilation automatically (REST) or via one `<Protobuf>` line (gRPC).

## How it works

On every build, for each configured client:

1. **Conditional GET** of the spec (sends `If-None-Match` with the cached ETag). `304 Not Modified`
   → the cached copy is reused.
2. The downloaded bytes are hashed (SHA-256). If the hash is unchanged **and** output already exists,
   generation is **skipped** (no rebuild churn).
3. On change: REST runs Kiota; gRPC places the `.proto` where Grpc.Tools will compile it.
4. **Network failure / timeout → falls back to the last cached spec**, so a flaky external service
   does not break the build. (If no cache exists yet, the build fails with a clear message.)

## Prerequisites

- **Kiota CLI** as a local dotnet tool (for REST). Add it once to the repo's tool manifest:
  ```bash
  dotnet new tool-manifest      # if .config/dotnet-tools.json doesn't exist
  dotnet tool install Microsoft.OpenApi.Kiota
  ```
  The build runs `dotnet tool restore` automatically.
- **Grpc.Tools** package reference in the consuming project (for gRPC).

## Configuration — `apiclients.json`

Place an `apiclients.json` next to the consuming `.csproj`:

```json
{
  "clients": [
    {
      "name": "PetStore",
      "protocol": "rest",
      "url": "https://petstore3.swagger.io/api/v3/openapi.json",
      "namespace": "MyApp.Clients.PetStore",
      "className": "PetStoreClient"
    },
    {
      "name": "Pricing",
      "protocol": "grpc",
      "url": "https://raw.githubusercontent.com/acme/pricing/main/pricing.proto",
      "namespace": "MyApp.Clients.Pricing"
    }
  ]
}
```

| Field       | REST | gRPC | Notes |
|-------------|:---:|:---:|-------|
| `name`      | ✅ | ✅ | Output folder, cache file stem, and (REST) default class name. |
| `protocol`  | ✅ | ✅ | `rest` or `grpc`. |
| `url`       | ✅ | ✅ | OpenAPI document URL / `.proto` URL. |
| `namespace` | ✅ | ⚠️ | REST: client namespace. gRPC: **informational** — the namespace comes from the `.proto` (`option csharp_namespace` / `package`). |
| `className` | ✅ | — | REST only; defaults to `{name}Client`. |

## Wiring a consuming project

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <!-- Path to the built generator tool (see "Running the tool" below). -->
    <ApiClientGenDll>$(MSBuildThisFileDirectory)..\..\tools\Cheetah.ApiClientGen\bin\$(Configuration)\net10.0\Cheetah.ApiClientGen.dll</ApiClientGenDll>
  </PropertyGroup>

  <!-- REST runtime deps (Kiota-generated client). -->
  <ItemGroup>
    <PackageReference Include="Microsoft.Kiota.Bundle" />
  </ItemGroup>

  <!-- gRPC deps. Grpc.Tools generates at build; the others are runtime. No <Protobuf> lines needed —
       the generator registers each fetched .proto automatically (see "gRPC integration"). -->
  <ItemGroup>
    <PackageReference Include="Grpc.Tools" PrivateAssets="all" />
    <PackageReference Include="Grpc.Net.Client" />
    <PackageReference Include="Google.Protobuf" />
  </ItemGroup>

  <Import Project="..\..\tools\Cheetah.ApiClientGen\Cheetah.ApiClientGen.targets" />

</Project>
```

Then use the generated clients:

```csharp
// REST (Kiota)
var adapter = new Microsoft.Kiota.Bundle.DefaultRequestAdapter(
    new Microsoft.Kiota.Abstractions.Authentication.AnonymousAuthenticationProvider());
var petStore = new MyApp.Clients.PetStore.PetStoreClient(adapter);

// gRPC (Grpc.Tools) — namespace comes from the .proto
using var channel = Grpc.Net.Client.GrpcChannel.ForAddress("https://pricing.acme.io");
var pricing = new Pricing.Pricing.PricingClient(channel);
```

### Running the tool

For the proof the consuming project references the tool with
`<ProjectReference … ReferenceOutputAssembly="false" />` so it is **built first**, and the targets run
its DLL (`$(ApiClientGenDll)`) — no nested `dotnet run`. For wider use, package it as a `dotnet tool`
and invoke `dotnet apiclientgen` instead of a DLL path.

## MSBuild integration (`Cheetah.ApiClientGen.targets`)

The imported target `CheetahApiClientGen` runs **before `PrepareForBuild`** — earlier than the whole
Grpc.Tools proto graph. It:

- runs `dotnet tool restore` (Kiota),
- executes the tool to fetch + generate into `$(IntermediateOutputPath)apiclients/`,
- adds the generated REST `*.cs` to `@(Compile)` and the fetched `*.proto` to `@(Protobuf)`,
  dynamically (the cache folder `_specs/` is excluded so a spec is never compiled twice).

### gRPC integration — fully automatic

No per-client `<Protobuf>` lines are required. The subtlety is **timing**: `Grpc.Tools` gathers
`@(Protobuf)` in targets that run just before `BeforeCompile`. Because `CheetahApiClientGen` runs at
`PrepareForBuild` — *before* that graph — the `<Protobuf>` items it adds after fetching the `.proto`
are picked up and compiled in the **same build**. (Adding them from a later hook, e.g. before
`CoreCompile`, would be too late — that was the original pitfall.)

## Constraints & notes

- **Fetch-on-every-build** is the configured behaviour; conditional GET (`304`) keeps it cheap and the
  cache fallback keeps it resilient. To fetch less aggressively, point the tool at a committed spec.
- Some external services ship an official .NET SDK on NuGet — check first; you may not need codegen.
- Authenticated specs (token-protected `/openapi` or `.proto`) are not covered yet — add an auth header
  in `SpecFetcher`.
- Pulling a `.proto` from gRPC **server reflection** (when no `.proto` file is published) is not
  implemented; supply the `.proto` by URL.

## Reference

`samples/ApiClientProof/` is a runnable proof generating both a REST (Swagger Petstore) and a gRPC
(`helloworld.proto`) client from public URLs in a single `dotnet build`.
