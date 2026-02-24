namespace Cheetah.Frontend.Auth;

public interface IAuthStateNotifier
{
    Task NotifyLoginAsync(string token);
    Task NotifyLogoutAsync();
}
