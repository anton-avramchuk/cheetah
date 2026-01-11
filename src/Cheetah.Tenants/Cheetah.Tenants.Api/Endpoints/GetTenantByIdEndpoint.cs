using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Contracts.Requests;
using Cheetah.Tenants.Contracts.ViewModels;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Api.Endpoints;

public sealed class GetTenantByIdEndpoint
    : QueryOrNotFoundEndpoint<GetTenantByIdRequest, GetTenantByIdQuery, Tenant, TenantViewModel>
{
    public override string Route => "/api/tenants/{id:guid}";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("GetTenantById")
            .WithTags("Tenants")
            .WithSummary("Get tenant by ID")
            .WithDescription("Returns a tenant by its unique identifier");
    }
}
