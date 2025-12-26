namespace Cheetah.Core.CQRS;

/// <summary>
/// Marker interface for commands that do not return a result
/// </summary>
public interface ICommand { }

/// <summary>
/// Marker interface for commands that return a result
/// </summary>
public interface ICommand<TResult> { }