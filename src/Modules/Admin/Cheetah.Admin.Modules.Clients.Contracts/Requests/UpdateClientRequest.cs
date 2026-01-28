using System.ComponentModel.DataAnnotations;
using Cheetah.AspNetCore.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Cheetah.Admin.Modules.Clients.Contracts.Requests;

public record UpdateClientRequest(
    [FromRoute] Guid Id,
    [property: Required(AllowEmptyStrings = false)]
    string Name,
    string? Description) : ICrmRequest;
