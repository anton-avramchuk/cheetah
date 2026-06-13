using ApiClientProof.PetStore;
using Grpc.Net.Client;
using Helloworld;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Bundle;

// Proof that BOTH generated clients compile and can be constructed. No network calls are made — this
// just exercises the generated types end to end.

// REST client generated from an OpenAPI URL via Kiota.
var adapter = new DefaultRequestAdapter(new AnonymousAuthenticationProvider());
var restClient = new PetStoreClient(adapter);
Console.WriteLine($"REST client ready:  {restClient.GetType().FullName}");

// gRPC client generated from a .proto URL via Grpc.Tools.
using var channel = GrpcChannel.ForAddress("https://localhost:5001");
var grpcClient = new Greeter.GreeterClient(channel);
var request = new HelloRequest { Name = "Cheetah" };
Console.WriteLine($"gRPC client ready:  {grpcClient.GetType().FullName}");
Console.WriteLine($"gRPC request type:  {request.GetType().FullName} (Name='{request.Name}')");
