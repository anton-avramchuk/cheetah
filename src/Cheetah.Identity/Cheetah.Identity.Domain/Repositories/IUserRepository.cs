using Cheetah.Core.Specification;
using Cheetah.Identity.Domain.Entities;

namespace Cheetah.Identity.Domain.Repositories;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    ValueTask<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by email (normalized)
    /// </summary>
    ValueTask<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by specification
    /// </summary>
    ValueTask<User?> GetBySpecAsync(ISpecification<User> spec, CancellationToken ct = default);

    /// <summary>
    /// Gets a user by specification with includes
    /// </summary>
    ValueTask<User?> GetBySpecWithIncludesAsync(
        ISpecification<User> spec,
        bool includeRoles = false,
        bool includeClaims = false,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all users matching specification
    /// </summary>
    ValueTask<List<User>> GetAllAsync(ISpecification<User>? spec = null, CancellationToken ct = default);

    /// <summary>
    /// Gets all users matching specification with includes
    /// </summary>
    ValueTask<List<User>> GetAllWithIncludesAsync(
        ISpecification<User>? spec = null,
        bool includeRoles = false,
        bool includeClaims = false,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a user exists by specification
    /// </summary>
    ValueTask<bool> ExistsAsync(ISpecification<User> spec, CancellationToken ct = default);

    /// <summary>
    /// Adds a new user
    /// </summary>
    void Add(User user);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    void Update(User user);

    /// <summary>
    /// Deletes a user
    /// </summary>
    void Delete(User user);

    /// <summary>
    /// Saves all changes
    /// </summary>
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets queryable for projections and complex queries (read-only)
    /// </summary>
    IQueryable<User> AsQueryable();

    /// <summary>
    /// Gets queryable with AsNoTracking for projections (read-only)
    /// </summary>
    IQueryable<User> AsNoTrackingQueryable();
}
