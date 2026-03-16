using System.ComponentModel.DataAnnotations;
using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Contracts.Requests;

[ApiRoute("api/auth/login", ApiMethod.PostWithResult, ResponseType = typeof(TokenViewModel), ServiceName = "Auth", MethodName = "LoginAsync")]
public record LoginRequest(
    [property: Required(AllowEmptyStrings = false)]
    string UserName,
    [property: Required(AllowEmptyStrings = false)]
    string Password) : ICrmRequest;
