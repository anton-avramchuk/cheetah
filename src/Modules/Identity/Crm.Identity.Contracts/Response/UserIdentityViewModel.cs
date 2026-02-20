using System;
using Cheetah.Contracts.Responses;

namespace Crm.Identity.Contracts.Response;

public record UserIdentityViewModel(Guid Id, string Name, string? Description) : ICrmResponse;