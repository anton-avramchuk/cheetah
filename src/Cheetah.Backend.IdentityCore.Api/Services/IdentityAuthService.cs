using Cheetah.Backend.IdentityCore.Api.Services.Abstractions;

namespace Cheetah.Backend.IdentityCore.Api.Services;

public class IdentityAuthService<TIdentityUser, TIdentityRole> : IAuthService
    where TIdentityUser : Cheetah.Backend.IdentityCore.Domain.IdentityUser<TIdentityRole>
    where TIdentityRole : Cheetah.Backend.IdentityCore.Domain.IdentityRole
{
    private readonly IdentitySignInManager<TIdentityUser, TIdentityRole> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public IdentityAuthService(IdentitySignInManager<TIdentityUser, TIdentityRole> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CrmSignInResult> SignIn(string userName, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(userName, password, false, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return CrmSignInResult.Success;
        }
        else if (result.IsLockedOut)
        {
            // Обработка ситуации, когда учетная запись заблокирована
            return CrmSignInResult.LockedOut;
        }
        else if (result.RequiresTwoFactor)
        {
            // Обработка ситуации, когда требуется двухфакторная аутентификация
            return CrmSignInResult.RequiresTwoFactor;
        }
        else if (result.IsNotAllowed)
        {
            // Обработка ситуации, когда пользователь не разрешен для входа
            return CrmSignInResult.NotAllowed;
        }

        // Обработка других возможных сценариев, когда вход не выполнен
        return CrmSignInResult.InvalidCredentials;
    }

    
}