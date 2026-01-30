namespace Cheetah.Blazor.Components.Crud;

/// <summary>
/// Base interface for grid view models.
/// Provides identifier for edit/delete operations.
/// </summary>
public interface IGridViewModel
{
    /// <summary>
    /// Unique identifier of the record.
    /// </summary>
    Guid Id { get; }
}

/// <summary>
/// Base interface for grid view models with typed identifier.
/// </summary>
/// <typeparam name="TKey">Type of the identifier.</typeparam>
public interface IGridViewModel<TKey> where TKey : notnull
{
    /// <summary>
    /// Unique identifier of the record.
    /// </summary>
    TKey Id { get; }
}
