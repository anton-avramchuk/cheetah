using Cheetah.Modules.Identity.Domain;

namespace AppName.Identity.Domain;

public class AppNameIdentityUser : IdentityUser<AppNameIdentityRole>
{
    private AppNameIdentityUser() { }
    private AppNameIdentityUser(Guid id, string userName, string email) : base(id, userName, email) { }

    public static AppNameIdentityUser Create(string userName, string email) => new(Guid.NewGuid(), userName, email);
}
