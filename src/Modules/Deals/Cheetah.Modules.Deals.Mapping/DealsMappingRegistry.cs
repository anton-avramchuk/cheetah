using Cheetah.Mapping.Core;
using Cheetah.Modules.Deals.Application.Deals;
using Cheetah.Modules.Deals.Application.Pipelines;
using Cheetah.Modules.Deals.Contracts;

namespace Cheetah.Modules.Deals.Mapping;

/// <summary>
/// Реестр генерируемых мапперов Deals (аналог Mapster-профиля, но через source generator).
/// Маркер размещён в выделенной маппинг-сборке, поэтому Contracts остаётся чистым. Все маппинги —
/// Request → Command/Query, проекция не нужна (<c>GenerateProjection = false</c>).
/// </summary>
// Сделки
[GenerateMapper(typeof(CreateDealRequest), typeof(CreateDealCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetDealByIdRequest), typeof(GetDealByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(ListDealsRequest), typeof(ListDealsQuery), GenerateProjection = false)]
[GenerateMapper(typeof(ChangeDealStageRequest), typeof(ChangeDealStageCommand), GenerateProjection = false)]
[GenerateMapper(typeof(WinDealRequest), typeof(WinDealCommand), GenerateProjection = false)]
[GenerateMapper(typeof(LoseDealRequest), typeof(LoseDealCommand), GenerateProjection = false)]
[GenerateMapper(typeof(AssignDealOwnerRequest), typeof(AssignDealOwnerCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetDealHistoryRequest), typeof(GetDealHistoryQuery), GenerateProjection = false)]
[GenerateMapper(typeof(GetDealBoardRequest), typeof(GetDealBoardQuery), GenerateProjection = false)]
// Воронки
[GenerateMapper(typeof(CreatePipelineRequest), typeof(CreatePipelineCommand), GenerateProjection = false)]
[GenerateMapper(typeof(GetPipelineByIdRequest), typeof(GetPipelineByIdQuery), GenerateProjection = false)]
[GenerateMapper(typeof(ListPipelinesRequest), typeof(ListPipelinesQuery), GenerateProjection = false)]
[GenerateMapper(typeof(AddPipelineStageRequest), typeof(AddPipelineStageCommand), GenerateProjection = false)]
public static partial class DealsMappingRegistry
{
}
