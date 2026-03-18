using Cheetah.Modules.Identity.Domain;

namespace AppName.Identity.Domain;

public class AppNameIdentityRole : IdentityRole
{
    private AppNameIdentityRole() { }
    private AppNameIdentityRole(Guid id, string name) : base(id, name) { }

    public static AppNameIdentityRole Create(string name) => new(Guid.NewGuid(), name);
    public static AppNameIdentityRole Create(Guid id, string name) => new(id, name);
}
