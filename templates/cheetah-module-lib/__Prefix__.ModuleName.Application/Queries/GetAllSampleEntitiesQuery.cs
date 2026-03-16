using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;

namespace __Prefix__.ModuleName.Application.Queries;

public record GetAllSampleEntitiesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    string? Filter = null) : IGridRequest, IQuery<GridResult<SampleEntityModel>>;
