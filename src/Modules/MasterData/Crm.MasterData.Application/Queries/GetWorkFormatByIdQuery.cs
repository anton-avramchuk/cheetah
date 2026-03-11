using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetWorkFormatByIdQuery(Guid Id) : IQuery<WorkFormatModel?>;
