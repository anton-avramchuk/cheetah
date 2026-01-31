using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Admin.Modules.Clients.Domain.Repositories;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;

namespace Cheetah.Admin.Modules.Clients.Application.Commands;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<DeleteTariffCommand>))]
public class DeleteTariffCommandHandler : ICommandHandler<DeleteTariffCommand>
{
    private readonly ITariffRepository _repository;

    public DeleteTariffCommandHandler(ITariffRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask HandleAsync(DeleteTariffCommand command, CancellationToken ct = default)
    {
        var entity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw EntityNotFoundException.For<Tariff>(command.Id);

        _repository.Delete(entity);
        await _repository.SaveChangesAsync(ct);
    }
}
