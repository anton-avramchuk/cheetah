using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Modules.Deals.Domain.Abstractions;
using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Application.Pipelines;

// ── Создать воронку ──────────────────────────────────────────────────────────────────────

public sealed record CreatePipelineCommand(string Name, bool IsDefault) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreatePipelineCommand, Guid>))]
public sealed class CreatePipelineCommandHandler : ICommandHandler<CreatePipelineCommand, Guid>
{
    private readonly IRepository<Pipeline, Guid> _pipelines;
    public CreatePipelineCommandHandler(IRepository<Pipeline, Guid> pipelines) => _pipelines = pipelines;

    public async ValueTask<Guid> HandleAsync(CreatePipelineCommand command, CancellationToken ct = default)
    {
        var pipeline = Pipeline.Create(command.Name, command.IsDefault);
        _pipelines.Add(pipeline);
        await _pipelines.SaveChangesAsync(ct);
        return pipeline.Id;
    }
}

// ── Добавить стадию ──────────────────────────────────────────────────────────────────────

public sealed record AddPipelineStageCommand(
    Guid PipelineId, string Name, int Order, int Probability, StageType Type) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AddPipelineStageCommand, Guid>))]
public sealed class AddPipelineStageCommandHandler : ICommandHandler<AddPipelineStageCommand, Guid>
{
    private readonly IPipelineRepository _pipelines;
    public AddPipelineStageCommandHandler(IPipelineRepository pipelines) => _pipelines = pipelines;

    public async ValueTask<Guid> HandleAsync(AddPipelineStageCommand command, CancellationToken ct = default)
    {
        var pipeline = await _pipelines.GetWithStagesAsync(command.PipelineId, ct)
            ?? throw EntityNotFoundException.For<Pipeline>(command.PipelineId);

        var stage = pipeline.AddStage(command.Name, command.Order, command.Probability, command.Type);
        _pipelines.Update(pipeline);
        await _pipelines.SaveChangesAsync(ct);
        return stage.Id;
    }
}
