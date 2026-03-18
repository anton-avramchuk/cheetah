using Cheetah.Contracts.Requests;
using Cheetah.Core.Grid;

namespace __Prefix__.ModuleName.Contracts.Requests;

public record GetSampleEntitiesGridRequest(
    int Page = 1,
    int PageSize = 20,
    string? Sort = null,
    string? Filter = null) : IGridRequest;
