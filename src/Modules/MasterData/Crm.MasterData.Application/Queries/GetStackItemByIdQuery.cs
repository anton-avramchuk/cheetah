using Cheetah.Core.CQRS;

namespace Crm.MasterData.Application.Queries;

public record GetStackItemByIdQuery(Guid Id) : IQuery<StackItemModel?>;