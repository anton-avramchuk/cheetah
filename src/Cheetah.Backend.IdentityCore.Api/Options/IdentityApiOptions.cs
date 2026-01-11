namespace Cheetah.Backend.IdentityCore.Api.Options;

public abstract class IdentityApiOptions<TIdentityUsersOptions, TIdentityRolesOptions>
    where TIdentityUsersOptions : IdentityUsersOptions
    where TIdentityRolesOptions : IdentityRolesOptions
{
    public TIdentityUsersOptions? Users { get; set; }

    public IdentityRolesOptions? Roles { get; set; }
}

public abstract class IdentityUsersOptions
{
}

public abstract class IdentityRolesOptions
{
}