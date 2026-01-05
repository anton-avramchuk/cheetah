using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Features.Domain.Entities;

namespace Cheetah.Features.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateFeatureCommand, string>))]
public class CreateFeatureCommandHandler(
    IRepository<Feature, string> repository,
    IEventBus eventBus
) : ICommandHandler<CreateFeatureCommand, string>
{
    public async ValueTask<string> HandleAsync(CreateFeatureCommand command, CancellationToken cancellationToken = default)
    {
        var feature = Feature.Create(
            command.Id,
            command.DisplayName,
            command.Description,
            command.IsEnabledByDefault,
            command.Group
        );

        await repository.InsertAsync(feature, cancellationToken);

        foreach (var domainEvent in feature.DomainEvents)
        {
            await eventBus.PublishAsync(domainEvent, cancellationToken);
        }
        feature.ClearDomainEvents();

        return feature.Id;
    }
}
