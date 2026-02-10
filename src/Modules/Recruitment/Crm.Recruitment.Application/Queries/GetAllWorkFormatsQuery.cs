using Cheetah.Core.CQRS;

namespace Crm.Recruitment.Application.Queries;

public record GetAllWorkFormatsQuery : IQuery<IReadOnlyList<WorkFormatModel>>;
