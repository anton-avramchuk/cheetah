using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetIndustryByIdQuery(Guid Id) : IQuery<IndustryModel?>;
