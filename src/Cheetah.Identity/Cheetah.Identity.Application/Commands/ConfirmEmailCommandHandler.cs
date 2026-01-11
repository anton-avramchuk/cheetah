using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Identity.Domain.Repositories;

namespace Cheetah.Identity.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<ConfirmEmailCommand>))]
public class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IEventBus _eventBus;

    public ConfirmEmailCommandHandler(IUserRepository userRepository, IEventBus eventBus)
    {
        _userRepository = userRepository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(ConfirmEmailCommand command, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId, ct);
        if (user == null)
            throw new InvalidOperationException($"User {command.UserId} not found");

        // Confirm email
        user.ConfirmEmail();

        await _userRepository.SaveChangesAsync(ct);

        // Publish domain events
        foreach (var domainEvent in user.DomainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, ct);
        }
        user.ClearDomainEvents();
    }
}
