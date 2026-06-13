using Crm.Customer.Api.Grpc;
using Crm.Customer.Api.Tests.Fixtures;
using Grpc.Core;
using Grpc.Net.Client;
using Shouldly;

namespace Crm.Customer.Api.Tests.Grpc;

[Collection("CustomerApi")]
public class CustomerGrpcTests
{
    private readonly CustomerGrpc.CustomerGrpcClient _client;

    public CustomerGrpcTests(CustomerApiFixture fixture)
    {
        var handler = fixture.Server.CreateHandler();
        var channel = GrpcChannel.ForAddress(
            fixture.Server.BaseAddress,
            new GrpcChannelOptions { HttpHandler = handler });
        _client = new CustomerGrpc.CustomerGrpcClient(channel);
    }

    [Fact]
    public async Task CreateThenGet_ShouldRoundTripThroughDispatcher()
    {
        var created = await _client.CreateCustomerAsync(new CreateCustomerGrpcRequest
        {
            Name = "Grpc Co",
            Description = "via grpc"
        });

        created.Id.ShouldNotBeNullOrEmpty();

        var fetched = await _client.GetCustomerByIdAsync(new GetCustomerByIdGrpcRequest { Id = created.Id });

        fetched.Id.ShouldBe(created.Id);
        fetched.Name.ShouldBe("Grpc Co");
        fetched.Description.ShouldBe("via grpc");
    }

    [Fact]
    public async Task GetById_WhenMissing_ShouldMapToNotFoundStatus()
    {
        var ex = await Should.ThrowAsync<RpcException>(async () =>
            await _client.GetCustomerByIdAsync(new GetCustomerByIdGrpcRequest
            {
                Id = Guid.NewGuid().ToString()
            }));

        ex.StatusCode.ShouldBe(StatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WhenIdIsNotAGuid_ShouldMapToInvalidArgument()
    {
        // кривой id от клиента -> Guid.Parse кидает FormatException -> должно стать InvalidArgument, не Internal
        var ex = await Should.ThrowAsync<RpcException>(async () =>
            await _client.GetCustomerByIdAsync(new GetCustomerByIdGrpcRequest { Id = "not-a-guid" }));

        ex.StatusCode.ShouldBe(StatusCode.InvalidArgument);
    }
}
