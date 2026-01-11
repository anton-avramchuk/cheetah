using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Identity.Domain.Entities;

namespace Cheetah.Identity.Domain.Specifications;

/// <summary>
/// Specification for finding user by email
/// </summary>
public class UserByEmailSpecification : Specification<User>
{
    private readonly string _normalizedEmail;

    public UserByEmailSpecification(string email)
    {
        _normalizedEmail = email.ToUpperInvariant();
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.NormalizedEmail == _normalizedEmail;
    }
}

/// <summary>
/// Specification for finding user by ID
/// </summary>
public class UserByIdSpecification : Specification<User>
{
    private readonly Guid _userId;

    public UserByIdSpecification(Guid userId)
    {
        _userId = userId;
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.Id == _userId;
    }
}

/// <summary>
/// Specification for finding active users
/// </summary>
public class ActiveUsersSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.IsActive;
    }
}

/// <summary>
/// Specification for finding users with confirmed email
/// </summary>
public class EmailConfirmedUsersSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.EmailConfirmed;
    }
}

/// <summary>
/// Specification for finding users by username
/// </summary>
public class UserByUsernameSpecification : Specification<User>
{
    private readonly string _normalizedUsername;

    public UserByUsernameSpecification(string username)
    {
        _normalizedUsername = username.ToUpperInvariant();
    }

    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.NormalizedUserName == _normalizedUsername;
    }
}
