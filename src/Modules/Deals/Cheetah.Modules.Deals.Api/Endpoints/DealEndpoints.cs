using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Deals.Application.Deals;
using Cheetah.Modules.Deals.Contracts;

namespace Cheetah.Modules.Deals.Api.Endpoints;

/// <summary>POST api/deals — создать сделку.</summary>
public sealed class CreateDealEndpoint : CreateCommandEndpoint<CreateDealRequest, CreateDealCommand>
{
    public override string Route => "api/deals";
    public override string GetByIdRouteName => "GetDeal";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("CreateDeal").WithTags("Deals");
}

/// <summary>GET api/deals/{dealId} — сделка по идентификатору.</summary>
public sealed class GetDealByIdEndpoint
    : QueryOrNotFoundEndpoint<GetDealByIdRequest, GetDealByIdQuery, DealDto, DealDto>
{
    public override string Route => "api/deals/{dealId:guid}";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetDeal").WithTags("Deals");
}

/// <summary>GET api/deals — список сделок с фильтрами.</summary>
public sealed class ListDealsEndpoint
    : QueryCollectionEndpoint<ListDealsRequest, ListDealsQuery, DealListItemDto, DealListItemDto>
{
    public override string Route => "api/deals";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("ListDeals").WithTags("Deals");
}

/// <summary>GET api/deals/board — данные Kanban-доски воронки.</summary>
public sealed class GetDealBoardEndpoint
    : QueryEndpoint<GetDealBoardRequest, GetDealBoardQuery, BoardDto, BoardDto>
{
    public override string Route => "api/deals/board";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetDealBoard").WithTags("Deals");
}

/// <summary>GET api/deals/{dealId}/history — история переходов сделки.</summary>
public sealed class GetDealHistoryEndpoint
    : QueryCollectionEndpoint<GetDealHistoryRequest, GetDealHistoryQuery, DealHistoryDto, DealHistoryDto>
{
    public override string Route => "api/deals/{dealId:guid}/history";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("GetDealHistory").WithTags("Deals");
}

/// <summary>POST api/deals/{dealId}/stage — сменить стадию сделки.</summary>
public sealed class ChangeDealStageEndpoint : CommandEndpoint<ChangeDealStageRequest, ChangeDealStageCommand>
{
    public override string Route => "api/deals/{dealId:guid}/stage";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("ChangeDealStage").WithTags("Deals");
}

/// <summary>POST api/deals/{dealId}/win — выиграть сделку.</summary>
public sealed class WinDealEndpoint : CommandEndpoint<WinDealRequest, WinDealCommand>
{
    public override string Route => "api/deals/{dealId:guid}/win";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("WinDeal").WithTags("Deals");
}

/// <summary>POST api/deals/{dealId}/lose — проиграть сделку.</summary>
public sealed class LoseDealEndpoint : CommandEndpoint<LoseDealRequest, LoseDealCommand>
{
    public override string Route => "api/deals/{dealId:guid}/lose";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("LoseDeal").WithTags("Deals");
}

/// <summary>POST api/deals/{dealId}/owner — сменить ответственного.</summary>
public sealed class AssignDealOwnerEndpoint : CommandEndpoint<AssignDealOwnerRequest, AssignDealOwnerCommand>
{
    public override string Route => "api/deals/{dealId:guid}/owner";

    protected override void Configure(EndpointConfiguration config)
        => config.WithName("AssignDealOwner").WithTags("Deals");
}
