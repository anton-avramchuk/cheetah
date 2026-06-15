using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Deals.Application.Pipelines;
using Cheetah.Modules.Deals.Contracts;

namespace Cheetah.Modules.Deals.Api.Endpoints;

/// <summary>POST api/pipelines — создать воронку.</summary>
public sealed class CreatePipelineEndpoint : CreateCommandEndpoint<CreatePipelineRequest, CreatePipelineCommand>
{
    public override string Route => "api/pipelines";
    public override string GetByIdRouteName => "GetPipeline";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("CreatePipeline").WithTags("Pipelines");
}

/// <summary>GET api/pipelines/{pipelineId} — воронка со стадиями.</summary>
public sealed class GetPipelineByIdEndpoint
    : QueryOrNotFoundEndpoint<GetPipelineByIdRequest, GetPipelineByIdQuery, PipelineDto, PipelineDto>
{
    public override string Route => "api/pipelines/{pipelineId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetPipeline").WithTags("Pipelines");
}

/// <summary>GET api/pipelines — список воронок.</summary>
public sealed class ListPipelinesEndpoint
    : QueryCollectionEndpoint<ListPipelinesRequest, ListPipelinesQuery, PipelineDto, PipelineDto>
{
    public override string Route => "api/pipelines";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("ListPipelines").WithTags("Pipelines");
}

/// <summary>POST api/pipelines/{pipelineId}/stages — добавить стадию.</summary>
public sealed class AddPipelineStageEndpoint
    : CreateCommandEndpoint<AddPipelineStageRequest, AddPipelineStageCommand>
{
    public override string Route => "api/pipelines/{pipelineId:guid}/stages";
    public override string GetByIdRouteName => "GetPipeline";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("AddPipelineStage").WithTags("Pipelines");
}
