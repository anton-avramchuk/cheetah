using Cheetah.AspNetCore.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Cheetah.Admin.Modules.Clients.Contracts.Requests;

public record GetClientByIdRequest([FromRoute] Guid Id) : ICrmRequest;