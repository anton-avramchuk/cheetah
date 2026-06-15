using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Deals.Application.Deals;
using Cheetah.Modules.Deals.Application.Pipelines;
using Cheetah.Modules.Deals.Contracts;
using Mapster;

namespace Cheetah.Modules.Deals.Api.Mapping;

/// <summary>
/// Маппинг HTTP-реквестов в CQRS-команды/запросы. Имена полей реквестов и команд совпадают —
/// Mapster соединяет их по соглашению. Ответы (DTO) совпадают с результатами обработчиков по типу.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public sealed class DealsMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        // Сделки
        config.NewConfig<CreateDealRequest, CreateDealCommand>();
        config.NewConfig<GetDealByIdRequest, GetDealByIdQuery>();
        config.NewConfig<ListDealsRequest, ListDealsQuery>();
        config.NewConfig<ChangeDealStageRequest, ChangeDealStageCommand>();
        config.NewConfig<WinDealRequest, WinDealCommand>();
        config.NewConfig<LoseDealRequest, LoseDealCommand>();
        config.NewConfig<AssignDealOwnerRequest, AssignDealOwnerCommand>();
        config.NewConfig<GetDealHistoryRequest, GetDealHistoryQuery>();
        config.NewConfig<GetDealBoardRequest, GetDealBoardQuery>();

        // Воронки
        config.NewConfig<CreatePipelineRequest, CreatePipelineCommand>();
        config.NewConfig<GetPipelineByIdRequest, GetPipelineByIdQuery>();
        config.NewConfig<ListPipelinesRequest, ListPipelinesQuery>();
        config.NewConfig<AddPipelineStageRequest, AddPipelineStageCommand>();
    }
}
