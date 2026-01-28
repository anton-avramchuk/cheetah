using System.ComponentModel.DataAnnotations;
using Cheetah.AspNetCore.Contracts.Requests;

namespace Cheetah.Admin.Modules.Clients.Contracts.Requests;

public record CreateClientRequest(
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;
