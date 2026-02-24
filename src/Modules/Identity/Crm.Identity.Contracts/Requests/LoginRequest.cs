using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Contracts.Requests;

[ApiRoute("api/auth/login", ApiMethod.PostWithResult, ResponseType = typeof(TokenViewModel), ServiceName = "Auth", MethodName = "LoginAsync")]
public record LoginRequest(
    [property: Required(AllowEmptyStrings = false)]
    string UserName,
    [property: Required(AllowEmptyStrings = false)]
    string Password) : ICrmRequest;
