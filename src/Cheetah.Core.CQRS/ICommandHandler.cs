namespace Cheetah.Core.CQRS;

/// <summary>
/// Handler for commands with result
/// </summary>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    ValueTask<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Handler for commands without result
/// </summary>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    ValueTask HandleAsync(TCommand command, CancellationToken ct = default);
}