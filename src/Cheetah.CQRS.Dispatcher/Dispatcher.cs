using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.CQRS.Dispatcher;

[Export(LifetimeType.Scoped)]
public sealed class Dispatcher(IServiceProvider services) : IDispatcher
{
    public async ValueTask SendAsync<TCommand>(TCommand command, CancellationToken ct)
        where TCommand : ICommand
    {
        var handler = services.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.HandleAsync(command, ct);
    }

    public async ValueTask<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken ct)
        where TCommand : ICommand<TResult>
    {
        var handler = services.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await handler.HandleAsync(command, ct);
    }

    public async ValueTask<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct)
        where TQuery : IQuery<TResult>
    {
        var handler = services.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        return await handler.HandleAsync(query, ct);
    }
}