using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Activities.Application.Exceptions;
using Cheetah.Modules.Activities.Contracts;
using Cheetah.Modules.Activities.Domain.Entities;

namespace Cheetah.Modules.Activities.Application.Activities;

/// <summary>Обновить базовые поля активности (заголовок, описание, приоритет, срок).</summary>
public sealed record UpdateActivityCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateActivityRequestBase;

public class UpdateActivityCommandHandler<TActivity, TUpdateRequest>
    : ICommandHandler<UpdateActivityCommand<TUpdateRequest>>
    where TActivity : ActivityBase
    where TUpdateRequest : UpdateActivityRequestBase
{
    private readonly IRepository<TActivity, Guid> _repository;

    public UpdateActivityCommandHandler(IRepository<TActivity, Guid> repository)
        => _repository = repository;

    public async ValueTask HandleAsync(UpdateActivityCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var activity = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new ActivityValidationException($"Activity '{command.Id}' not found");

        activity.Update(
            command.Request.Title, command.Request.Description, command.Request.Priority, command.Request.DueAt);
        await _repository.SaveChangesAsync(ct);
    }
}
