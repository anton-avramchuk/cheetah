using Cheetah.AspNetCore.Contracts.Requests;

namespace Cheetah.Tenants.Contracts.Requests;

public sealed record GetTenantByIdRequest(Guid Id) : ICrmRequest;
