using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Requests;

namespace Crm.Identity.Contracts.Requests;

public record LoginRequest(
    [property: Required(AllowEmptyStrings = false)]
    string UserName,
    [property: Required(AllowEmptyStrings = false)]
    string Password) : ICrmRequest;
