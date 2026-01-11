using Cheetah.Core.Specification;
using Cheetah.Identity.Domain.Entities;

namespace Cheetah.Identity.Domain.Repositories;

/// <summary>
/// Repository interface for Role entity
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets a role by ID
    /// </summary>
    ValueTask<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a role by name (normalized)
    /// </summary>
    ValueTask<Role?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets a role by specification
    /// </summary>
    ValueTask<Role?> GetBySpecAsync(ISpecification<Role> spec, CancellationToken ct = default);

    /// <summary>
    /// Gets a role by specification with includes
    /// </summary>
    ValueTask<Role?> GetBySpecWithIncludesAsync(
        ISpecification<Role> spec,
        bool includeClaims = false,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all roles matching specification
    /// </summary>
    ValueTask<List<Role>> GetAllAsync(ISpecification<Role>? spec = null, CancellationToken ct = default);

    /// <summary>
    /// Gets all roles matching specification with includes
    /// </summary>
    ValueTask<List<Role>> GetAllWithIncludesAsync(
        ISpecification<Role>? spec = null,
        bool includeClaims = false,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a role exists by specification
    /// </summary>
    ValueTask<bool> ExistsAsync(ISpecification<Role> spec, CancellationToken ct = default);

    /// <summary>
    /// Adds a new role
    /// </summary>
    void Add(Role role);

    /// <summary>
    /// Updates an existing role
    /// </summary>
    void Update(Role role);

    /// <summary>
    /// Deletes a role
    /// </summary>
    void Delete(Role role);

    /// <summary>
    /// Saves all changes
    /// </summary>
    ValueTask<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets queryable for projections and complex queries (read-only)
    /// </summary>
    IQueryable<Role> AsQueryable();

    /// <summary>
    /// Gets queryable with AsNoTracking for projections (read-only)
    /// </summary>
    IQueryable<Role> AsNoTrackingQueryable();
}
