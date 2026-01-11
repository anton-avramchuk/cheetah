using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Tenants.Application.Commands;
using Cheetah.Tenants.Contracts.Requests;

namespace Cheetah.Tenants.Api.Endpoints;

public sealed class CreateTenantEndpoint
    : CreateCommandEndpoint<CreateTenantRequest, CreateTenantCommand>
{
    public override string Route => "/api/tenants";

    public override string GetByIdRouteName => "GetTenantById";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("CreateTenant")
            .WithTags("Tenants")
            .WithSummary("Create new tenant")
            .WithDescription("Creates a new tenant in the system");
    }
}
