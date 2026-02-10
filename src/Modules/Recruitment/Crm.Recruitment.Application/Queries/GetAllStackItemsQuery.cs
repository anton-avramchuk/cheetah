using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetAllStackItemsQuery : IQuery<IReadOnlyList<StackItemModel>>;
