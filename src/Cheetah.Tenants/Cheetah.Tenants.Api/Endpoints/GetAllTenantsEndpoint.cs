using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Contracts.Requests;
using Cheetah.Tenants.Contracts.ViewModels;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Api.Endpoints;

public sealed class GetAllTenantsEndpoint
    : QueryCollectionEndpoint<GetAllTenantsRequest, GetAllTenantsQuery, Tenant, TenantViewModel>
{
    public override string Route => "/api/tenants";

    protected override void Configure(EndpointConfiguration config)
    {
        config
            .WithName("GetAllTenants")
            .WithTags("Tenants")
            .WithSummary("Get all tenants")
            .WithDescription("Returns a list of all tenants in the system");
    }
}
