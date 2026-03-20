namespace AppName.Identity.Contracts.Requests;

public record LoginRequest(string UserName, string Password)
    : Cheetah.Modules.Identity.Contracts.Requests.LoginRequest(UserName, Password);
