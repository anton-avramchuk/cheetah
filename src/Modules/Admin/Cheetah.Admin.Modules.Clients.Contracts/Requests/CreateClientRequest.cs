using Cheetah.AspNetCore.Contracts.Requests;

namespace Cheetah.Admin.Modules.Clients.Contracts.Requests;

public record CreateClientRequest(string Name, string? Description) : ICrmRequest;
