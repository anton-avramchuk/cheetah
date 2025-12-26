namespace Cheetah.Core.CQRS;

/// <summary>
/// Lightweight mediator for dispatching commands and queries
/// </summary>
public interface IDispatcher
{
    ValueTask SendAsync<TCommand>(TCommand command, CancellationToken ct = default) 
        where TCommand : ICommand;
    
    ValueTask<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken ct = default) 
        where TCommand : ICommand<TResult>;
    
    ValueTask<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct = default) 
        where TQuery : IQuery<TResult>;
}